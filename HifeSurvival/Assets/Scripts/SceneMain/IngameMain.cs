using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IngameMain : MonoBehaviour
{
    [SerializeField] private WorldMap _worldMap;

    private void Awake()
    {
        ControllerManager.Instance.Init();
    }
}
