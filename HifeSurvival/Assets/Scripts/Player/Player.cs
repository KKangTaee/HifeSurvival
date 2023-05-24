using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;


[RequireComponent(typeof(MoveMachine))]
public class Player : MonoBehaviour
{
    public enum EStatus
    {
        IDLE,

        MOVE,

        ATTACK,

        USE_SKILL,
    }

    //  [SerializeField] private SpineCharacter _character;
    [SerializeField] private MoveMachine _moveMachine;
    [SerializeField] private TriggerMachine _triggerMachine;

    [SerializeField] private HeroAnimator _anim;


    private WorldMap _worldMap;

    public EStatus Status { get; private set; }
    public bool IsSelf { get; private set; }


    //-----------------
    // unity events
    //-----------------

    private void Awake()
    {
        _moveMachine = GetComponent<MoveMachine>();
    }

    private void Start()
    {
        _anim.PlayAnimation(HeroAnimator.AnimKey.KEY_IDLE);
    }


    //-----------------
    // functions
    //-----------------

    public void Init(bool isSelf, in Vector3 inPos)
    {
        if (isSelf == true)
            SetTrigger();

        IsSelf = isSelf;

        SetWorldPos(inPos);
    }
    public void SetWorldPos(in Vector3 inPos)
    {
        transform.position = inPos;
    }

    public void OnMove(in Vector3 inDir)
    {
        _anim.OnWalk(inDir);
        _moveMachine.MoveManual(inDir);
    }

    public void OnIdle()
    {
        _anim.OnIdle();
    }

    // Player 클래스 내부
    public void SetTrigger()
    {
        _triggerMachine.AddTriggerStay((sender, col) =>
        {
            if (col.CompareTag(TagName.WORLDMAP_WALL) == true)
            {
                if (_worldMap == null)
                    _worldMap = col.gameObject.GetComponentInParent<WorldMap>();

                Vector3 playerPos = transform.position;
                Vector3 hitPoint = col.ClosestPoint(playerPos);

                _worldMap?.UpdateWallMasking(hitPoint, playerPos);
            }
        });

        _triggerMachine.AddTriggerExit((sender, col) =>
        {
            if (col.CompareTag(TagName.WORLDMAP_WALL) == true)
            {
                if (_worldMap == null)
                    _worldMap = col.gameObject.GetComponentInParent<WorldMap>();

                _worldMap?.DoneWallMasking();
            }
        });
    }
}