using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;


[Popup(PATH_IN_RESOURCES_FOLDER="Prefabs/Popups/PopupSelectHeros/PopupSelectHeros",
       IN_RESOURCES_FORLDER = true)]
public class PopupSelectHeros : PopupBase
{
    [Header("[PopupSelectHeros]")]
    [SerializeField] Button           BTN_close;
    [SerializeField] RectTransform    RT_heroSelection;
    [SerializeField] HeroSelectButton _selectButtonPrefab;

    [SerializeField] TMP_Text TMP_name;
    [SerializeField] TMP_Text TMP_desc;
    [SerializeField] TMP_Text TMP_str;
    [SerializeField] TMP_Text TMP_def;
    [SerializeField] TMP_Text TMP_dex;
    [SerializeField] TMP_Text TMP_hp;

    [SerializeField] TMP_Text TMP_leftTime;

    [SerializeField] PlayerSelectView [] _playerSelectViewArr;

    private Subject<int> _onClickFrame = new Subject<int>();

    private int _nextJoinIndex = 0;

    protected override void Awake()
    {
        base.Awake();

        _eOpenAnim  = EAnim.NONE;
        _eCloseAnim = EAnim.NONE;

        SetHeroButton();

        StartCoroutine("Co_Timer");
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

            _onClickFrame.Subscribe(id => { prefab.OnClickFrame(id); }).AddTo(this);

            prefab.SetInfo(data, (data)=>
            {
                SetHeroInfo(data);

                // 프레임 변경
                _onClickFrame.OnNext(data.id);
            });
        }
    }

    public void SetHeroInfo(StaticData.Heros inData)
    {
        TMP_name.text = inData.name;

        TMP_desc.text = inData.desc;

        TMP_str.text  = inData.str.ToString();

        TMP_def.text  = inData.def.ToString();

        TMP_dex.text  = inData.dex.ToString();

        TMP_hp.text   = inData.hp.ToString();
    }



    protected override void OnButtonEvent(Button inButton)
    {
        if(inButton == BTN_close)
        {
            Close();
        }
    }


    IEnumerator Co_Timer()
    {
        float leftTime = 60;

        while(true)
        {
            if(leftTime < 0.1f)
               break;

            leftTime -= Time.deltaTime;

            TMP_leftTime.text = $"게임시작 {(int)leftTime}초 전";

            yield return null;
        }

        GameStart();
    }

    private  void GameStart()
    {
        _ = SceneManager.Instance.ChangeScene(SceneManager.SCENE_NAME_INGAME);
    }

    public void Join()
    {
        
    }

    public void Leave()
    {

    }
}
