using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScenesEngine : MonoBehaviour
{

    [DllImport("__Internal")]
    private static extern void SignInWithGoogle();

    [DllImport("__Internal")]
    private static extern void SuscribeToAuthentication();

    [DllImport("__Internal")]
    private static extern bool IsSuscribedToAuthentication();

    [DllImport("__Internal")]
    private static extern void LogOut();

    public void SignInWithGoogleFromJS() => SignInWithGoogle();

    public void LogOutFromJS() => LogOut();

    void Start()
    {
        if (IsSuscribedToAuthentication()) return;
        ScenesEngine.SuscribeToAuthentication();
    }

    public void OnUserAuthenticated(string userJson)
    {
        if (string.IsNullOrEmpty(userJson)) SceneManager.LoadScene("SignIn");
        else SceneManager.LoadScene("MainMenu");
    }

    public void GoToWarScene() => SceneManager.LoadScene("WarScene");

    public void GoToMainMenuScene() => SceneManager.LoadScene("MainMenu");

}
