using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum EGameCurrency : uint
{
    COIN    = 0,
    TICKET  = 1,
    ITEM    = 2,
}

public class HUD_CurrencyView : MonoBehaviour
{

    [SerializeField] private Button  BTN_click;

    [SerializeField] private Image   IMG_Icon;

    [SerializeField] private TMP_Text TXT_count;

    [SerializeField] private Sprite [] _iconArr;



    //--------------
    // functions
    //--------------

    public void SetInfo(EGameCurrency inCurrency, int inCount)
    {

        SetIcon(inCurrency);

        SetButton(inCurrency);

        SetCount(inCount);
    }


    public void SetIcon(EGameCurrency inCurrency)
    {
        switch(inCurrency)
        {
            case EGameCurrency.TICKET:
                IMG_Icon.sprite = _iconArr[1];
                break;

            case EGameCurrency.COIN:
                IMG_Icon.sprite = _iconArr[0];
                break;
        }

        IMG_Icon.SetNativeSize();
    }


    public void SetButton(EGameCurrency inCurrency)
    {
        BTN_click.onClick.AddListener(()=>
        {
            // TODO@taeho.kang 상점열기
        });
    }


    public void SetCount(int inCount)
    {
        TXT_count.text = inCount.ToString();
    }
}
