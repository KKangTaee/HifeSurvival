using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class PopupAttribute : System.Attribute
{
    public string PATH_IN_NOT_RESOURCES_FOLDER;     // 리소스폴더 아닌 곳의 경로 (번들용도)
    public string PATH_IN_RESOURCES_FOLDER;         // 리소스폴더
    public bool   IN_RESOURCES_FORLDER;             // 해당 팝업이 어디 경로에 있는지 유무 체크
}


public class PopupManager : MonoBehaviour
{
    private static PopupManager _instacne;

    public static PopupManager Instance {
        get
        {
            if(_instacne == null)
                _instacne = new PopupManager();

            return _instacne;
        }
    }


    //-----------------
    // variables
    //-----------------

    Stack<PopupBase> _openedPopups      = new Stack<PopupBase>();
    Queue<Action>    _reserverPopups     = new Queue<Action>();



    //------------------
    // functions
    //------------------

    public void Show<T>(Action<T> inCreateCallback) where T : PopupBase
    {
        // 이미노출되었다면..? 열지 않는다
        if(IsExposedNow<T>() == true)
            return;

        T popup = CreatePopup<T>(inCreateCallback);

        if(popup == null)
        {
            Debug.LogError($"[{nameof(Show)}] popup object is null or empty!");
            return;
        }

        _openedPopups.Push(popup);

        if(popup is IPopupOpen iOpen)
        {
            iOpen.PrevOpen();

            popup.Open(0, popup => 
            { 
                iOpen.PostOpen(); 
            });
        }
        else
        {
            popup.Open(0);
        }

    }


    public async void ShowAsync<T>(Action<T> inCreateCallback) where T : PopupBase
    {
        T popup = CreatePopup<T>(inCreateCallback);

        if(popup == null)
        {
            Debug.LogError($"[{nameof(Show)}] popup object is null or empty!");
            return;
        }

        if(popup is IPopupOpenAsync iOpenAsync)
        {
            await iOpenAsync.PrevOpenAsync();

            popup.Open(0, async (popup)=>
            {
                await iOpenAsync.PostOpenAsync();
            });
        }
    }


    public void Reserve<T>(Action<T> inCreateCallback) where T : PopupBase
    {
        // 열려있는 팝업이 없다면..? 그냥 열어라
        if(_openedPopups?.Count == 0)
        {
            Show<T>(inCreateCallback);
        }
        // 예약걸어라.
        else
        {
            _reserverPopups.Enqueue(()=> { Show<T>(inCreateCallback); });
        }
    }


    private T CreatePopup<T>(Action<T> inCreateCallback) where T : PopupBase
    {
        Type popupType = typeof(T);

        T popup = null;

        foreach(var attr in popupType.GetCustomAttributes(true))
        {
            if(attr is PopupAttribute popupAttr)
            {
                if(popupAttr.IN_RESOURCES_FORLDER == true)
                {
                    popup = Resources.Load<T>(popupAttr.PATH_IN_RESOURCES_FOLDER);
                }
                else
                {
                    // TODO@taeho.kang 에셋번들에서 로드
                }
            }
        }
     
        if(popup == null)
        {
            Debug.LogError($"[{nameof(CreatePopup)}] popup asset is null!");
            return null;
        }

        popup = Instantiate<T>(popup);

        return popup;
    }


    public void Close()
    {
        if(_openedPopups?.Count > 0)
        {
            var popup = _openedPopups.Pop();

            popup.Close(popup =>
            {
                Destroy(popup);

            });
        }
    }


    public bool IsExposedNow<T>()
    {
        foreach(var popup in _openedPopups)
        {
            if(popup is T)
               return true;
        }

        return false;
    }
}