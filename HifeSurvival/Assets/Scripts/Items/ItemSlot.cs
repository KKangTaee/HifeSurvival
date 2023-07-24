using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UniRx;
using System;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] Image IMG_itemIcon;
    [SerializeField] Image IMG_itemAdd;
    [SerializeField] Image IMG_cooltime;
    [SerializeField] TMP_Text TMP_cooltime;
    [SerializeField] Button BTN_click;
    [SerializeField] Slider SLD_stackRatio;
    [SerializeField] Image [] _stackFrameLineArr;

    private IDisposable updateSubscription;
    private IDisposable countdownSubscription;

    public bool IsEquipping    { get=> ItemInfo != null; }
    public EntityItem ItemInfo { get; private set; }


    public void EquipItem(EntityItem entityItem)
    {
        ItemInfo = entityItem;

        SetActiveIcon();

        IMG_itemIcon.sprite = GetSpriteIcon(entityItem.ItemKey);

        UpdateStack(entityItem);
    }
    
    public void RemoveItem()
    {
        StopCooltime();

        SetActiveIcon();
    }

    public void StartCooltime()
    {
        int cooltime = ItemInfo.Skill.Cooltime;
        IMG_cooltime.fillAmount = 1f;

        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();

        updateSubscription = Observable.EveryUpdate()
            .TakeWhile(_ => stopwatch.Elapsed.TotalSeconds < cooltime)
            .Subscribe(_ =>
            {
                float decreaseAmount = (float)(stopwatch.Elapsed.TotalSeconds / cooltime);
                IMG_cooltime.fillAmount = 1f - decreaseAmount;
            },
            () => { IMG_cooltime.fillAmount = 0f; })
            .AddTo(this);

        countdownSubscription = Observable.Interval(TimeSpan.FromSeconds(1))
            .Take(cooltime)
            .Subscribe(_ =>
            {
                int remainingSeconds = cooltime - (int)stopwatch.Elapsed.TotalSeconds;
                TMP_cooltime.text = remainingSeconds.ToString();
            },
            () => { TMP_cooltime.text = "0"; })
            .AddTo(this);
    }

    public void SetActiveIcon()
    {
        IMG_itemAdd.gameObject.SetActive(IsEquipping == false);
        IMG_itemIcon.gameObject.SetActive(IsEquipping == true);
        SLD_stackRatio.gameObject.SetActive(IsEquipping == true);
    }

    public void StopCooltime()
    {
        if (updateSubscription != null)
        {
            updateSubscription.Dispose();
            updateSubscription = null;
        }

        if (countdownSubscription != null)
        {
            countdownSubscription.Dispose();
            countdownSubscription = null;
        }
    }

    public Sprite GetSpriteIcon(int itemKey)
    {
        // TODO@taeho.kang 임시
        string path = "Textures/Items";
        return Resources.Load<Sprite>($"{path}/item_icon_{itemKey}");
    }

    private void UpdateStack(EntityItem entityItem)
    {
        for(int i =0; i<_stackFrameLineArr.Length; i++)
           _stackFrameLineArr[i].gameObject.SetActive(i < entityItem.MaxStack - 1);

        SLD_stackRatio.value = entityItem.CurrentStack / (float)entityItem.MaxStack;
    }
}
