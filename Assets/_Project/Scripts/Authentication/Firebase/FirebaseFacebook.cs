/*using System;
using Facebook.Unity;
using UnityEngine;

namespace _Project.Scripts.Authentication
{
    public class FirebaseFacebook : MonoBehaviour , ISignInFacebook
    {
        void SetInit()
        {
            if (FB.IsLoggedIn)
            {
                Debug.Log("Facebook is Logged in!");
                var s = "client token" + FB.ClientToken + 
                        "User Id" + AccessToken.CurrentAccessToken.UserId +
                        "token string" + AccessToken.CurrentAccessToken.TokenString;
            }
            else
            {
                Debug.Log("Facebook is not Logged in!");
            }
            //DealWithFbMenus(FB.IsLoggedIn);
        }

        void OnHiddenUnity(bool isGameShown) => Time.timeScale = !isGameShown ? 0 : 1;

        public void FacebookSignIn(Action onComplete = null, Action onFailure = null)
        {
            FB.Init(SetInit, OnHiddenUnity);

            if (!FB.IsInitialized)
            {
                FB.Init(() =>
                    {
                        if (FB.IsInitialized)
                            FB.ActivateApp();
                        else
                            print("Couldn't initialize");
                    },
                    isGameShown => { Time.timeScale = !isGameShown ? 0 : 1; });
            }
            else
                FB.ActivateApp();
            
            var credential =
                Firebase.Auth.FacebookAuthProvider.GetCredential(AccessToken.CurrentAccessToken.TokenString);
            var auth = FirebaseAuthenticator.Instance.Auth;
            auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task => {
                if (task.IsCanceled) {
                    Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                    return;
                }

                var result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);
            });
        }
    }
}*/