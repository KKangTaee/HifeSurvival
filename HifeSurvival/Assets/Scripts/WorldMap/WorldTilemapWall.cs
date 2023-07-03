using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldTilemapWall : WorldTilemap
{
    [SerializeField] private TriggerMachine _triggerTilemap;

    private const string MATERIAL_PROPERTY_KEY_START_POSITION = "_StarPosition";

    private Transform _targetTr;



    //-----------------
    // unity events
    //------------------
    
    protected override void Awake()
    {
        base.Awake();

        SetTriiger();
    }

    private void Update()
    {
        UpdateMasking();
    }



    //-----------------
    // functions
    //-----------------

    public void SetTriiger()
    {
        _triggerTilemap.AddTriggerEnter(col =>
        {
            if(col.CompareTag(TagName.PLAEYR_SELF) == true)
            {
                _targetTr = col.transform;
            }
            else if(col.CompareTag(TagName.PLAYER_OTHER) == true)
            {
                var other = col.GetComponent<Player>();
            }
        });

        _triggerTilemap.AddTriggerExit(col =>
        {
            if(col.CompareTag(TagName.PLAEYR_SELF) == true)
            {
                StopMasking();
            }
            else if(col.CompareTag(TagName.PLAYER_OTHER) == true)
            {
                var other = col.GetComponent<Player>();
            }
        });
    }

    private void UpdateMasking()
    {
        if (_targetTr == null)
            return;
        
        Vector3 offset = Vector3.up;
        TilemapMat.SetVector(MATERIAL_PROPERTY_KEY_START_POSITION, _targetTr.position + offset);
    }

    private void StopMasking()
    {
        _targetTr = null;
        TilemapMat.SetVector(MATERIAL_PROPERTY_KEY_START_POSITION, new Vector3(-9999, -9999, -9999));
    }
}
