using System;
using Google;
using UnityEngine;

namespace _Project.Scripts.Authentication.Firebase
{
    /*public class FirebaseGoogle : MonoBehaviour, ISignInGoogle
    {
        [SerializeField] private string _googleWebApi;

        private GoogleSignInConfiguration _configuration;

        private void Start()
        {
            _configuration = new GoogleSignInConfiguration()
            {
                WebClientId = _googleWebApi,
                RequestIdToken = true,
                RequestEmail = true,
                UseGameSignIn = false
            };
        }

        public void GoogleSignIn(Action onComplete = null, Action<string> onFailure = null)
        {
            Google.GoogleSignIn.Configuration = _configuration;
            Google.GoogleSignIn.DefaultInstance.SignIn().ContinueWith(task =>
            {
                if (task.IsCanceled) {
                    onFailure?.Invoke("Login was canceled.");
                    Debug.LogError($"Login was canceled");
                    return;
                }
                if (task.IsFaulted) {
                    onFailure?.Invoke("Encountered an error: " + task.Exception);
                    Debug.LogError("Encountered an error: " + task.Exception);
                    return;
                }

                var firebaseAuthenticator = FirebaseAuthenticator.Instance;
                var googleIdToken = task.Result.IdToken;
                firebaseAuthenticator.ActiveCredential = global::Firebase.Auth.GoogleAuthProvider.GetCredential(googleIdToken, null);

                firebaseAuthenticator.Auth.SignInAndRetrieveDataWithCredentialAsync(firebaseAuthenticator.ActiveCredential).ContinueWith(task => {
                    if (task.IsCanceled) {
                        onFailure?.Invoke("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                        Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                        return;
                    }
                    if (task.IsFaulted) {
                        onFailure?.Invoke("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                        Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + task.Exception);
                        return;
                    }

                    onComplete?.Invoke();
                    var result = task.Result;
                    Debug.LogFormat("User signed in successfully: {0} ({1})",
                        result.User.DisplayName, result.User.UserId);
                });
            });
            
        }
    }*/
}