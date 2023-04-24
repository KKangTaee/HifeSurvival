using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading.Tasks;

public class SceneManager
{
    private static SceneManager _instance;

    public static SceneManager Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SceneManager();

            return _instance;
        }
    }



    //-------------
    // variable
    //-------------

    private const int MILLISECOND_DELAY = 33;
    private const int MILLISECOND_TIME_OUT = MILLISECOND_DELAY * 200;
    private const string SCENE_LOADING_PREFAB_PATH = "Prefabs/Scenes/SceneLoading";
    
    public const string SCENE_NAME_LOBBY = "LobbyScene";
    public const string SCENE_NAME_TITLE = "TitleScene";


    private Dictionary<string, SceneBase> _sceneDic = new Dictionary<string, SceneBase>()
    {
        {nameof(LobbyScene), new LobbyScene()},
        {nameof(TitleScene), new TitleScene()}
    };

    private SceneLoading _sceneLoading;

    public string CurrSceneName { get; private set; }



    //------------
    // function
    //------------


    private async Task<bool> LoadSceneAsync(string inSceneName)
    {
        return await CheckTimeout(UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(inSceneName));
    }

    private async Task<bool> UnloadSceneAsync(string inSceneName)
    {
        return await CheckTimeout(UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(inSceneName));
    }

    private async Task<bool> CheckTimeout(AsyncOperation waiter)
    {
        int timer = 0;

        while (waiter.isDone == false)
        {
            if (timer > MILLISECOND_TIME_OUT)
                return false;

            await Task.Delay(MILLISECOND_DELAY);
            timer += MILLISECOND_DELAY;
        }

        return true;
    }

    public async Task<bool> Load(string inSceneName)
    {
        bool isSuccess = false;

        if (_sceneDic.TryGetValue(inSceneName, out var scene) == true)
        {
            if(scene == null)
                return false;
                
            if (scene is ISceneLoad load)
            {
                isSuccess &= await load.PrevLoadAsync();

                isSuccess &= await LoadSceneAsync(scene.SceneName);

                isSuccess &= await load.PostLoadAsync();
            }
            else
            {
                isSuccess = await LoadSceneAsync(scene.SceneName);
            }
        }

        return isSuccess;
    }


    public async Task<bool> Unload(string inSceneName)
    {
        bool isSuccess = false;

        if (_sceneDic.TryGetValue(inSceneName, out var scene) == true)
        {
            if (scene is ISceneUnload unload)
            {
                isSuccess &= await unload.PrevUnloadAsync();

                isSuccess &= await UnloadSceneAsync(scene.SceneName);

                isSuccess &= await unload.PostUnloadAsync();
            }
            else
            {
                isSuccess &= await UnloadSceneAsync(scene.SceneName);
            }
        }

        return isSuccess;
    }    


    public async Task ChangeScene(string inSceneName)
    {
        if(CurrSceneName == inSceneName)
           return;

        bool isSuccess = false;
        
        await ShowLoadingAsync();

        // Unload
        if(CurrSceneName != null)
           isSuccess &= await Unload(CurrSceneName);
    
        // Load
        isSuccess &= await Load(inSceneName);

        // 성공했다면..? 이름 변경
        if(isSuccess == true)
           CurrSceneName = inSceneName;
    
        await HideLoadingAsync();
    }


    public async Task ShowLoadingAsync()
    {
        if(_sceneLoading == null)
        {
            var prefab    = Resources.Load<SceneLoading>(SCENE_LOADING_PREFAB_PATH);   
            _sceneLoading = MonoBehaviour.Instantiate<SceneLoading>(prefab);
            MonoBehaviour.DontDestroyOnLoad(_sceneLoading); 
        }
        
        _sceneLoading.gameObject.SetActive(true);
        var waiter = new AsyncWaiting();

        _sceneLoading?.Show(_=>
        {
            waiter.Signal();
        });

        await waiter.Wait();
    }


    public async Task HideLoadingAsync()
    {
        var waiter = new AsyncWaiting();

        _sceneLoading?.Hide(_=>
        {
            waiter.Signal();
        });

        await waiter.Wait();

        _sceneLoading.gameObject.SetActive(false);
    }
}
