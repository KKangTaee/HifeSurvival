using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using System.Threading.Tasks;
using Google;
using System;


// NOTE@taeho.kang 반드시 모노비헤이비어를 상속받아야 함.
public class FirebaseAuthManager : MonoBehaviour
{
    private static FirebaseAuthManager _instance;

    public static FirebaseAuthManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<FirebaseAuthManager>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("FirebaseAuthManager");
                    _instance = obj.AddComponent<FirebaseAuthManager>();
                }
            }
            return _instance;
        }
    }


    public FirebaseAuth firebaseAuth;
    public Action<bool> loginSuccessCallback;

    private string webClientId = "213361373065-efemj6qb2jebo50ptdlv4r3hdkrttvev.apps.googleusercontent.com"; // Google Developer Console에서 생성한 클라이언트 ID를 여기에 붙여넣습니다.

    public async Task Init(Action<bool> inLoginSuccessCallback = null)
    {
        await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            firebaseAuth = FirebaseAuth.DefaultInstance;
            loginSuccessCallback = inLoginSuccessCallback;
            Debug.Log($"[{nameof(Init)}] 초기화 성공!");
        });
    }

                                    
    public async void SignInWithGoogle()
    {
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = webClientId
        };

        Debug.Log($"[SignInWithGoogle] 클라이언트 아이디 : {webClientId}");

#if UNITY_EDITOR
        // 에디터에서 테스트 중일 때 Google 로그인 시뮬레이션
        // string simulatedUserId = "SimulatedUserId";
        // string simulatedDisplayName = "SimulatedDisplayName";
        // OnGoogleLoginSuccess(simulatedUserId, simulatedDisplayName);
#else
    // 실제 기기에서 실행 중일 때 Google 로그인 실행
    try
    {
        var user = await GoogleSignIn.DefaultInstance.SignIn();
        OnGoogleLoginSuccess(user.IdToken, user.AuthCode);
    }
    catch (Exception e)
    {
        Debug.LogError("Google Sign In error: " + e);
    }
#endif
    }


    public void SignOutWithGoogle()
    {
        GoogleSignIn.DefaultInstance.SignOut();
    }



    public void SignUpWithEmail(string email, string password)
    {
        firebaseAuth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("SignUpWithEmail(): Sign up failed.");
                return;
            }

            Debug.Log("SignUpWithEmail(): Sign up successful.");
            // 회원가입 성공 후 처리할 코드를 여기에 추가하세요.
        });
    }

    public void SignInWithEmail(string email, string password)
    {
        firebaseAuth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("SignInWithEmail(): Sign in failed.");
                return;
            }

            Debug.Log("SignInWithEmail(): Sign in successful.");
            // 로그인 성공 후 처리할 코드를 여기에 추가하세요.
        });
    }

    private void OnGoogleLoginSuccess(string idToken, string accessToken)
    {
        Credential credential = GoogleAuthProvider.GetCredential(idToken, accessToken);
        firebaseAuth.SignInWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.LogError("로그인에 실패했습니다: " + task.Exception);
                loginSuccessCallback?.Invoke(false);
                return;
            }

            FirebaseUser newUser = task.Result;
            Debug.LogFormat("로그인에 성공했습니다: {0} ({1})", newUser.DisplayName, newUser.UserId);
            loginSuccessCallback?.Invoke(true);
            // Firestore에 데이터를 저장하거나 불러오려면 이 부분에 코드를 추가하세요.
            FirestoreManager.Instance.LoadUserData(newUser.UserId);
        });
    }
}