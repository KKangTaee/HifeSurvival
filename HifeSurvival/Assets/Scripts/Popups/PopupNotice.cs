using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using System;

[Popup(PATH_IN_RESOURCES_FOLDER = "Prefabs/Popups/PopupNotice/PopupNotice",
       IN_RESOURCES_FORLDER = true)]
public class PopupNotice : PopupBase
{
    [Header("[PopupNotice]")]
    [SerializeField] TMP_Text TMP_desc;

    private IDisposable _timerDisposable;

    protected override void OnButtonEvent(Button inButton)
    {
        throw new System.NotImplementedException();
    }

    public void SetDesc(string inDesc)
    {
        StopTimer();

        _timerDisposable = Observable.Timer(TimeSpan.FromSeconds(3))
                                      .Subscribe(_=>
                                      {
                                            Close();
                                      });
    }

    public void StopTimer()
    {
        if(_timerDisposable != null)
        {
            _timerDisposable.Dispose();
            _timerDisposable = null;
        }
    }
}
