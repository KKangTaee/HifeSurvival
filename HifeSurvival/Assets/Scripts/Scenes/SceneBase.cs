using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx.Async;


//-------------
// interface
//-------------

public interface ISceneLoad
{
    UniTask<bool> PrevLoadAsync();
    UniTask<bool> PostLoadAsync();
}

public interface ISceneUnload
{
    UniTask<bool> PrevUnloadAsync();
    UniTask<bool> PostUnloadAsync();
}


public abstract class SceneBase 
{
    public abstract string SceneName {get;}
}

public class LobbyScene : SceneBase
{
    public override string SceneName => nameof(LobbyScene);
}


public class TitleScene : SceneBase
{
    public override string SceneName => nameof(TitleScene);
}

public class IngameScene : SceneBase, ISceneLoad, IUpdateGameStatus
{
    public override string SceneName => nameof(IngameScene);

    private bool _playStartToken = false;

    public async UniTask<bool> PrevLoadAsync()
    {
        GameMode.Instance.OnUpdateGameModeStatusHandler += OnUpdateGameStatus;
        return true;
    }

    public async UniTask<bool> PostLoadAsync()
    {
        while(_playStartToken == false)
            await UniTask.Yield();

         GameMode.Instance.OnUpdateGameModeStatusHandler -= OnUpdateGameStatus;
        _playStartToken = false;

        return true;
    } 

    public void OnUpdateGameStatus(UpdateGameModeStatusBroadcast packet)
    {
        if((EGameModeStatus)packet.status != EGameModeStatus.PlayStart)
            return;
        
        _playStartToken = true;
    }
}
