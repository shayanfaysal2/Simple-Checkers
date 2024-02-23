//Commented Because Sdk is not installed.

/*using System;
using System.Collections.Generic;
using System.Linq;
using Firebase;
using Firebase.Auth;
using UnityEngine;

namespace _Project.Scripts.Authentication.Firebase
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

        public void FacebookSignIn(Action onComplete = null, Action<string> onFailure = null)
        {
            FB.Init(SetInit, OnHiddenUnity);

            if (!FB.IsInitialized)
            {
                FB.Init(() =>
                    {
                        if (FB.IsInitialized)
                            FB.ActivateApp();
                        else
                        {
                            onFailure?.Invoke("Couldn't initialize Facebook");
                            Debug.LogError("Couldn't initialize Facebook");
                        }
                    },
                    isGameShown => { Time.timeScale = !isGameShown ? 0 : 1; });
            }
            else
                FB.ActivateApp();
            
            if(!FB.IsInitialized) return;
            
            var credential = global::Firebase.Auth.FacebookAuthProvider.GetCredential(AccessToken.CurrentAccessToken.TokenString);
            var auth = FirebaseAuthenticator.Instance.Auth;
            auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(task => {
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

                var result = task.Result;
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);
            });
        }

        public async void LinkToFacebook(Action<AuthenticationData> onComplete = null, Action<string> onFailure = null)
        {
            FB.Init(SetInit, OnHiddenUnity);

            if (!FB.IsInitialized)
            {
                FB.Init(() =>
                    {
                        if (FB.IsInitialized)
                            FB.ActivateApp();
                        else
                        {
                            onFailure?.Invoke("Couldn't initialize Facebook");
                            Debug.LogError("Couldn't initialize Facebook");
                        }
                    },
                    isGameShown => { Time.timeScale = !isGameShown ? 0 : 1; });
            }
            else
                FB.ActivateApp();
            
            if(!FB.IsInitialized) return;
            
            var credential = global::Firebase.Auth.FacebookAuthProvider.GetCredential(AccessToken.CurrentAccessToken.TokenString);
            var auth = FirebaseAuthenticator.Instance.Auth;

            var isFaulted = false;
            var faultErrorMsg = string.Empty;
            var authenticationData = new AuthenticationData();
            
            await auth.CurrentUser.LinkWithCredentialAsync(credential).ContinueWith(task =>
                {
                    if (task.IsCanceled)
                    {
                        isFaulted = true;
                        faultErrorMsg = "Linking with Facebook was canceled.";
                        Debug.LogError("Linking With Facebook was canceled.");
                        return;
                    }

                    if (task.IsFaulted)
                    {
                        /*if (task.Exception?.InnerException is FirebaseException firebaseException)
                        {
                            Debug.LogError("While linking with Facebook encountered an error: " + task.Exception);
                            
                            var authError = ((AuthError)firebaseException.ErrorCode);

                            if (authError == AuthError.AccountExistsWithDifferentCredentials)
                            {
                                Debug.Log("Merging both accounts.");
                                
                                var currentUserId = auth.CurrentUser.UserId;
                                var currentEmail = auth.CurrentUser.Email;
                                var currentDisplayName = auth.CurrentUser.DisplayName;
                                var currentPhotoUrl = auth.CurrentUser.PhotoUrl;

                                // Sign in with the new credentials.
                                auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWith(mergeTask => {
                                    if (mergeTask.IsCanceled) {
                                        onFailure?.Invoke("Failed to Merging both accounts.");
                                        Debug.LogError("SignInAndRetrieveDataWithCredentialAsync was canceled.");
                                        return;
                                    }
                                    if (mergeTask.IsFaulted) {
                                        onFailure?.Invoke("Failed to Merging both accounts.");
                                        Debug.LogError("SignInAndRetrieveDataWithCredentialAsync encountered an error: " + mergeTask.Exception);
                                        return;
                                    }

                                    var result = mergeTask.Result;
                                    Debug.LogFormat("User signed in successfully: {0} ({1})",
                                        result.User.DisplayName, result.User.UserId);

                                    // TODO: Merge app specific details using the newUser and values from the
                                    // previous user, saved above.
                                    firebaseAuthenticator.UpdateAuthenticatedData(currentDisplayName, currentPhotoUrl.ToString(),
                                        () =>
                                        {
                                            onComplete?.Invoke();
                                        },()=>
                                        {
                                            onFailure?.Invoke("Failed to update previous account's details");
                                        });
                                });
                            }
                        }#1#
                        
                        isFaulted = true;
                        faultErrorMsg = "While linking with Facebook encountered an error: " + task.Exception;
                        Debug.LogError("While linking with Facebook encountered an error: " + task.Exception);
                        return;
                    }

                    var result = task.Result;
                    authenticationData.UserId = result.User.UserId;
                    authenticationData.EmailAddress = result.User.Email;
                    authenticationData.DisplayName = result.User.DisplayName;
                    authenticationData.Provider = result.User.ProviderId;
                    authenticationData.PhotoUrl = result.User.PhotoUrl;
                    authenticationData.IsAnonymous = result.User.IsAnonymous;
                    var userInfo = result.User.ProviderData.ToList();
                    var providerIds = new List<string>();
                    for (int i = 0; i < userInfo.Count; i++)
                    {
                        var providerId = userInfo[i].ProviderId;
                        providerIds.Add(providerId);
                    }
                    authenticationData.Providers = providerIds;
                    Debug.LogFormat("Credentials successfully linked to Firebase user: {0} ({1})",
                        result.User.DisplayName, result.User.UserId);
                });
            
            if (isFaulted)
            {
                onFailure?.Invoke(faultErrorMsg);
                return;
            }
            
            onComplete?.Invoke(authenticationData);
        }

        public async void UnLinkFacebookAccount(Action<AuthenticationData> onComplete = null, Action<string> onFailure = null)
        {
            var firebaseAuthenticator = FirebaseAuthenticator.Instance;
            var auth = firebaseAuthenticator.Auth;

            var hasFacebookProvider = firebaseAuthenticator.Providers.Contains("facebook.com");
            
            if (!hasFacebookProvider)
            {
                onFailure?.Invoke("This Email is not Linked to Google");
                Debug.LogError("This Email is not Linked to Google");
                return;
            }

            var isFaulted = false;
            var faultedMsg = "";
            var authenticationData = new AuthenticationData();
            
            await auth.CurrentUser.UnlinkAsync("facebook.com").ContinueWith(unlinkingTask =>
            {
                if (unlinkingTask.IsCanceled)
                {
                    Debug.LogError("UnlinkAsync was canceled.");
                    isFaulted = true;
                    faultedMsg = "UnlinkAsync was canceled.";
                    return;
                }
            
                if (unlinkingTask.IsFaulted)
                {
                    Debug.LogError("UnlinkAsync encountered an error: " + unlinkingTask.Exception);
                    isFaulted = true;
                    faultedMsg = "UnlinkAsync encountered an error: " + unlinkingTask.Exception;
                    return;
                }
            
                // The user has been unlinked from the provider.
                var unlinkingResult = unlinkingTask.Result;
                
                authenticationData.UserId = unlinkingResult.User.UserId;
                authenticationData.EmailAddress = unlinkingResult.User.Email;
                authenticationData.DisplayName = unlinkingResult.User.DisplayName;
                authenticationData.Provider = unlinkingResult.User.ProviderId;
                authenticationData.PhotoUrl = unlinkingResult.User.PhotoUrl;
                authenticationData.IsAnonymous = unlinkingResult.User.IsAnonymous;
                var userInfo = unlinkingResult.User.ProviderData.ToList();
                var providerIds = new List<string>();
                for (int i = 0; i < userInfo.Count; i++)
                {
                    var providerId = userInfo[i].ProviderId;
                    providerIds.Add(providerId);
                }
                authenticationData.Providers = providerIds;
                
                Debug.LogFormat("Credentials successfully unlinked from user: {0} ({1})",
                    unlinkingResult.User.DisplayName, unlinkingResult.User.UserId);
            });

            if (isFaulted)
            {
                onFailure?.Invoke(faultedMsg);
                return;
            }
            
            onComplete?.Invoke(authenticationData);
        }
    }
}*/