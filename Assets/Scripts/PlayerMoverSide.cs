using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoverIsometric : MonoBehaviour
{

    InputAction moveAction;
    Rigidbody2D playerBody;
    public float Speed { get; set; }

    private void Awake()
    {
        if (Speed == 0f) Speed = 20;
        moveAction = InputSystem.actions.FindAction("Move");
        playerBody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (moveAction.IsPressed())
        {
            var moveDirection = moveAction.ReadValue<Vector2>();

            if (moveDirection.x == 1)
                playerBody.MovePosition(new Vector2(playerBody.position.x + Speed * Time.deltaTime, playerBody.position.y)); //right
            else if (moveDirection.x == -1)
                playerBody.MovePosition(new Vector2(playerBody.position.x - Speed * Time.deltaTime, playerBody.position.y)); //left
        }
    }
}
