using System;
using UnityEngine;

namespace _Project.Scripts.Authentication.Firebase
{
    public class FirebaseGoogleLinker : MonoBehaviour, IGoogleLinker
    {
        public void LinkToGoogle(Action onComplete = null, Action<string> onFailure = null)
        {
            var firebaseAuthenticator = FirebaseAuthenticator.Instance;
            var auth = firebaseAuthenticator.Auth;
            var credential = firebaseAuthenticator.ActiveCredential;
            auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWith(task => {
                if (task.IsCanceled) {
                    onFailure?.Invoke("LinkWithCredentialAsync was canceled.");
                    Debug.LogError("LinkWithCredentialAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    onFailure?.Invoke("LinkWithCredentialAsync encountered an error: " + task.Exception);
                    Debug.LogError("LinkWithCredentialAsync encountered an error: " + task.Exception);
                    return;
                }

                var result = task.Result;
                onComplete?.Invoke();
                Debug.LogFormat("Credentials successfully linked to Firebase user: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);
            });
        }
    }
}
