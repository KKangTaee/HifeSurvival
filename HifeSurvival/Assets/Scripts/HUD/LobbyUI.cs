using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{

    [SerializeField] Button BTN_gameStart;

    private void Awake()
    {
        GetComponent<Canvas>().worldCamera = Camera.main;
    }

    public void OnButtonEvent(Button inButton)
    {
        if(inButton == BTN_gameStart)
        {
            PopupManager.Instance.Show<PopupSelectHeros>(null);
        }
    }
}
