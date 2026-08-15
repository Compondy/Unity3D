using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class PlayerInputHandler : MonoBehaviour
{
    [Inject] private PlayerController _player;

    private PlayerInputActions _inputActions;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
        _inputActions.Player.Enable();

        _inputActions.Player.Jump.performed += OnJump;
        _inputActions.Player.Slide.performed += OnSlide;
        _inputActions.Player.MoveLeft.performed += OnMoveLeft;
        _inputActions.Player.MoveRight.performed += OnMoveRight;

        _inputActions.Player.Jump.canceled += OnJumpCanceled;
        _inputActions.Player.Slide.canceled += OnSlideCanceled;
    }

    private void OnDestroy()
    {
        if (_inputActions != null)
        {
            _inputActions.Player.Jump.performed -= OnJump;
            _inputActions.Player.Slide.performed -= OnSlide;
            _inputActions.Player.MoveLeft.performed -= OnMoveLeft;
            _inputActions.Player.MoveRight.performed -= OnMoveRight;
            _inputActions.Player.Jump.canceled -= OnJumpCanceled;
            _inputActions.Player.Slide.canceled -= OnSlideCanceled;
            _inputActions.Dispose();
        }
    }

    private void OnJump(InputAction.CallbackContext ctx)
    {
        _player.Jump();
    }

    private void OnJumpCanceled(InputAction.CallbackContext ctx)
    {
    }

    private void OnSlide(InputAction.CallbackContext ctx)
    {
        _player.Slide();
    }

    private void OnSlideCanceled(InputAction.CallbackContext ctx)
    {
    }

    private void OnMoveLeft(InputAction.CallbackContext ctx)
    {
        _player.MoveLeft();
    }

    private void OnMoveRight(InputAction.CallbackContext ctx)
    {
        _player.MoveRight();
    }
}