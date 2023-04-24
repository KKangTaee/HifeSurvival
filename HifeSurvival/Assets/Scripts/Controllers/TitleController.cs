using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;

public class TitleController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GoToLobby();
    }

    async void GoToLobby()
    {
        await Task.Delay(3000);

        await SceneManager.Instance.ChangeScene(SceneManager.SCENE_NAME_LOBBY);
    }
}
