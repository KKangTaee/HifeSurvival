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

    public void SetInfo(Sprite inSprite, string inUserName, string inHeroName)
    {
        IMG_hero.sprite = inSprite;
        TMP_userName.text = inUserName;
        TMP_heroName.text = inHeroName;
    }
}
