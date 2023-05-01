using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerData
{
    private static ServerData _instance;
    public static ServerData Instance
    {
        get
        {
            if (_instance == null)
                _instance = new ServerData();

            return _instance;
        }
    }

    public string UserID { get; private set; }
    
    public void SetUserID(string inUserID) =>
        UserID = inUserID;
}


public class User
{
    public string _id;
    public int _cash;

    public User(string inId, int inCash)
    {

    }
}
