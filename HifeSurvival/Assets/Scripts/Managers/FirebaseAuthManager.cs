using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using System.Threading.Tasks;
using Google;
using System;


// NOTE@taeho.kang 반드시 모노비헤이비어를 상속받아야 함.
// < 파이어베이스 구글로그인 구현 과정>
// 1. Firebase Console에서 프로젝트 만들기.
// 2. 인증들어가서 "로그인 제공업체"에 "Google" 추가.
// 3. 유니티에서 Keystore 생성 및 SHA-1 키 뽑기.
// 4. Firebase Console에서 프로젝트 설정 - 안드로이드 - 여기서 디지털 지문추가에 SHA-1키 등록.

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
    
    public AsyncWaiting _waiting = new AsyncWaiting();

    private string webClientId = "213361373065-efemj6qb2jebo50ptdlv4r3hdkrttvev.apps.googleusercontent.com"; // Google Developer Console에서 생성한 클라이언트 ID를 여기에 붙여넣습니다.

    public async Task Init()
    {
        await FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            firebaseAuth = FirebaseAuth.DefaultInstance;
            Debug.Log($"[{nameof(FirebaseAuthManager)}] 초기화 성공!");
        });

        await _waiting.Wait();
    }

                                    
    public async void SignInWithGoogle()
    {
        GoogleSignIn.Configuration = new GoogleSignInConfiguration
        {
            RequestIdToken = true,
            WebClientId = webClientId
        };

        Debug.Log($"[SignInWithGoogle] 클라이언트 아이디 : {webClientId}");

#if! UNITY_EDITOR
        // 에디터에서 테스트 중일 때 Google 로그인 시뮬레이션
        // string simulatedUserId = "SimulatedUserId";
        // string simulatedDisplayName = "SimulatedDisplayName";
        // OnGoogleLoginSuccess(simulatedUserId, simulatedDisplayName);
        _waiting.Signal();
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
            
            _waiting.Signal();
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
                return;
            }

            FirebaseUser newUser = task.Result;
            Debug.LogFormat("로그인에 성공했습니다: {0} ({1})", newUser.DisplayName, newUser.UserId);
        
            // 유저아이디 처리.
            ServerData.Instance.SetUserID(newUser.UserId);
            _waiting.Signal();
        });
    }
}