using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using System;
using System.Linq;

public class PlayerUI : MonoBehaviour
{
    [Serializable]
    public class ItemView
    {
        [SerializeField] private Image icon;

        public bool IsEquipping { get => icon.sprite != null; }

        public void SetSprite(Sprite iconSprite)
        {
            this.icon.gameObject.SetActive(iconSprite != null);
            this.icon.sprite = iconSprite;
        }
    }


    [SerializeField] Slider SLD_hpInner;
    [SerializeField] Slider SLD_hpBar;
    [SerializeField] Image  IMG_hpInnerFill;
    [SerializeField] Image  IMG_hpBarFill;
    [SerializeField] TMP_Text TMP_hp;
    [SerializeField] ItemView [] _itemViewArr;

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

        SLD_hpBar.value    = (float)_currHP / _maxHP;

        // 번쩍이는 효과를 위해 색상을 잠시 흰색으로 변경
        Color originalColor = IMG_hpInnerFill.color;
        IMG_hpInnerFill.color = Color.white;

        // 0.2초 후에 원래 색상으로 되돌림
        DOVirtual.DelayedCall(0.2f, () => IMG_hpInnerFill.color = originalColor);

        // 애니메이션으로 감소시키는 hpInner
        float newFillAmount = (float)_currHP / _maxHP;
        DOTween.To(() => SLD_hpInner.value, x => SLD_hpInner.value = x, newFillAmount, 0.2f).SetEase(Ease.InCubic);
    
        TMP_hp.transform.localScale *= 1.8f;
        TMP_hp.transform.DOScale(1, 0.2f);
        TMP_hp.text = _currHP.ToString();
    }

    public void UpdateHpBar()
    {
        // 바로 감소시키는 hpBar
        SLD_hpBar.value    = (float)_currHP / _maxHP;
        SLD_hpInner.value  = (float)_currHP / _maxHP;
    }

    public void EquipItem(EntityItem entityItem)
    {
        var emptyView = _itemViewArr.FirstOrDefault(x=>x.IsEquipping == false);

        if(emptyView == null)
        {
            Debug.LogError($"[{nameof(EquipItem)}] emptyView is null or empty!");
            return;
        }

        var iconSprite = GetSpriteIcon(entityItem.ItemKey);
        emptyView.SetSprite(iconSprite);
    }

    public Sprite GetSpriteIcon(int itemKey)
    {
        // TODO@taeho.kang 임시
        string path = "Textures/Items";
        return Resources.Load<Sprite>($"{path}/item_icon_{itemKey}");
    }
}
