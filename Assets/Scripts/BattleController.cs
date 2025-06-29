using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Zenject;


public class BattleController : MonoBehaviour
{
    [Inject]
    private BattleField battlefield;

    private InputAction esc;
    private InputAction space;

    private void Awake()
    {
        esc = InputSystem.actions.FindAction("ESC");
        space = InputSystem.actions.FindAction("Approve");
        battlefield.Init();
    }


    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

        if (esc.WasCompletedThisFrame())
        {
            battlefield.ClearSelection();
        }
        if (space.WasCompletedThisFrame())
        {
            battlefield.ApproveSelection();
        }

    }

    

}
