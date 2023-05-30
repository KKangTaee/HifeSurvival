using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KDView : MonoBehaviour
{
    [SerializeField] TMP_Text TMP_killCount;
    [SerializeField] TMP_Text TMP_deadCount;

    public int PlayerId  { get; private set; }
    public int KillCount { get; private set; }
    public int DeadCount { get; private set; }

    public void SetInfo(int inPlayerId, int inKillCount, int inDeadCount)
    {
        PlayerId = inPlayerId;

        AddKill(inKillCount);

        AddDead(inDeadCount);
    }

    public void AddKill(int inCount)
    {
        KillCount += inCount;
        TMP_killCount.text = KillCount.ToString();
    }

    public void AddDead(int inCount)
    {
        DeadCount += inCount;
        TMP_deadCount.text = DeadCount.ToString();
    }
}
