using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
public class Database_Manager
{
    public static Database_Manager _DATABASE_MANAGER;
    //Connection parameters
    const string connectionString = "Server=db4free.net;Port=3306;database=m17uf3unity;Uid=joanvallbonaenti;password=dbc9969a;SSL Mode=None;connect timeout=3600;default command timeout=3600;";
    MySqlConnection conn;
    MySqlDataReader reader;
    MySqlCommand cmd;

    //Database lists
    List<Users> users;

    //Class Constructor
    public Database_Manager()
    {
        conn = new MySqlConnection(connectionString);
    }

    private List<Users> findNicknames(string name)
    {
        string query = "SELECT * FROM Users WHERE name = \"";
        query += name;
        query += "\"";

        cmd = conn.CreateCommand();
        cmd.CommandText = query;
        reader = cmd.ExecuteReader();

        List<Users> res = new List<Users>();
        while (reader.Read())
        {
            res.Add(new Users(reader));
        }
        reader.Close();
        return res;
    }
    private List<Users> findUsers(string name, string passwd)
    {
        string query = "SELECT * FROM Users WHERE name = \"";
        query += name;
        query += "\" AND passwd = \"";
        query += passwd;
        query += "\";";

        cmd = conn.CreateCommand();
        cmd.CommandText = query;
        reader = cmd.ExecuteReader();

        List<Users> res = new List<Users>();
        while (reader.Read())
        {
            Users user = new Users(reader);
            res.Add(user);
        }
        reader.Close();
        return res;
    }
    private void insertUser(string name, string passwd, int id_race_user)
    {
        string query = "INSERT INTO Users(name, passwd, id_race_user) VALUES(\"";
        query += name;
        query += "\", \"";
        query += passwd;
        query += "\",";
        query += id_race_user;
        query += ");";

        cmd = conn.CreateCommand();
        cmd.CommandText = query;
        reader = cmd.ExecuteReader();

        reader.Close();
    }
    public List<Race> getRaces()
    {
        string query = "SELECT * FROM Races;";

        cmd = conn.CreateCommand();
        cmd.CommandText = query;
        reader = cmd.ExecuteReader();

        List<Race> res = new List<Race>();
        while (reader.Read())
        {
            res.Add(new Race(reader));
        }
        reader.Close();
        return res;
    }
    public void OpenConnection()
    {
        //Try to open connection
        try
        {
            conn.Open();
            if (_DATABASE_MANAGER == null) { _DATABASE_MANAGER = this; }
        }
        catch (Exception ex) { Console.WriteLine(ex.Message); }
    }
    public void CloseConnection()
    {
        //Close connection
        conn.Close();
    }
    /*Users*/
    public List<Users> FindUsernames(string name) { return findNicknames(name); }

    public List<Users> FindUsers(string name, string passwd) { return findUsers(name, passwd); }

    public void InsertUser(string name, string passwd, int id_race_user) { insertUser(name, passwd, id_race_user); }

    public List<Race> GetRaces() { return getRaces(); }
}
