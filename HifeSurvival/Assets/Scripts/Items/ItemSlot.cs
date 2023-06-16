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
    [SerializeField] Image IMG_cooltime;
    [SerializeField] TMP_Text TMP_cooltime;
    [SerializeField] Button BTN_click;

    private IDisposable updateSubscription;
    private IDisposable countdownSubscription;

    public bool IsEquipping    { get=> ItemInfo != null; }
    public EntityItem ItemInfo { get; private set; }

    public void EquipItem(EntityItem inEntityItem)
    {
        ItemInfo = inEntityItem;
    }
    
    public void RemoveItem()
    {
        StopCooltime();
    }

    public void StartCooltime()
    {
        int cooltime = ItemInfo.cooltime;
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
}
