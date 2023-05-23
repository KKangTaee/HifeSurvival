using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeroRTCapture : MonoBehaviour
{
    [SerializeField] HeroAnimator _anim;
    [SerializeField] Camera _captureCamera;

    public RenderTexture GetCaptureTexture()
    {
        return _captureCamera.targetTexture;
    }

    public HeroAnimator GetAnimator()
    {
        return _anim;
    }


    //---------------
    // statics
    //---------------

    static HeroRTCapture _capture;

    public static HeroRTCapture GetInstance()
    {
        if(_capture == null)
        {
            var prefab = Resources.Load<HeroRTCapture>($"Prefabs/Commons/{nameof(HeroRTCapture)}");

            if(prefab == null)
            {
                Debug.LogError("prefab is null or empty!");
                return null;
            }

            _capture = Instantiate(prefab);
            DontDestroyOnLoad(_capture);

            _capture.transform.position = new Vector3(1,1,1);
        }

        return _capture;
    }
}
