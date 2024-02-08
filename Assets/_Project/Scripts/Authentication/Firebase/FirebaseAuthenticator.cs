using System;
using Firebase.Auth;
using UnityEngine;

namespace _Project.Scripts.Authentication.Firebase
{
    public class FirebaseAuthenticator : MonoBehaviour , ISignIn, ISignOut, ISignInGuest, IUpdateDisplayName, ISignUp ,IAuthenticationData
    {
        public FirebaseUser user { private set; get; }
        public string UserId { private set; get; }
        public string DisplayName { get; private set; }
        public string EmailAddress { get; private set; }
        public Uri PhotoUrl { get; private set; }
        public bool SignedIn { get; private set; }
        
        public FirebaseAuth Auth { private set; get; }
        public Credential ActiveCredential { set; get; } 
        
        public static FirebaseAuthenticator Instance { private set; get; }

        public event Action OnSignedIn; 
        public event Action OnSignedOut;
        public event Action OnDisplayNameSet;

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            InitializeFirebase();
        }

        void InitializeFirebase() 
        {
            Auth = global::Firebase.Auth.FirebaseAuth.DefaultInstance;
            Auth.StateChanged += AuthStateChanged;
            AuthStateChanged(this, null);
        }
        
        public void CheckForDataAvailability() => AuthStateChanged(this, null);


        void AuthStateChanged(object sender, System.EventArgs eventArgs)
        {
            if (Auth.CurrentUser == user)
            {
                if (user == null)
                {
                    OnSignedOut?.Invoke();
                    return;
                }

                UserId = user.UserId ?? "";
                DisplayName = user.DisplayName ?? "";
                EmailAddress = user.Email ?? "";
                PhotoUrl = user.PhotoUrl ?? default;
                OnSignedIn?.Invoke();
                return;
            }
            
            SignedIn = user != Auth.CurrentUser && Auth.CurrentUser != null
                                                && Auth.CurrentUser.IsValid();
            if (!SignedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
                OnSignedOut?.Invoke();
            }
            
            user = Auth.CurrentUser;
            if (!SignedIn) return;
            
            Debug.Log("Signed in " + user.UserId);
            DisplayName = user.DisplayName ?? "";
            EmailAddress = user.Email ?? "";
            PhotoUrl = user.PhotoUrl ?? default;
            OnSignedIn?.Invoke();
        }
        
        public void SignUp(string email, string password, Action onComplete = null, Action<string> onFailure = null)
        {
            ActiveCredential = EmailAuthProvider.GetCredential(email, password);
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
            ActiveCredential = EmailAuthProvider.GetCredential(email, password);
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
        
        public void SignOut()
        {
            Auth.SignOut();
            //OnSignedOut?.Invoke();
        }

        void OnDestroy()
        {
            Auth.StateChanged -= AuthStateChanged;
            Auth = null;
        }

        public void UpdateDisplayName(string newDisplayName, Action onComplete = null, Action<string> onFailure = null)
        {
            Auth.CurrentUser.UpdateUserProfileAsync(new UserProfile{
                DisplayName = newDisplayName,
                PhotoUrl = Auth.CurrentUser.PhotoUrl,
            }).ContinueWith(task => {
                if (task.IsCanceled)
                {
                    onFailure?.Invoke("UpdateUserProfileAsync was canceled.");
                    Debug.LogError("UpdateUserProfileAsync was canceled.");
                    return;
                }
                if (task.IsFaulted)
                {
                    onFailure?.Invoke("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                DisplayName = newDisplayName;
                Debug.Log("Display name updated successfully");
                onComplete?.Invoke();
                EventManager.DisplayNameSet();
            });
        }
    }
}