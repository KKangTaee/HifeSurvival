using Firebase;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class FirestoreManager : MonoBehaviour
{
    private static FirestoreManager _instance;
    public static FirestoreManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<FirestoreManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("FirestoreManager");
                    _instance = obj.AddComponent<FirestoreManager>();
                }
            }
            return _instance;
        }
    }


    private FirebaseFirestore db;


    public void Init()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => 
        {
            db = FirebaseFirestore.DefaultInstance;
        });
    }


    public void LoadUserData(string userId)
    {
        Debug.Log("LoadUserData 호출!");
        DocumentReference userDocRef = db.Collection("users").Document(userId);
        userDocRef.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("사용자 데이터 로드에 실패했습니다: " + task.Exception);
                return;
            }

            DocumentSnapshot snapshot = task.Result;
            if (snapshot.Exists)
            {
                Debug.Log("사용자 데이터 로드: " + snapshot.Id);
                // 데이터 처리 코드를 여기에 추가하세요.
            }
            else
            {
                Debug.Log("사용자 데이터 없음. 새로운 데이터를 생성합니다.");
                SaveNewUserData(userId);
            }
        });
    }

    public void SaveNewUserData(string userId)
    {
        DocumentReference userDocRef = db.Collection("users").Document(userId);

        // 사용자 데이터를 저장할 때 사용할 초기 값
        Dictionary<string, object> userData = new Dictionary<string, object>
        {
            { "score", 0 },
            { "level", 1 },
        };

        userDocRef.SetAsync(userData).ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.LogError("사용자 데이터 저장에 실패했습니다: " + task.Exception);
                return;
            }

            Debug.Log("사용자 데이터 저장에 성공했습니다.");
        });
    }
}