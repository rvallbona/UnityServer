using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;


public class Network_Manager
{
    private List<Client> clients;
    private TcpListener serverListener;
    private Mutex clientListMutex;

    private int lastTimePing;
    private List<Client> disconnectClients;

    public Network_Manager()
    {
        int port = 6543;

        //Lista de clientes que se van conectando al servidor
        this.clients = new List<Client>();

        //Instanciamos la clase de listener para que escuche cualquier IP por el puerto 6543
        this.serverListener = new TcpListener(IPAddress.Any, port);

        //Instanciamos el lister para evitar problemas de memoria compartida
        this.clientListMutex = new Mutex();

        //Creamos una variable a modo de temporizador para enviar pings
        this.lastTimePing = Environment.TickCount;

        //Lista de clientes pendientes de desconexion
        this.disconnectClients = new List<Client>();
    }
    //Función para iniciar el listener
    public void Start_Network_Service()
    {
        try
        {
            //Iniciamos el listener para prepararlo para la escucha de peticiones
            this.serverListener.Start();
            StartListening();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
    //Función para acepatar conexiones TCP
    private void StartListening()
    {
        Console.WriteLine("Esperando nueva conexión");
        //Cada vez que nos llece una solicitud de conexion llamaremos de forma asincrona a la funcion de Callback AcceptConnection (automaticamente genera un thread)
        this.serverListener.BeginAcceptTcpClient(AcceptConnection, this.serverListener);
    }
    private void AcceptConnection(IAsyncResult ar)
    {
        Console.WriteLine("Recibo una conexion");

        //Almacenamos los datos de la peticion entrante
        TcpListener listener = (TcpListener)ar.AsyncState;

        //Bloqueo el mutex ya que la lista de clientes es usada en el thread principal del programa y en esta funcion que es llamada de forma asincrona cada vez que hay una peticion de conexion
        clientListMutex.WaitOne();

        //Añado la conexion en la lista
        this.clients.Add(new Client(listener.EndAcceptTcpClient(ar)));

        //Libero el mutex
        clientListMutex.ReleaseMutex();

        //Vuelvo a escuchar nuevas peticiones
        StartListening();
    }
    public void CheckMessage()
    {
        //Dado que voy a recorrer la lista de clientes bloqueo las peticiones
        clientListMutex.WaitOne();

        //Recorro los clientes conectados
        foreach (Client client in this.clients)
        {
            //Obtento el streaming de datos, es decir, el envio y recepcion de datos
            NetworkStream netStream = client.GetTcpClient().GetStream();

            //Comprobamos si la información esta lista para ser leida (puede ser enviada parcialmente, corrompida...)
            if (netStream.DataAvailable)
            {
                //Instancio el reader para leer los datos recibidos
                StreamReader reader = new StreamReader(netStream, true);

                //Leo una linea de datos, cada readline lee un writeline recibido
                string data = reader.ReadLine();

                //Solo leo datos si no son nulos
                if (data != null)
                {
                    //Gestiono los valores
                    ManageData(client, data);
                }
            }
        }
        //Libero el mutex
        clientListMutex.ReleaseMutex();
    }
    public void CheckConnection()
    {
        //Revisamos conexiones cada cierto tiempo, en este caso 5000 milisegundos
        if (Environment.TickCount - this.lastTimePing > 5000)
        {
            //Bloqueo el mutex para poder leer la lista de clientes conectados
            clientListMutex.WaitOne();

            //Recorremos los clientes conectados
            foreach (Client client in this.clients)
            {
                //Si le habiamos enviado un ping le añadimos a la lista de usuarios a desconectar (todavia no lo desconecto)
                if (client.GetWaitingPing() == true)
                {
                    disconnectClients.Add(client);
                }
                //Si no habiamos enviado ping le enviamos uno
                else
                {
                    //Enviamos ping para confirmar que sigue conectado
                    SendPing(client);
                }
            }

            //Almacenamos el tiempo actual del ultimo envio de ping
            this.lastTimePing = Environment.TickCount;

            //Liberamos mutex
            clientListMutex.ReleaseMutex();
        }
    }
    private void ManageData(Client client, string data)
    {
        //Separo los datos enviados, primero el comando y luego los parametros adicionales
        string[] parameters = data.Split('/');

        switch (parameters[0])
        {
            //Hemos definido que el 0 significa login y recibe dos parametros mas (usuario y contraseña)
            case "0":
                Login(parameters[1], parameters[2], client);
                break;
            case "1":
                //Hemos definido que el 1 es la respuesta del ping y no recibe mas parametros
                ReceivePing(client);
                break;
            case "2":
                int id_race = Int32.Parse(parameters[3]);
                Register(parameters[1], parameters[2], id_race, client);
                break;
            case "3":
                GetRaces(client);
                break;
        }
    }
    private void Login(string name, string passwd, Client client)
    {
        //Verificariamos los datos de login

        List<Users> users = Database_Manager._DATABASE_MANAGER.FindUsers(name, passwd);

        if (users.Count != 1)
        {
            StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());

            writer.WriteLine("IncorrectLogin");
            writer.Flush();
            //writer.Close();
        }
        else
        {
            StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());

            writer.WriteLine("CorrectLogin/" + users[0].id_user + "/" + users[0].name + "/" + users[0].passwd + "/" + users[0].id_race_user);
            writer.Flush();
            //writer.Close();
        }
    }
    private void Register(string name, string password, int id_race_user, Client client)
    {
        //Verificariamos los datos de register

        Console.WriteLine("Registro de: " + name + " usando la passwd: " + password + " con el id_race: " + id_race_user);

        try
        {
            StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());

            Database_Manager._DATABASE_MANAGER.InsertUser(name, password, id_race_user);

            //Enviamos la información
            writer.WriteLine("CorrectRegister");
            writer.Flush();
        }
        catch (Exception ex)
        {
            Console.WriteLine("Registro error: " + ex.Message + " con el cliente: " + client.GetName());
            client.GetName();
        }
    }
    private void GetRaces(Client client)
    {
        List<Race> races = Database_Manager._DATABASE_MANAGER.getRaces();

        foreach (Race race in races)
        {
            StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());

            writer.WriteLine("CorrectRaces/" + race.id_race + "/" + race.name + "/" + race.maxHealth + "/" + race.damage + "/" + race.velocity + "/" + race.jumpForce + "/" + race.cadence +  "/" + races.Count());
            writer.Flush();
        }
    }
    private void SendPing(Client client)
    {
        try
        {
            //Instanciamos el writer para enviar el mensaje de ping
            StreamWriter writer = new StreamWriter(client.GetTcpClient().GetStream());

            //Enviamos el ping
            writer.WriteLine("ping");

            //Limpiamos el bufer de envio para evitar que siga acumulando datos
            writer.Flush();

            Console.WriteLine("Ping realizado");

            //Asignamos a true la variable de ping propia del cliente para identificar que le hemos enviado un ping para la siguiente iteracion
            client.SetWaitingPing(true);
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message + " with client" + client.GetName());
        }
    }
    //Al recibir un ping asignamos la variable del ping propia del cliente a false para indicar que nos ha respondido
    private void ReceivePing(Client client)
    {
        client.SetWaitingPing(false);
    }
    public void DisconnectClients()
    {

        //Bloqueo el mutex para acceder a la lista de clientes (en el remove)
        clientListMutex.WaitOne();

        //Recorro la lista de usuarios a desconectar
        foreach (Client client in this.disconnectClients)
        {
            Console.WriteLine("Desconectando usuarios");

            //Cierro la conexion antes de eliminar el cliente de la lista de conectados
            client.GetTcpClient().Close();

            //Elimino el cliente de la lista de clientes a desconectar
            this.clients.Remove(client);
        }

        //Vacio la lista de usuarios a desconectar
        this.disconnectClients.Clear();

        //Libero el mutex
        clientListMutex.ReleaseMutex();
    }
}