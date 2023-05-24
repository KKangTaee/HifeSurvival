using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MoveMachine), typeof(TriggerMachine))]
public class MonsterBase : MonoBehaviour
{
    public enum EStatus
    {
        IDLE,

        FOLLOW_TARGET,

        BACK_TO_SPAWN,

        ATTACK,

        DAMAGED,
    }


    // [SerializeField] SpineCharacter _spine;

    public int MosterId { get; private set; }

    public virtual void OnCommand<T>(EStatus inStatus,  in T inParams) where T : IParam
    {
        switch(inStatus)
        {
            case EStatus.IDLE:
                break;

            case EStatus.ATTACK:
                break;

            case EStatus.FOLLOW_TARGET:          
                break;

            case EStatus.BACK_TO_SPAWN:
                break;

            case EStatus.DAMAGED:
                break;
        }
    }


    public void SetWorldPos(in Vector3 inPos)
    {
        transform.position = inPos;
    }

    public void Initialize(int inId, in Vector3 inPos)
    {
        MosterId = inId;
        SetWorldPos(inPos);
    }
   
    public interface IParam { }

    public struct AttackParam : IParam
    {

    }
}
