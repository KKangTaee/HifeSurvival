using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEditor;
using System.Linq;
using System;
using System.Reflection;


public class WorldMap : MonoBehaviour
{
    [SerializeField] private WorldTilemap       _ground;
    [SerializeField] private WorldTilemap       _block;
    [SerializeField] private WorldTilemapWall   _wall;

    [SerializeField] private Transform          _objectRoot;

    private Dictionary<Type, List<WorldObjectBase>> _worldObjDict;
    private ObjectPoolController _objectPoolController;

    public void Init()
    {
        // 월드 오브젝트 설정
        SetupToWorldObject();

        _objectPoolController = ControllerManager.Instance.GetController<ObjectPoolController>();
        
        // GameMode.Instance.OnRecvUpdateRewardHandler   += OnRecvUpdateReward;
    
        GameMode.Instance.GetEventHandler<IngamePacketEventHandler>().RegisterClient<UpdateRewardBroadcast>(OnRecvUpdateReward);
    }

    public void SetupToWorldObject()
    {
        _worldObjDict = new Dictionary<Type, List<WorldObjectBase>>();

        var typeArr = Assembly.GetAssembly(typeof(WorldObjectBase)).GetTypes();
        var derivedTypes = typeArr.Where(x => x.IsClass == true &&
                                              x.IsAbstract == false &&
                                              x.IsSubclassOf(typeof(WorldObjectBase)))
                                  .ToList();


        var stack = new Stack<Transform>();
        stack.Push(_objectRoot);

        while (stack.Count > 0)
        {
            var tr = stack.Pop();

            foreach (Transform child in tr)
            {
 
              var worldObj = child.GetComponent<WorldObjectBase>();

                if (worldObj != null)
                {
                    var type = worldObj.GetType();
                    AddWorldObject(worldObj);
                }

                if (child.childCount > 0)
                    stack.Push(child);
            }
        }
    }

    public IEnumerable<T> GetWorldObjEnumerable<T>() where T : WorldObjectBase
    {
        return _worldObjDict.TryGetValue(typeof(T), out var list) ? list.Cast<T>()
                                                                 : Enumerable.Empty<T>();
    }


    public void AddWorldObject<T>(T inObj) where T : WorldObjectBase
    {
        var type = inObj.GetType();

        if(_worldObjDict.TryGetValue(type, out var list) == true)
        {
            list.Add(inObj);
        }
        else
        {
            var objList = new List<WorldObjectBase>();
            objList.Add(inObj);

            _worldObjDict.Add(type, objList);
        }
    }

    public void RemoveWorldObject<T>(T inObj) where T : WorldObjectBase
    {
        if(_worldObjDict.TryGetValue(inObj.GetType(), out var list) == true)
        {
            list.Remove(inObj);
        }
    }

    public List<Vector3> GetBackgroundCanGoTileList()
    {
        List<Vector3> result = new List<Vector3>();
        return result;
    }


    public void PickReward(UpdateRewardBroadcast packet)
    {
        var itemObj = GetWorldObjEnumerable<WorldItem>().FirstOrDefault(x => x.WorldId == packet.worldId);

        if (itemObj == null)
        {
            Debug.LogError($"[{nameof(PickReward)}] itemObject is null or empty!");
            return;
        }

        if(packet.rewardType == (int)ERewardType.GOLD)
           ActionDisplayUI.Show(ActionDisplayUI.ESpawnType.GET_GOLD, packet.gold, itemObj.transform.position + Vector3.up);

        itemObj.PlayGetItem(() => 
        {
            RemoveWorldObject(itemObj);
            _objectPoolController.StoreToPool(itemObj); 
        });
    }


    //-------------
    // Server
    //-------------

    public void OnRecvUpdateReward(UpdateRewardBroadcast inPacket)
    {
        // 아이템 드랍시
        if(inPacket.status == (int)ERewardStatus.DROP_REWARD)
        {
        
            var itemObj = _objectPoolController.SpawnFromPool<WorldItem>();

            if(itemObj == null)
            {
                Debug.LogError($"[{nameof(OnRecvUpdateReward)}] itemObj is null or empty!");
                return;
            }

            itemObj.SetInfo(inPacket.worldId, inPacket.pos.ConvertUnityVector3(), inPacket.rewardType);
            itemObj.PlayDropItem();

            AddWorldObject(itemObj);
        
        }
        else if(inPacket.status == (int)ERewardStatus.PICK_REWARD)
        {
             PickReward(inPacket);
        }
    }
}
