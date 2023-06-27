using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MonsterUI : MonoBehaviour
{

    [SerializeField] Slider SLD_hpBar;

    private int _maxHP;
    private int _currHP;

    // Start is called before the first frame update
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

    public void UpdateHpBar()
    {
        // 바로 감소시키는 hpBar
        SLD_hpBar.value = (float)_currHP / _maxHP;
    }

    public void DecreaseHP(int damageValue)
    {
         _currHP -= damageValue;
         
         UpdateHpBar();
    }
}
