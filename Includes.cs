using System;
using System.Data;
using System.Data.Common;
public struct Users
{
    public int id_user;
    public string name;
    public string passwd;
    public int id_race_user;

    public Users(IDataReader dataReader)
    {
        id_user = dataReader.GetInt32(0);
        name = dataReader.GetString(1);
        passwd = dataReader.GetString(2);
        id_race_user = dataReader.GetInt32(3);
    }
}
public struct Race
{
    public int id_race;
    public string name;
    public int maxHealth;
    public int damage;
    public int velocity;
    public int jumpForce;
    public int cadence;

    public Race(IDataReader dataReader)
    {
        id_race = dataReader.GetInt32(0);
        name = dataReader.GetString(1);
        maxHealth = dataReader.GetInt32(2);
        damage = dataReader.GetInt32(3);
        velocity = dataReader.GetInt32(4);
        jumpForce = dataReader.GetInt32(5);
        cadence = dataReader.GetInt32(6);
    }
}