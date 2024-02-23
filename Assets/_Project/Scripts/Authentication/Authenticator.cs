using System;
using System.Collections.Generic;
//using _Project.Scripts.UI;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.Authentication
{
    public class Authenticator : MonoBehaviour
    {
        private IAuthenticationData _authenticationData;
        private IAuthenticator _authenticator;
        private ISignInGoogle _authenticatorGoogle;
        private ISignInFacebook _authenticatorFacebook;
        private ILinker _linker;
        
        public AuthenticationData AuthenticatedData;

        public static event Action<AuthenticationData> OnAuthenticationSuccessful;
        public static event Action OnAuthenticationFailed;

        public static event Action OnSigningIn;
        public static event Action OnSignedIn;
        public static event Action<string> OnFailedToSignIn;
        
        public static event Action OnSigningUp;
        public static event Action OnSignedUp;
        public static event Action<string> OnFailedToSignUp;
        
        public static event Action OnFetchingProviders;
        public static event Action<List<string>> OnProvidersFetched;
        public static event Action<string> OnFailedToFetchProviders;
        
        public static event Action OnLinkingAccount;
        public static event Action<AuthenticationData> OnAccountLinked;
        public static event Action<string> OnAccountFailedToLink;
        
        public static event Action OnUnlinkingAccount;
        public static event Action<AuthenticationData> OnAccountUnlinked;
        public static event Action<string> OnAccountFailedToUnlink;
        
        public static event Action OnSigningOut;
        public static event Action OnSignedOut;
        public static event Action<string> OnFailedToSignOut;
        
        public bool SignedIn => _authenticationData?.SignedIn ?? false;
        
        public static Authenticator Instance { private set; get; }

        private void Awake()
        {
            if (Instance != null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            
            _authenticator = GetComponent<IAuthenticator>();
            _authenticatorGoogle = GetComponent<ISignInGoogle>();
            _authenticatorFacebook = GetComponent<ISignInFacebook>();
            _authenticationData = GetComponent<IAuthenticationData>();
            _linker = GetComponent<ILinker>();
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
        }


        private void LoggedIn()
        {
            AuthenticatedData = new AuthenticationData()
            {
                IsAnonymous = _authenticationData.IsAnonymous,
                UserId = _authenticationData.UserId,
                DisplayName = _authenticationData.DisplayName,
                EmailAddress = _authenticationData.EmailAddress,
                PhotoUrl = _authenticationData.PhotoUrl,
                Provider = _authenticationData.Provider,
                Providers = _authenticationData.Providers
            };
            OnAuthenticationSuccessful?.Invoke(AuthenticatedData);
        }

        private void FailedToLogInOrHasLoggedOut() => OnAuthenticationFailed?.Invoke();

        #region SignIn/SignOut

        private bool _signingIn;

        private void SignInWithCredentials(string email, string pass)
        {
            if (_signingIn) return;
            _signingIn = _authenticator != null;
            OnSigningIn?.Invoke();
            _authenticator?.SignIn(email, pass,
                () =>
                {
                    _signingIn = false;
                    OnSignedIn?.Invoke();
                }, (exc) =>
                {
                    _signingIn = false;
                    OnFailedToSignIn?.Invoke(exc);
                });
        }

        private bool _signingInAsGuest;

        private void SignInAsGuest()
        {
            if(_signingInAsGuest) return;
            _signingInAsGuest = _authenticator != null;
            OnSigningIn?.Invoke();
            _authenticator?.SignInGuest(
                () =>
                {
                    _signingInAsGuest = false;
                    OnSignedIn?.Invoke();
                }, (exc) =>
                { 
                    _signingInAsGuest = false;  
                    OnFailedToSignIn?.Invoke(exc);
                });
        }
        
        private bool _signingUp;

        private void SignUp(string email, string pass)
        {
            if (_signingUp) return;
            _signingUp = _authenticator != null;
            OnSigningUp?.Invoke();
            _authenticator?.SignUp(email, pass, 
                () =>
                {
                    _signingUp = false;
                    OnSignedUp?.Invoke();
                }, (exc) =>
                {
                    _signingUp = false;
                    OnFailedToSignUp?.Invoke(exc);
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
                    _signingInAsGoogle = false;
                    OnSignedIn?.Invoke();
                }, (exc) =>
                {
                    _signingInAsGoogle = false;
                    OnFailedToSignIn?.Invoke(exc);
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
                    _signingInAsFacebook = false;
                    OnSignedIn?.Invoke();
                }, (exc) =>
                {
                    _signingInAsFacebook = false;
                    OnFailedToSignIn?.Invoke(exc);
                });
        }

        [ContextMenu("SignOut")]
        public void SignOut()
        {
            if (!SignedIn)
            {
                OnFailedToSignOut?.Invoke("No User Logged In.");
                return;
            }
            OnSigningOut?.Invoke();
            _authenticator?.SignOut();
            OnSignedOut?.Invoke();
        }
        #endregion
        
        #region Linking
        
        private bool _gettingAllLinkedProviders;
        public void GetAllLinkedProviders(string email)
        {
            if(_gettingAllLinkedProviders) return;
            _gettingAllLinkedProviders = _linker != null;
            OnFetchingProviders?.Invoke();
            if (string.IsNullOrEmpty(email) || string.IsNullOrWhiteSpace(email))
            {
                _gettingAllLinkedProviders = false;
                OnFailedToFetchProviders?.Invoke("Email that you've entered is null or empty");
                return;
            }
            _linker?.GetAllProvidersOnEmail(email, (providers) =>
            {
                _gettingAllLinkedProviders = false;
                OnProvidersFetched?.Invoke(providers);
            }, (exc) =>
            {
                _gettingAllLinkedProviders = false;
                OnFailedToFetchProviders?.Invoke(exc);
            });
        }
        
        
        private bool _linkingToCustomAccount;
        public void LinkToCustomAccount(string email, string password)
        {
            if(_linkingToCustomAccount) return;
            _linkingToCustomAccount = _linker != null;
            OnLinkingAccount?.Invoke();
            _linker?.LinkAccount(email, password, (authData) =>
            {
                _linkingToCustomAccount = false;
                OnAccountLinked?.Invoke(authData);
            }, (exc) =>
            {
                _linkingToCustomAccount = false;
                OnAccountFailedToLink?.Invoke(exc);
            });
        }
        
        private bool _unlinkingToCustomAccount;
        public void UnlinkFromCustomAccount()
        {
            if(_unlinkingToCustomAccount) return;
            _unlinkingToCustomAccount = _linker != null;
            OnUnlinkingAccount?.Invoke();
            _linker?.UnlinkAccount((authData) =>
            {
                _unlinkingToCustomAccount = false;
                OnAccountUnlinked?.Invoke(authData);
            }, (exc) =>
            {
                _unlinkingToCustomAccount = false;
                OnAccountFailedToUnlink?.Invoke(exc);
            });
        }
        
        
        private bool _linkingToGoogle;
        public void LinkToGoogle()
        {
            if(_linkingToGoogle) return;
            _linkingToGoogle = _authenticatorGoogle != null;
            OnLinkingAccount?.Invoke();
            _authenticatorGoogle?.LinkToGoogle((authData) =>
            {
                _linkingToGoogle = false;
                OnAccountLinked?.Invoke(authData);
            }, (exc) =>
            {
                _linkingToGoogle = false;
                OnAccountFailedToLink?.Invoke(exc);
            });
        }
        
        private bool _unlinkingToGoogle;
        public void UnlinkFromGoogle()
        {
            if(_unlinkingToGoogle) return;
            _unlinkingToGoogle = _authenticatorGoogle != null;
            OnUnlinkingAccount?.Invoke();
            _authenticatorGoogle?.UnlinkGoogleAccount((authData) =>
            {
                _unlinkingToGoogle = false;
                OnAccountUnlinked?.Invoke(authData);
            }, (exc) =>
            {
                _unlinkingToGoogle = false;
                OnAccountFailedToUnlink?.Invoke(exc);
            });
        }
        
        private bool _linkingToFacebook;
        public void LinkToFacebook()
        {
            if (_linkingToFacebook) return;
            _linkingToFacebook = _authenticatorFacebook != null;
            OnLinkingAccount?.Invoke();
            _authenticatorFacebook?.LinkToFacebook((authData) =>
            {
                _linkingToFacebook = false;
                OnAccountLinked?.Invoke(authData);
            }, (exc) =>
            {
                _linkingToFacebook = false;
                OnAccountFailedToLink?.Invoke(exc);
            });
        }
        
        private bool _unlinkingToFacebook;
        public void UnlinkFromFacebook()
        {
            if (_unlinkingToFacebook) return;
            _unlinkingToFacebook = _authenticatorFacebook != null;
            OnUnlinkingAccount?.Invoke();
            _authenticatorFacebook?.UnLinkFacebookAccount((authData) =>
            {
                _unlinkingToFacebook = false;
                OnAccountUnlinked?.Invoke(authData);
            }, (exc) =>
            {
                _unlinkingToFacebook = false;
                OnAccountFailedToUnlink?.Invoke(exc);
            });
        }
        
        #endregion
        
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
        public bool IsAnonymous;
        public string UserId;
        public string DisplayName;
        public string EmailAddress;
        public Uri PhotoUrl;
        public string Provider;
        public List<string> Providers;
    }
}