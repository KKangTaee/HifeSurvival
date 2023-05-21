using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PlayerSelectView : MonoBehaviour
{
    [SerializeField] Image IMG_hero;
    [SerializeField] TMP_Text TMP_userName;
    [SerializeField] TMP_Text TMP_heroName;
    [SerializeField] Image IMG_readyDimmed;
    [SerializeField] TMP_Text TMP_ready;

    public int PlayerId { get; private set; }
    public bool IsUsing { get; private set; }

    public void SetInfo(int inPlayerId, Sprite inSprite, string inUserName, string inHeroName)
    {
        PlayerId = inPlayerId;
        TMP_userName.text = inUserName;
        SetHero(inSprite, inHeroName);

        IsUsing = true;
        IMG_readyDimmed.gameObject.SetActive(false);
    }

    public void SetHero(Sprite inSprite, string inHeroName)
    {
        IMG_hero.sprite = inSprite;
        TMP_heroName.text = inHeroName;
    }

    public void Clear()
    {
        TMP_userName.text = null;
        SetHero(null, null);
        IsUsing = false;
    }

    public void Ready()
    {
        IMG_readyDimmed.gameObject.SetActive(true);
        
         // TMP_ready가 처음에는 scale 4에서 0.3초만에 scale 2로 변형
        TMP_ready.rectTransform.DOScale(2f, 0.3f).OnComplete(() =>
        {
            // 변형이 끝나면, IMG_readyDimmed가 번개에 맞은 것처럼 반짝이며 알파값이 0.5로 변경
            IMG_readyDimmed.DOFade(0.5f, 0.1f).SetLoops(6, LoopType.Yoyo).OnComplete(() =>
            {
                IMG_readyDimmed.DOFade(0.5f, 0.3f); // 최종적으로 알파값이 0.5가 되도록 함
            });
        });
    }
}
