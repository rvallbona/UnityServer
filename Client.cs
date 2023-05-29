using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
//Clase para almacenar los datos necesarios de cada cliente
class Client
{
    private TcpClient tcp; //Almacenamos los datos de conexion del cliente
    private string name; //Almacenamos el nick del cliente (en este caso hardcodeado)
    private bool waitingPing; //Almacenamos si estamos a la espera que responda un ping de conexion

    //Constructor de la clase
    public Client(TcpClient tcp)
    {
        this.tcp = tcp;
        this.name = "Guest";
        this.waitingPing = false;
    }
    //-------------------------------- Getters ----------------------
    public TcpClient GetTcpClient()
    {
        return this.tcp;
    }
    public string GetName()
    {
        return this.name;
    }
    public bool GetWaitingPing()
    {
        return this.waitingPing;
    }
    //-------------------------------- Setters ----------------------
    public void SetWaitingPing(bool waitingPing)
    {
        this.waitingPing = waitingPing;
    }
}

