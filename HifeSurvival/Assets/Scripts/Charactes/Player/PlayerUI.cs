using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class PlayerUI : MonoBehaviour
{
    [SerializeField] Image IMG_hpInner;
    [SerializeField] Image IMG_hpBar;

    private int _maxHP;
    private int _currHP;

    //-------------------
    // unity events
    //-------------------

    private void Start()
    {
        GetComponent<Canvas>().worldCamera = ControllerManager.Instance.GetController<CameraController>().MainCamera;
    }


    //-------------------
    // functions
    //-------------------
    
    public void Init(int inMaxHP)
    {
        SetMaxHP(inMaxHP);
        SetHP(inMaxHP);
    }

    public void SetMaxHP(int inMaxHP)
    {
        _maxHP = inMaxHP;

        UpdateHpBar();
    }

    public void SetHP(int inHp)
    {
        _currHP = inHp;

        UpdateHpBar();
    }

    public void DecreaseHP(int damageValue)
    {
        _currHP -= damageValue;

        // UpdateHpBar();

        // 번쩍이는 효과를 위해 색상을 잠시 흰색으로 변경
        Color originalColor = IMG_hpInner.color;
        IMG_hpInner.color = Color.white;

        // 0.2초 후에 원래 색상으로 되돌림
        DOVirtual.DelayedCall(0.2f, () => IMG_hpInner.color = originalColor);

        // 애니메이션으로 감소시키는 hpInner
        float newFillAmount = (float)_currHP / _maxHP;
        DOTween.To(() => IMG_hpInner.fillAmount, x => IMG_hpInner.fillAmount = x, newFillAmount, 0.2f).SetEase(Ease.InCubic);
    }

    public void UpdateHpBar()
    {
        // 바로 감소시키는 hpBar
        IMG_hpBar.fillAmount    = (float)_currHP / _maxHP;
        IMG_hpInner.fillAmount  = (float)_currHP / _maxHP;
    }
}
