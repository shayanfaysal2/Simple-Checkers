using System;
using System.Collections.Generic;
using System.Linq;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;

namespace _Project.Scripts.Authentication.Firebase
{
    public class FirebaseAuthenticatorHandler : MonoBehaviour , IAuthenticator , IAuthenticationData
    {
        public FirebaseUser user { private set; get; }
        public string UserId { private set; get; }
        public string DisplayName { get; private set; }
        public string EmailAddress { get; private set; }
        public Uri PhotoUrl { get; private set; }
        public string Provider { get; private set; }
        public List<string> Providers { get; private set; }
        public bool IsAnonymous { get; private set; }
        public bool SignedIn { get; private set; }
        
        public FirebaseAuth Auth { private set; get; }
        
        public static FirebaseAuthenticatorHandler Instance { private set; get; }

        public event Action OnSignedIn; 
        public event Action OnSignedOut;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                return;
            }

            Instance = this;
        }

        public void InitializeFirebaseAuthentication() 
        {
            Auth = FirebaseAuth.DefaultInstance;
            Auth.StateChanged += AuthStateChanged;
            AuthStateChanged(this, null);
        }
        
        void AuthStateChanged(object sender, System.EventArgs eventArgs)
        {
            List<IUserInfo> providers;
            if (Auth.CurrentUser == user)
            {
                if (user == null)
                {
                    OnSignedOut?.Invoke();
                    return;
                }

                IsAnonymous = user.IsAnonymous;
                UserId = user.UserId ?? "";
                DisplayName = user.DisplayName ?? "";
                EmailAddress = user.Email ?? "";
                PhotoUrl = user.PhotoUrl ?? default;
                Provider = user.ProviderId ?? "";
                Providers ??= new List<string>();
                if(Providers.Count > 0) Providers.Clear();
                providers = user.ProviderData.ToList();
                for (int i = 0; i < providers.Count; i++)
                {
                    var provider = providers[i].ProviderId;
                    Providers.Add(provider);
                }
                
                OnSignedIn?.Invoke();
                EventManager.LoginSuccessful();

                return;
            }
            
            SignedIn = user != Auth.CurrentUser && Auth.CurrentUser != null
                                                && Auth.CurrentUser.IsValid();
            if (!SignedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
                OnSignedOut?.Invoke();
                return;
            }
            
            user = Auth.CurrentUser;

            if (user == null) return;
            
            Debug.Log("Signed in: " + user.UserId);
            Debug.Log("Provider Id: " + user.ProviderId);
            
            IsAnonymous = user.IsAnonymous;
            UserId = user.UserId ?? "";
            DisplayName = user.DisplayName ?? "";
            EmailAddress = user.Email ?? "";
            PhotoUrl = user.PhotoUrl ?? default;
            Provider = user.ProviderId ?? "";
            Providers ??= new List<string>();
            if(Providers.Count > 0) Providers.Clear();
            providers = user.ProviderData.ToList();
            for (int i = 0; i < providers.Count; i++)
            {
                var provider = providers[i].ProviderId;
                Providers.Add(provider);
            }
            OnSignedIn?.Invoke();
            EventManager.LoginSuccessful();
        }
        
        public void UpdateDisplayName(string displayName, Action onCompleted = null, Action onFailed = null)
        {
            var currentUser = Auth.CurrentUser;
            if (currentUser == null) return;
            var profile = new UserProfile
            {
                DisplayName = displayName,
                PhotoUrl = currentUser.PhotoUrl,
            };
            currentUser.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }
                
                Debug.Log("User profile updated successfully.");
                EventManager.DisplayNameSet();
            });
        }

        public void UpdateProfileImage(string photoUrl, Action onCompleted = null, Action onFailed = null)
        {
            var currentUser = Auth.CurrentUser;
            if (currentUser == null) return;
            var profile = new UserProfile
            {
                DisplayName = currentUser.DisplayName,
                PhotoUrl = new Uri(photoUrl),
            };
            currentUser.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }
                
                Debug.Log("User profile updated successfully.");
            });
        }

        public void UpdateAuthenticatedData(string displayName, string photoUrl, Action onCompleted = null, Action onFailed = null)
        {
            var currentUser = Auth.CurrentUser;
            if (currentUser == null) return;
            var profile = new UserProfile
            {
                DisplayName = displayName,
                PhotoUrl = new Uri(photoUrl),
            };
            currentUser.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }
                
                Debug.Log("User profile updated successfully.");
            });
        }
        
        public void SignUp(string email, string password, Action onComplete = null, Action<string> onFailure = null)
        {
            Auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
                if (task.IsCanceled) {
                    onFailure?.Invoke("CreateUserWithEmailAndPasswordAsync was canceled.");
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    onFailure?.Invoke("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                // Firebase user has been created.
                var result = task.Result;
                onComplete?.Invoke();
                Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);
            });
        }

        public void SignIn(string email, string password, Action onComplete = null, Action<string> onFailure = null)
        {
            Auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
                if (task.IsCanceled) {
                    onFailure?.Invoke("SignInWithEmailAndPasswordAsync was canceled.");
                    Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    onFailure?.Invoke("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                var result = task.Result;
                onComplete?.Invoke();
                Debug.LogFormat("User signed in successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
            });
        }
        
        public void SignInGuest(Action onComplete = null, Action<string> onFailure = null)
        {
            Auth.SignInAnonymouslyAsync().ContinueWith(task => {
                if (task.IsCanceled) {
                    onFailure?.Invoke("SignInAnonymouslyAsync was canceled.");
                    Debug.LogError("SignInAnonymouslyAsync was canceled.");
                    return;
                }
                if (task.IsFaulted) {
                    onFailure?.Invoke("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                    Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                    return;
                }

                var result = task.Result;
                onComplete?.Invoke();
                Debug.LogFormat("User signed in successfully: {0} ({1})",
                    result.User.DisplayName, result.User.UserId);
            });
        }
        
        public void SignOut() => Auth.SignOut();

        void OnDestroy()
        {
            Auth.StateChanged -= AuthStateChanged;
            Auth = null;
        }
    }
}