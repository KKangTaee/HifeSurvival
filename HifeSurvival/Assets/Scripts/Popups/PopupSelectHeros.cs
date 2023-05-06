using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Popup(PATH_IN_RESOURCES_FOLDER="Prefabs/Popups/PopupSelectHeros/PopupSelectHeros",
       IN_RESOURCES_FORLDER = true)]
public class PopupSelectHeros : PopupBase
{
    [Header("PopupSelectHeros")]
    [SerializeField] Button           BTN_close;
    [SerializeField] RectTransform    RT_heroSelection;
    [SerializeField] HeroSelectButton _selectButtonPrefab;

    private void Awake()
    {
        SetHeroButton();
    }

    private void SetHeroButton()
    {
        var staticData = StaticData.Instance.HeroDic.Values;

        if(staticData == null || _selectButtonPrefab == null)
        {
            Debug.LogError($"[{nameof(SetHeroButton)}] ");
            return;
        }

        foreach(var data in staticData)
        {
            var prefab = Instantiate<HeroSelectButton>(_selectButtonPrefab, RT_heroSelection);

            prefab.SetInfo(data, (data)=>
            {
                // 여기 호출.
            });
        }
    }


    protected override void OnButtonEvent(Button inButton)
    {
        if(inButton == BTN_close)
        {
            Close();
        }
    }
}
