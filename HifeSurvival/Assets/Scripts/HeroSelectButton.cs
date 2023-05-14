using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class HeroSelectButton : MonoBehaviour
{
    [SerializeField] Button BTN_click;
    [SerializeField] Image  IMG_hero;
    [SerializeField] Image  IMG_frame;

    private StaticData.Heros _data;
    private Action<StaticData.Heros> _clickCallback;

    public void SetInfo(StaticData.Heros inData,  Action<StaticData.Heros> inClickCallback)
    {
        _data = inData;
        _clickCallback = inClickCallback;

        SetHeroImage(_data.id);

        SetClick();
    }

    public void SetHeroImage(int inId)
    {
        IMG_hero.sprite = Resources.Load<Sprite>($"Prefabs/Textures/Profiles/profile_{inId}");
    }

    public void SetClick()
    {
        BTN_click.onClick.AddListener(()=>
        {
            _clickCallback?.Invoke(_data);
        });
    }

    public void OnClickFrame(int inId)
    {
        IMG_frame.color = inId == _data.id ? Color.magenta : Color.white;
    }
}
