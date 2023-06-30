using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System;

public class SimplePoint : MonoBehaviour
{
    [SerializeField] Text TXT_point;

    public void SetPoint(Vector3 pos,  Color color)
    {
        transform.position = pos;
        TXT_point.text = $"({transform.position.x},{transform.position.y},{transform.position.z})";
        GetComponent<SpriteRenderer>().color = color;

        Observable.Timer(TimeSpan.FromSeconds(3))
                  .Subscribe(_=>
                  { 
                    Destroy(this.gameObject); 
                  })
                  .AddTo(this);
    }
}
