using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;


//-------------
// interface
//-------------

public interface ISceneLoad
{
    Task<bool> PrevLoadAsync();
    Task<bool> PostLoadAsync();
}

public interface ISceneUnload
{
    Task<bool> PrevUnloadAsync();
    Task<bool> PostUnloadAsync();
}


public abstract class SceneBase 
{
    public abstract string SceneName {get;}
    public abstract void Initialize();
}

public class LobbyScene : SceneBase, ISceneLoad
{
    public override string SceneName => nameof(LobbyScene);

    public override void Initialize()
    {
        
    }

    public async Task<bool> PostLoadAsync()
    {
        await Task.Delay(1000);
        return true;
    }

    public async Task<bool> PrevLoadAsync()
    {
        await Task.Delay(1000);
        return true;
    }
}


public class TitleScene : SceneBase
{
    public override string SceneName => nameof(TitleScene);

    public override void Initialize()
    {

    }
}

public class IngameScene : SceneBase
{
    public override string SceneName => nameof(IngameScene);

    public override void Initialize()
    {

    }
}
