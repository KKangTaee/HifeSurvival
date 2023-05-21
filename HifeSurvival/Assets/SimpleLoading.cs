using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class SimpleLoading : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Image _backdimmed;
    [SerializeField] Image IMG_icon;
    [SerializeField] TMP_Text TMP_desc;

    public void Show(string inDesc = null, Sprite inIcon = null)
    {
        var startIconPos = IMG_icon.rectTransform.anchoredPosition;

        IMG_icon.rectTransform.DOAnchorPosY(startIconPos.y - 10f, 1)
                              .SetEase(Ease.InOutSine)
                              .SetLoops(-1, LoopType.Yoyo);
        SetDesc(inDesc);
        // SetIcon(inIcon);
    }

    public void SetDesc(string inDesc)
    {
        TMP_desc.text = inDesc;
    }

    public void SetIcon(Sprite inIcon)
    {
        IMG_icon.sprite = inIcon;
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }


    //----------------
    // statics
    //----------------

    static SimpleLoading _obj = null;

    public static void Expose(string inDesc = null, Sprite inIcon = null)
    {
        if(_obj == null)
        {
            var prefab = Resources.Load<SimpleLoading>($"Prefabs/Commons/{nameof(SimpleLoading)}");
            
            if(prefab == null)
            {
                Debug.LogError($"[{nameof(Show)}] prefab is not load");
                return;
            }
            
            _obj = Instantiate(prefab);
            DontDestroyOnLoad(_obj);
        }

        _obj.SetActive(true);
        _obj?.Show(inDesc, inIcon);
    }


    public static void Hide()
    {
        _obj?.SetActive(false);
    }

    public static void ChangeDesc(string inDesc)
    {
        _obj?.SetDesc(inDesc);
    }
}
