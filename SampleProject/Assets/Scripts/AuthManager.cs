using Firebase.Auth;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AuthType
{
    Guest,
    Google,
    Apple,
}

public class AuthManager : Singleton<AuthManager>
{
    private FirebaseAuth auth;
    private string fbUserID;

    public void Start()
    {
#if UNITY_ANDROID

        PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder()
            .EnableSavedGames()
            .RequestIdToken()
            .Build();

        PlayGamesPlatform.InitializeInstance(config);
        PlayGamesPlatform.DebugLogEnabled = true;
        PlayGamesPlatform.Activate();
#elif UNITY_IOS
 
        GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
#endif

        auth = FirebaseAuth.DefaultInstance;
    }

    public void Login(AuthType authType, Action<bool> resultAction = null)
    {
        if (Social.localUser.authenticated) return; //이미 로그인 중일 경우 리턴

        Social.localUser.Authenticate(result =>
        {
            //로그인 시도 성공
            if (result)
            {
                switch (authType)
                {
                    case AuthType.Guest:
                        break;
                    case AuthType.Google:
                        StartCoroutine(GPGSLoginReq(resultAction));
                        break;
                    case AuthType.Apple:
                        break;
                }
            }
            //로그인 시도 실패
            else
            {

            }
        });
    }

    public void Logout(Action<bool> resultAction = null)
    {
        //접속중이면 로그아웃
        if (Social.localUser.authenticated)
        {
            PlayGamesPlatform.Instance.SignOut();
            auth.SignOut();
            resultAction?.Invoke(true);
        }
        else
        {
            resultAction?.Invoke(false);
        }
    }

    public IEnumerator GPGSLoginReq(Action<bool> resultAction = null)
    {
        var localUser = (PlayGamesLocalUser)Social.localUser;
        string it = localUser.GetIdToken();

        //id토큰 받아오면 진행
        while (string.IsNullOrEmpty(((PlayGamesLocalUser)Social.localUser).GetIdToken()))
        {
            yield return null;
        }

        string idToken = ((PlayGamesLocalUser)Social.localUser).GetIdToken();

        Credential credential = GoogleAuthProvider.GetCredential(idToken, null);
        auth.SignInWithCredentialAsync(credential).ContinueWith(task =>
        {
            if (task.IsCanceled || task.IsFaulted)
            {
                resultAction?.Invoke(false);
            }

            if (task.IsCompleted)
            {
                FirebaseUser user = task.Result;
                fbUserID = user.UserId;

                resultAction?.Invoke(true);
            }
        });
    }
}
