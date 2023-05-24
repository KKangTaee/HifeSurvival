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
    [SerializeField] private MoveMachine    _moveMachine;
    [SerializeField] private TriggerMachine _triggerMachine;

    private WorldMap _worldMap;

    public EStatus Status { get; private set; }
    public bool IsSelf    { get; private set; }



    //-----------------
    // unity events
    //-----------------

    private void Awake()
    {
        _moveMachine    = GetComponent<MoveMachine>();
    }

    private void Start()
    {
        // _character.PlayAnimation(SpineCharacter.EAnimKey.IDLE);
    }


    //-----------------
    // functions
    //-----------------

    public void MoveAuto(List<Vector3> inMoveList)
    {
        // _character.PlayAnimation(SpineCharacter.EAnimKey.RUN);

        // _moveMachine.MoveAuto(inMoveList,
        //                       changeDirCallback: dir =>
        //                       {
        //                           _character.SetDir(dir);
        //                       },
        //                       doneCallback: isDone =>
        //                       {
        //                           _character.PlayAnimation(SpineCharacter.EAnimKey.IDLE);
        //                       });
    }


    public void MoveMenual(in Vector3 inDir)
    {
        _moveMachine.MoveManual(inDir);
    }


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

    public void SetWorldPos(in Vector3 inPos)
    {
        transform.position = inPos;
    }


    public void Initialzie(bool isMe, in Vector3 inPos)
    {
        if(isMe == true)
           SetTrigger();

        IsSelf = isMe;

        SetWorldPos(inPos);
    }
}