using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;
using TMPro;
using System.Linq;
using System;
using DG.Tweening;


[Popup(PATH_IN_RESOURCES_FOLDER = "Prefabs/Popups/PopupSelectHeros/PopupSelectHeros",
       IN_RESOURCES_FORLDER = true)]
public class PopupSelectHeros : PopupBase
{
    [Header("[PopupSelectHeros]")]
    [SerializeField] Button BTN_close;
    [SerializeField] Button BTN_ready;
    [SerializeField] RectTransform RT_heroSelection;
    [SerializeField] HeroSelectButton _selectButtonPrefab;

    [SerializeField] TMP_Text TMP_name;
    [SerializeField] TMP_Text TMP_desc;
    [SerializeField] TMP_Text TMP_str;
    [SerializeField] TMP_Text TMP_def;
    [SerializeField] TMP_Text TMP_dex;
    [SerializeField] TMP_Text TMP_hp;
    [SerializeField] TMP_Text TMP_leftTime;
    [SerializeField] RawImage RM_heroCapture;

    [SerializeField] PlayerSelectView[] _playerSelectViewArr;

    private Subject<int> _onClickFrame = new Subject<int>();
    private Dictionary<int, Sprite> _heroImageDic = new Dictionary<int, Sprite>();

    private Action<int> _onSendSelectHeroCB;
    private Action      _onSendReadyCB;
    private Action      _disconnectCB;

    private int _playerIdSelf;
    private HeroRTCapture _capture;

    
    
    //-------------------
    // unity events
    //-------------------

    protected override void Awake()
    {
        base.Awake();

        _eOpenAnim = EAnim.NONE;
        _eCloseAnim = EAnim.NONE;

        SetHeroCapture();

        ClearPlayerView();

        LoadHeroSprite();

        SetHeroButton();

        StartCoroutine(nameof(Co_SelectTimer));
    }



    //-------------------
    // override
    //-------------------

    protected override void OnButtonEvent(Button inButton)
    {
        if (inButton == BTN_close)
        {
            Close(_ => _disconnectCB?.Invoke());
        }
        else if (inButton == BTN_ready)
        {
            Ready();
        }
    }


    private void SetHeroButton()
    {
        var staticData = StaticData.Instance.HeroDict.Values;

        if (staticData == null || _selectButtonPrefab == null)
        {
            Debug.LogError($"[{nameof(SetHeroButton)}] ");
            return;
        }

        StaticData.Heros hero = null;

        foreach (var data in staticData)
        {
            var prefab = Instantiate<HeroSelectButton>(_selectButtonPrefab, RT_heroSelection);

            _onClickFrame.Subscribe(id => { prefab.OnClickFrame(id); }).AddTo(this);

            prefab.SetInfo(data, (data) =>
            {
                OnClickSelectButton(data);
            }
            , GetHeroSprite(data.key));

            if(hero == null)
               hero = data;
        }

        // 초기화때는 첫번째로 고정
        OnClickSelectButton(hero);
    }


    public void SetHeroInfo(StaticData.Heros inData)
    {
        TMP_name.text = inData.name;

        TMP_desc.text = inData.desc;

        TMP_str.text = inData.str.ToString();

        TMP_def.text = inData.def.ToString();

        TMP_hp.text = inData.hp.ToString();

        // TMP_dex.text = inData.dex.ToString();
    }



    private void OnClickSelectButton(StaticData.Heros inData)
    {
        SetHeroInfo(inData);

        // 프레임 변경
        _onClickFrame.OnNext(inData.key);

        // 뷰 변경
        ChangeHeroView(_playerIdSelf, inData.key);

        // 서버전송
        _onSendSelectHeroCB?.Invoke(inData.key);

        // 캐릭터 변경
        _capture.GetAnimator().SetAnimatorController(inData.key -1);
    }


    IEnumerator Co_SelectTimer()
    {
        float leftTime = 30;

        while (true)
        {
            if (leftTime < 0.1f)
                break;

            leftTime -= Time.deltaTime;

            TMP_leftTime.text = $"캐릭터 선택 종료까지 {(int)leftTime}초 전";

            yield return null;
        }
    }

    IEnumerator Co_CountdownTimer(float inSec)
    {
        float leftTime = inSec;

        while (true)
        {
            if (leftTime < 0.1f)
                break;

            leftTime -= Time.deltaTime;

            TMP_leftTime.text = $"게임시작 {(int)leftTime}초 전";

            yield return null;
        }
    }

    public void SetHeroCapture()
    {
        _capture = HeroRTCapture.GetInstance();
        RM_heroCapture.texture = _capture.GetCaptureTexture();
    }


    public void Ready()
    {
        _onSendReadyCB?.Invoke();

        BTN_ready.enabled = false;
        var imgComp = BTN_ready.GetComponent<Image>();

        Color col = imgComp.color;
        col.a = 0.2f;
        imgComp.color = col;

        imgComp.DOFade(0.5f, 0.5f).SetEase(Ease.InOutSine).SetLoops(-1, LoopType.Yoyo);
        BTN_close.gameObject.SetActive(false);

        ReadyPlayerView(_playerIdSelf);
    }

    private void ReadyPlayerView(int inPlayerId)
    {
        var view = _playerSelectViewArr.FirstOrDefault(x => x.PlayerId == inPlayerId);

        if (view == null)
            return;

        view.Ready();
    }

    private void GameStart()
    {
        _ = SceneManager.Instance.ChangeScene(SceneManager.SCENE_NAME_INGAME);
    }


    public void OnRecvJoin(PlayerEntity inEntity)
    {
        AddPlayerView(inEntity);
    }

    public void OnLeave(int inPlayerId)
    {
        var view = _playerSelectViewArr.FirstOrDefault(x => x.PlayerId == inPlayerId);

        if (view == null)
            return;

        view.Clear();
    }

    public void OnRecvSelectHero(PlayerEntity inEntity)
    {
        ChangeHeroView(inEntity.targetId, inEntity.heroId);
    }

    public void OnRecvReadyToGame(PlayerEntity inEntity)
    {
        ReadyPlayerView(inEntity.targetId);
    }

    public void OnRecvCountdown(int inSec)
    {
        StopCoroutine(nameof(Co_SelectTimer));
        StartCoroutine(Co_CountdownTimer(inSec));
    }

    public void OnRecvStartGame()
    {
        GameStart();
    }

    public void ChangeHeroView(int inPlayerId, int inHeroId)
    {
        var view = _playerSelectViewArr.FirstOrDefault(x => x.PlayerId == inPlayerId);

        if (view == null)
            return;

        if (StaticData.Instance.HeroDict.TryGetValue(inHeroId.ToString(), out var data) == false)
            return;

        view.SetHero(GetHeroSprite(inHeroId), data.name);
    }

    public void AddPlayerView(PlayerEntity inEntity)
    {
        var view = _playerSelectViewArr.FirstOrDefault(x => x.IsUsing == false);

        if (view == null)
        {
            Debug.LogError($"[{nameof(AddPlayerView)}] 가득참!");
            return;
        }

        if (StaticData.Instance.HeroDict.TryGetValue(inEntity.heroId.ToString(), out var data) == false)
            return;

        view.SetInfo(inEntity.targetId, GetHeroSprite(inEntity.heroId), inEntity.userName, data.name);
    }

    public void ClearPlayerView()
    {
        foreach (var view in _playerSelectViewArr)
            view.Clear();
    }

    public void LoadHeroSprite()
    {
        _heroImageDic?.Clear();

        var staticData = StaticData.Instance.HeroDict.Values;

        foreach (var data in staticData)
        {
            //TODO@taeho.kang 후에 번들이나 다른방식으로 로드..
            var sprite = Resources.Load<Sprite>($"Prefabs/Textures/Profiles/profile_{data.key}");
            _heroImageDic.Add(data.key, sprite);
        }
    }

    public Sprite GetHeroSprite(int inHeroId)
    {
        return _heroImageDic.TryGetValue(inHeroId, out var sprite) ? sprite : null;
    }

    public void AddEvent(Action<int> inOnSendSelectHero, Action inOnSendReadyToGame, Action inDisconnect)
    {
        _onSendSelectHeroCB = inOnSendSelectHero;
        _onSendReadyCB      = inOnSendReadyToGame;
        _disconnectCB       = inDisconnect;
    }

    public void RemoveEvent()
    {
        _disconnectCB = null;
        _onSendReadyCB = null;
        _onSendSelectHeroCB = null;
    }

    public void SetPlayerIdSelf(int inId)
    {
        _playerIdSelf = inId;
    }
}
