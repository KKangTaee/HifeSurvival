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

    public User UserData { get; private set; }

    public void SetUserData(User inUser)
    {
        UserData = inUser;
    }
}


public class User
{
    public string user_id;
    public string nickname;
    public string photo_url;
}
