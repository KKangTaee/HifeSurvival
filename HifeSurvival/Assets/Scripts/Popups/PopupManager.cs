using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;


public class PopupAttribute : System.Attribute
{
    public string PATH_IN_NOT_RESOURCES_FOLDER;     // 리소스폴더 아닌 곳의 경로 (번들용도)
    public string PATH_IN_RESOURCES_FOLDER;         // 리소스폴더
    public bool IN_RESOURCES_FORLDER;             // 해당 팝업이 어디 경로에 있는지 유무 체크
}


public class PopupManager
{
    private static PopupManager _instacne;

    public static PopupManager Instance
    {
        get
        {
            if (_instacne == null)
                _instacne = new PopupManager();

            return _instacne;
        }
    }


    //-----------------
    // variables
    //-----------------

    List<PopupBase> _openedPopups = new List<PopupBase>();
    Queue<Action> _reserverPopups = new Queue<Action>();
    int _currentLayerOlder = 1000;


    //------------------
    // functions
    //------------------

    public void Show<T>(Action<T> inCreateCallback) where T : PopupBase
    {
        // 이미노출되었다면..? 열지 않는다
        if (IsExposedNow<T>() == true)
            return;

        T popup = CreatePopup<T>(inCreateCallback);

        if (popup == null)
        {
            Debug.LogError($"[{nameof(Show)}] popup object is null or empty!");
            return;
        }

        _openedPopups.Add(popup);

        if (popup is IPopupOpen iOpen)
        {
            iOpen.PrevOpen();

            popup.Open(_currentLayerOlder++, popup =>
            {
                iOpen.PostOpen();
            });
        }
        else
        {
            popup.Open(_currentLayerOlder++);
        }

    }


    public async void ShowAsync<T>(Action<T> inCreateCallback) where T : PopupBase
    {
        T popup = CreatePopup<T>(inCreateCallback);

        if (popup == null)
        {
            Debug.LogError($"[{nameof(Show)}] popup object is null or empty!");
            return;
        }

        if (popup is IPopupOpenAsync iOpenAsync)
        {
            await iOpenAsync.PrevOpenAsync();

            popup.Open(_currentLayerOlder++, async (popup) =>
            {
                await iOpenAsync.PostOpenAsync();
            });
        }
    }


    public void Reserve<T>(Action<T> inCreateCallback) where T : PopupBase
    {
        // 열려있는 팝업이 없다면..? 그냥 열어라
        if (_openedPopups?.Count == 0)
        {
            Show<T>(inCreateCallback);
        }
        // 예약걸어라.
        else
        {
            _reserverPopups.Enqueue(() => { Show<T>(inCreateCallback); });
        }
    }


    private T CreatePopup<T>(Action<T> inCreateCallback) where T : PopupBase
    {
        Type popupType = typeof(T);

        T popup = null;

        foreach (var attr in popupType.GetCustomAttributes(true))
        {
            if (attr is PopupAttribute popupAttr)
            {
                if (popupAttr.IN_RESOURCES_FORLDER == true)
                {
                    popup = Resources.Load<T>(popupAttr.PATH_IN_RESOURCES_FOLDER);
                }
                else
                {
                    // TODO@taeho.kang 에셋번들에서 로드
                }
            }
        }

        if (popup == null)
        {
            Debug.LogError($"[{nameof(CreatePopup)}] popup asset is null!");
            return null;
        }

        popup = MonoBehaviour.Instantiate<T>(popup);
        inCreateCallback?.Invoke(popup);
        
        return popup;
    }

    // 베이스팝업에서 close를 할 경우, 애니메이션 호출 후
    // 팝업매니저에 담긴 _openedPopups에서 자신의 데이터를 제거해야함.
    // 그런데, 이걸 어떻게 구현하는 것이 효율적인 방법인지 정확하게 알기 힘듬.

    public void RemovePopup(PopupBase inPopup)
    {
        if(_openedPopups.Remove(inPopup) == true)
        {
            Debug.Log("[RemovePopup]");
        }
    }


    public bool IsExposedNow<T>()
    {
        foreach (var popup in _openedPopups)
        {
            if (popup is T)
                return true;
        }

        return false;
    }


    public T GetPopup<T>() where T : PopupBase
    {
        if(IsExposedNow<T>() == false)
            return default;
        
        return _openedPopups.FirstOrDefault(x=>x.GetType() == typeof(T)) as T;
    }

}
