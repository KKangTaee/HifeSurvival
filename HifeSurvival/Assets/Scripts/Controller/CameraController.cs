using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class CameraController : ControllerBase, TouchController.ITouchUpdate
{
    public enum ECamearaStatus
    {
        ZOOM,

        SLIDING,

        FOLLWING_TARGET,
    
        NONE,
    }


    [Range(1, 10)]
    [SerializeField] private float _cameraSpeed;

    public const float FOLLWING_TARGET_SPEED  = 40.0f;
    public const float FOLLWING_TARGET_OFFSET = 1f;


    public readonly Vector3 INVALIED_VECTOR_VALUE = new Vector3(-9999, -9999, -9999);


    //-----------------
    // variables
    //-----------------

    private Camera  _main;

    private Vector3 _prevPos;
    private Vector3 _moveDir;
    private Vector3 _follwingStartPos;

    private float   _dragPower    = 0.0f;
    private float   _prevZoomDist = float.MinValue;
    
    private ECamearaStatus _eStatus = ECamearaStatus.NONE;
    private Transform _followingTarget;
    
    public Camera   MainCamera { get => _main; }


    //-----------------
    // unity events
    //-----------------


    private void Update()
    {
        UpdateCamera();
    }


    //----------------
    // override
    //----------------
    
    public override void Init()
    {
        _main = Camera.main;
        _main.clearStencilAfterLightingPass = true;

        _prevPos = INVALIED_VECTOR_VALUE;
        _moveDir = INVALIED_VECTOR_VALUE;
    }


    //----------------
    // functions
    //----------------

    public void OnTouchUpdate(TouchController.ETouchCommand inCommand, Vector2[] inTouchPos, Collider2D collider = null)
    {
        switch (inCommand)
        {
            case TouchController.ETouchCommand.CAMERA_MOVE:

                Vector3 currPos = _main.ScreenToWorldPoint(inTouchPos.First());

                if (_prevPos != INVALIED_VECTOR_VALUE)
                {
                    Vector3 diff = currPos - _prevPos;
                    _prevPos = currPos;

                    _moveDir = Vector3.Normalize(diff);
                    _dragPower = Vector3.Magnitude(diff) * _cameraSpeed;

                    Vector3 movePos = _cameraSpeed * diff / (float)_main.orthographicSize;
                    _main.transform.position += movePos;
                }

                _prevPos = currPos;
                _eStatus = ECamearaStatus.NONE;

                break;

            case TouchController.ETouchCommand.CAMERA_ZOOM:

                if (inTouchPos?.Length != 2)
                    return;

                float currDist = Vector2.Distance(inTouchPos[0], inTouchPos[1]);

                if (Mathf.Abs(_prevZoomDist - float.MinValue) > 1e-6f)
                {
                    float zoomDelta = currDist - _prevZoomDist;
                    _main.orthographicSize += (zoomDelta);
                }

                _prevZoomDist = currDist;

                break;

            case TouchController.ETouchCommand.CAMERA_MOVE_DONE:
                
                _prevPos = INVALIED_VECTOR_VALUE;

                if (_dragPower > 0)
                    _eStatus = ECamearaStatus.SLIDING;

                break;
        }
    }


    public void UpdateCamera()
    {
        switch(_eStatus)
        {
            case ECamearaStatus.SLIDING:

                if (_dragPower <= 0.5f)
                {
                    _eStatus = ECamearaStatus.NONE;
                    _moveDir = INVALIED_VECTOR_VALUE;
                    return;
                }

                _main.transform.position += (_moveDir * _dragPower * Time.deltaTime);
                _dragPower -= 0.5f;

                break;

            case ECamearaStatus.FOLLWING_TARGET:

                if (_followingTarget == null)
                    return;

                Vector2 cameraPos = _main.transform.position;
                Vector2 targetPos = _followingTarget.transform.position;

                if (Mathf.Abs(targetPos.x - cameraPos.x) > FOLLWING_TARGET_OFFSET ||
                    Mathf.Abs(targetPos.y - cameraPos.y) > FOLLWING_TARGET_OFFSET)
                {
                    var dir = Vector3.Normalize(targetPos - cameraPos);
                    _main.transform.position += (dir * FOLLWING_TARGET_SPEED * Time.deltaTime);
                }
                else
                {
                    _main.transform.position = _followingTarget.position + new Vector3(0, 0, -10);
                }


                break;
        }
    }



    public void FollowingTarget(Transform inTarget)
    {
        _eStatus = ECamearaStatus.FOLLWING_TARGET;
        _followingTarget  = inTarget;
        _follwingStartPos = inTarget.position;
    }


}
