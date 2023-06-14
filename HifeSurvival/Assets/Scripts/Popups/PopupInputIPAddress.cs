using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class PopupInputIPAddress : PopupBase
{
    [SerializeField] Button BTN_join;
    [SerializeField] Button BTN_close;
    [SerializeField] InputField IF_inputAddress;

    private Action _joinCallback;

    protected override void OnButtonEvent(Button inButton)
    {
        if(inButton == BTN_join)
        {
            
        }
        else if(inButton == BTN_close)
        {

        }
    }

    private bool CheckIPAddress()
    {

        return true;
    }

    public void SetInfo(Action inJoinCallback)
    {
        _joinCallback = inJoinCallback;
    }
}
