using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class KDView : MonoBehaviour
{
    [SerializeField] TMP_Text TMP_killCount;
    [SerializeField] TMP_Text TMP_deadCount;

    public int targetId  { get; private set; }
    public int KillCount { get; private set; }
    public int DeadCount { get; private set; }

    public void SetInfo(int inTargetId, int inKillCount, int inDeadCount)
    {
        targetId = inTargetId;

        AddKill(inKillCount);

        AddDead(inDeadCount);

        gameObject.SetActive(true);
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
