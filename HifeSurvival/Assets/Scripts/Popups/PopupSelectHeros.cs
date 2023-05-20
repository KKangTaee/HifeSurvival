using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using System.Linq;
using System;


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
    private Dictionary<int, Sprite> _heroImageDic = new Dictionary<int, Sprite>();
    private Action <int, int> _onSendSelectHeroCB;

    private int _playerIdSelf;

    protected override void Awake()
    {
        base.Awake();

        _eOpenAnim  = EAnim.NONE;
        _eCloseAnim = EAnim.NONE;

        ClearPlayerView();

        LoadHeroSprite();

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

                ChangeHeroView(_playerIdSelf, data.id);

                // 서버전송
                _onSendSelectHeroCB?.Invoke(_playerIdSelf, data.id);
            }
            , GetHeroSprite(data.id));
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

    public void OnRecvJoin(PlayerEntity inEntity)
    {
        AddPlayerView(inEntity);
    }

    public void Leave(int inPlayerId)
    {
        var view = _playerSelectViewArr.FirstOrDefault(x=> x.PlayerId == inPlayerId);

        if(view == null)
            return;

        view.Clear();
    }

    public void OnRecvSelectHero(PlayerEntity inEntity)
    {
       ChangeHeroView(inEntity.playerId, inEntity.heroId);
    }

    public void ChangeHeroView(int inPlayerId, int inHeroId)
    {
         var view = _playerSelectViewArr.FirstOrDefault(x=>x.PlayerId == inPlayerId);

        if(view == null)
            return;

        if(StaticData.Instance.HeroDic.TryGetValue(inHeroId.ToString(), out var data) == false)
           return;

        view.SetHero(GetHeroSprite(inHeroId), data.name);
    }

    public void AddPlayerView(PlayerEntity inEntity)
    {
        var view = _playerSelectViewArr.FirstOrDefault(x=>x.IsUsing == false);

        if(view == null)
        {
            Debug.LogError($"[{nameof(AddPlayerView)}] 가득참!");
            return;
        }

        if(StaticData.Instance.HeroDic.TryGetValue(inEntity.heroId.ToString(), out var data) == false)
           return;

        view.SetInfo(inEntity.playerId, GetHeroSprite(inEntity.heroId), inEntity.userName, data.name);
    }

    public void ClearPlayerView()
    {
        foreach(var view in _playerSelectViewArr)
            view.Clear();
    }


    public void LoadHeroSprite()
    {
        _heroImageDic?.Clear();

        var staticData = StaticData.Instance.HeroDic.Values;

        foreach(var data in staticData)
        {
            //TODO@taeho.kang 후에 번들이나 다른방식으로 로드..
            var sprite = Resources.Load<Sprite>($"Prefabs/Textures/Profiles/profile_{data.id}");
            _heroImageDic.Add(data.id, sprite);
        }
    }

    public Sprite GetHeroSprite(int inHeroId)
    {
        return _heroImageDic.TryGetValue(inHeroId, out var sprite) ? sprite : null; 
    }

    public void AddEvent(Action<int, int> inOnSendSelectHero)
    {
        _onSendSelectHeroCB = inOnSendSelectHero;
    }

    public void RemoveEvent()
    {
        _onSendSelectHeroCB = null;
    }

    public void SetPlayerIdSelf(int inId)
    {
        _playerIdSelf = inId;
    }
}
