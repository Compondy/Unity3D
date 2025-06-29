using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneController : MonoBehaviour
{
/*    public void OpenMainScene()
    {
        SceneManager.LoadSceneAsync(0);
    }*/
    public void OpenGameScene()
    {
        SceneManager.LoadSceneAsync(0);//,LoadSceneMode.Additive);
    }
}
