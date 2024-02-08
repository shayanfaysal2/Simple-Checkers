using System;
//using _Project.Scripts.UI;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Authentication
{
    public class Authenticator : MonoBehaviour
    {
        private IAuthenticationData _authenticationData;
        private ISignIn _authenticatorSignIn;
        private ISignOut _authenticatorSignOut;
        private ISignUp _authenticatorSignUp;
        private ISignInGuest _authenticatorSignInGuest;
        private ISignInGoogle _authenticatorGoogle;
        private ISignInFacebook _authenticatorFacebook;
        private IGoogleLinker _googleLinker;
        private IUpdateDisplayName _updateDisplayName;
        
        private AuthenticationData _authenticatedData;

        public static event Action<AuthenticationData> OnAuthenticationSuccessful;
        public static event Action OnAuthenticationFailed;

        public static event Action OnSigningIn;
        public static event Action OnSignedIn;
        public static event Action<string> OnFailedToSignIn;
        
        public static event Action OnSigningUp;
        public static event Action OnSignedUp;
        public static event Action<string> OnFailedToSignUp;
        
        public static event Action OnSigningOut;
        public static event Action OnSignedOut;
        public static event Action<string> OnFailedToSignOut;

        public static event Action OnDisplayNameSet;
        
        public bool SignedIn => _authenticationData?.SignedIn ?? false;

        private void Awake()
        {
            _authenticatorSignIn = GetComponent<ISignIn>();
            _authenticatorSignOut = GetComponent<ISignOut>();
            _authenticatorSignUp = GetComponent<ISignUp>();
            _authenticatorSignInGuest = GetComponent<ISignInGuest>();
            _authenticatorGoogle = GetComponent<ISignInGoogle>();
            _authenticatorFacebook = GetComponent<ISignInFacebook>();
            _authenticationData = GetComponent<IAuthenticationData>();
            _googleLinker = GetComponent<IGoogleLinker>();
            _updateDisplayName = GetComponent<IUpdateDisplayName>();
        }

        private void OnEnable()
        {
            _authenticationData.OnSignedIn += LoggedIn;
            _authenticationData.OnSignedOut += FailedToLogInOrHasLoggedOut;

            /*AuthenticationUI.OnSignInGuest += SignInAsGuest;
            AuthenticationUI.OnSignInWithCredentials += SignInWithCredentials;
            AuthenticationUI.OnSignUpWithCredentials += SignUp;
            AuthenticationUI.OnSignInGoogle += SignInGoogle;
            AuthenticationUI.OnSignInFacebook += SignInFacebook;
            ProfilingUI.DoSignOut += SignOut;*/
            
            _authenticationData.CheckForDataAvailability();
        }


        private void LoggedIn()
        {
            _authenticatedData = new AuthenticationData()
            {
                UserId = _authenticationData.UserId,
                DisplayName = _authenticationData.DisplayName,
                EmailAddress = _authenticationData.EmailAddress,
                PhotoUrl = _authenticationData.PhotoUrl
            };
            OnAuthenticationSuccessful?.Invoke(_authenticatedData);
        }

        private void FailedToLogInOrHasLoggedOut() => OnAuthenticationFailed?.Invoke();


        private bool _signingIn;

        private void SignInWithCredentials(string email, string pass)
        {
            if (_signingIn) return;
            _signingIn = _authenticatorSignIn != null;
            OnSigningIn?.Invoke();
            _authenticatorSignIn?.SignIn(email, pass,
                () =>
                {
                    OnSignedIn?.Invoke();
                    _signingIn = false;
                }, (exc) =>
                {
                    OnFailedToSignIn?.Invoke(exc);
                    _signingIn = false;
                });
        }

        private bool _signingInAsGuest;

        private void SignInAsGuest()
        {
            if(_signingInAsGuest) return;
            _signingInAsGuest = _authenticatorSignInGuest != null;
            OnSigningIn?.Invoke();
            _authenticatorSignInGuest?.SignInGuest(
                () =>
                {
                    OnSignedIn?.Invoke();
                    _signingInAsGuest = false;
                }, (exc) =>
                { 
                    OnFailedToSignIn?.Invoke(exc);
                    _signingInAsGuest = false;  
                });
        }
        
        private bool _signingUp;

        private void SignUp(string email, string pass)
        {
            if (_signingUp) return;
            _signingUp = _authenticatorSignUp != null;
            OnSigningUp?.Invoke();
            _authenticatorSignUp?.SignUp(email, pass, 
                () =>
                {
                    OnSignedUp?.Invoke();
                    _signingUp = false;
                }, (exc) =>
                {
                    OnFailedToSignUp?.Invoke(exc);
                    _signingUp = false;
                });
        }

        private bool _signingInAsGoogle;

        private void SignInGoogle()
        {
            if(_signingInAsGoogle) return;
            _signingInAsGoogle = _authenticatorGoogle != null;
            OnSigningIn?.Invoke();
            _authenticatorGoogle?.GoogleSignIn(
                () =>
                {
                    OnSignedIn?.Invoke();
                    _signingInAsGoogle = false;
                }, (exc) =>
                {
                    OnFailedToSignIn?.Invoke(exc);
                    _signingInAsGoogle = false;
                });
        }

        private bool _signingInAsFacebook;
        private void SignInFacebook()
        {
            if(_signingInAsFacebook) return;
            _signingInAsFacebook = _authenticatorFacebook != null;
            OnSigningIn?.Invoke();
            _authenticatorFacebook?.FacebookSignIn(
                () =>
                {
                    OnSignedIn?.Invoke();
                    _signingInAsFacebook = false;
                }, (exc) =>
                {
                    OnFailedToSignIn?.Invoke(exc);
                    _signingInAsFacebook = false;
                });
        }

        private bool _linkingToGoogle;
        public void LinkToGoogle()
        {
            if(_linkingToGoogle) return;
            _linkingToGoogle = _googleLinker != null;
            _googleLinker?.LinkToGoogle(() =>
            {
                _linkingToGoogle = false;
            }, (exc) =>
            {
                _linkingToGoogle = false;
            });
        }
        
        public void SignOut()
        {
            if (!SignedIn)
            {
                OnFailedToSignOut?.Invoke("No User Logged In.");
                return;
            }
            OnSigningOut?.Invoke();
            _authenticatorSignOut?.SignOut();
            OnSignedOut?.Invoke();
        }

        private void OnDisable()
        {
            _authenticationData.OnSignedIn -= LoggedIn;
            _authenticationData.OnSignedOut -= FailedToLogInOrHasLoggedOut;
            
            /*AuthenticationUI.OnSignInGuest -= SignInAsGuest;
            AuthenticationUI.OnSignInWithCredentials -= SignInWithCredentials;
            AuthenticationUI.OnSignUpWithCredentials -= SignUp;
            AuthenticationUI.OnSignInGoogle -= SignInGoogle;
            AuthenticationUI.OnSignInFacebook -= SignInFacebook;
            ProfilingUI.DoSignOut -= SignOut;*/
        }
    }

    public struct AuthenticationData
    {
        public string UserId;
        public string DisplayName;
        public string EmailAddress;
        public Uri PhotoUrl;
    }
}