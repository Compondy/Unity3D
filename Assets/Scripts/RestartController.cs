using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Zenject;

public class RestartController : MonoBehaviour
{


    Image fill;
    InputAction restartAction;
    CanvasGroup canvasGroup; //used to toggle UI

    [Inject]
    private SceneController sceneController;

    private bool UIenabled = false;
    private float restartFill = 0f;

    private void EnableUI()
    {
        canvasGroup.alpha = 1.0f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
        UIenabled = true;
    }
    private void DisableUI()
    {
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        UIenabled = false;
    }

    DateTime awaken;

    void Awake()
    {
        fill = gameObject.GetComponentsInChildren<Image>().Where(x=>x.type == Image.Type.Filled).FirstOrDefault();
        canvasGroup = gameObject.GetComponentInParent<CanvasGroup>();
        DisableUI();
        restartAction = InputSystem.actions.FindAction("Restart");
        awaken = DateTime.Now;
    }


    void Update()
    {
        if (restartAction.IsPressed() && (DateTime.Now - awaken).Seconds > 2)
        {
            if (!UIenabled) EnableUI();
            restartFill += Time.deltaTime / 3; //3 seconds
            if (restartFill > 1.0f)
                sceneController.OpenGameScene();

            fill.fillAmount = restartFill;
        }
        else if (restartAction.WasCompletedThisFrame())
        {
            if (restartFill > 1.0f)
                sceneController.OpenGameScene();
            
            else
            {
                restartFill = 0.0f;
                fill.fillAmount = restartFill;
                DisableUI();
            }
        }
    }
}
