using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerSelectView : MonoBehaviour
{
    [SerializeField] Image IMG_hero;
    [SerializeField] TMP_Text TMP_userName;
    [SerializeField] TMP_Text TMP_heroName;

    public int PlayerId { get; private set; }
    public bool IsUsing { get; private set; }

    public void SetInfo(int inPlayerId, Sprite inSprite, string inUserName, string inHeroName)
    {
        PlayerId = inPlayerId;
        TMP_userName.text = inUserName;
        SetHero(inSprite, inHeroName);

        IsUsing = true;
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
}
