using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMover : MonoBehaviour
{

    InputAction moveAction;
    Rigidbody2D playerBody;

    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        playerBody = GetComponent<Rigidbody2D>();
    }

   


    // Update is called once per frame
    void Update()
    {
        if (moveAction.WasPerformedThisFrame())
        {
            var moveDirection = moveAction.ReadValue<Vector2>();

            if (moveDirection.x == 1) playerBody.MovePosition(new Vector2(playerBody.position.x + 1, playerBody.position.y)); //right
            else if (moveDirection.x == -1) playerBody.MovePosition(new Vector2(playerBody.position.x - 1, playerBody.position.y)); //left
                else if (moveDirection.y == 1) playerBody.MovePosition(new Vector2(playerBody.position.x, playerBody.position.y + 1)); //up
            else if (moveDirection.y == -1) playerBody.MovePosition(new Vector2(playerBody.position.x, playerBody.position.y - 1)); //down

        }

    }
}
