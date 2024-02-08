using System;

namespace _Project.Scripts.Authentication
{
    public interface IAuthenticationData
    {
        public event Action OnSignedIn;
        public event Action OnSignedOut;
        public bool SignedIn { get; }
        public string UserId { get; }
        public string DisplayName { get; }
        public string EmailAddress { get; }
        public Uri PhotoUrl { get; }

        public void CheckForDataAvailability();
    }
    
    public interface ISignUp
    {
        public void SignUp(string email, string password, Action onComplete = null, Action<string> onFailure = null);
    }

    public interface ISignIn
    {
        public void SignIn(string email, string password, Action onComplete = null, Action<string> onFailure = null);
    }

    public interface ISignOut
    {
        public void SignOut();
    }

    public interface ISignInGuest
    {
        public void SignInGuest(Action onComplete = null, Action<string> onFailure = null);
    }

    public interface IUpdateDisplayName
    {
        public void UpdateDisplayName(string displayName, Action onComplete = null, Action<string> onFailure = null);
    }

    public interface ISignInGoogle
    {
        public void GoogleSignIn(Action onComplete = null, Action<string> onFailure = null);
    }
    
    public interface ISignInFacebook
    {
        public void FacebookSignIn(Action onComplete = null, Action<string> onFailure = null);
    }

    public interface IGoogleLinker
    {
        public void LinkToGoogle(Action onComplete = null, Action<string> onFailure = null);
    }
}