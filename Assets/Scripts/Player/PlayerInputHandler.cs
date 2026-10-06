using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class PlayerInputHandler : MonoBehaviour
{
    [Inject] private PlayerController _player;
    [Inject] private GameManager _gameManager;

    private PlayerInputActions _inputActions;

    [Inject] private MetaProgress _meta;
    [Inject] private ShopUI _shop;

    private float _restartHoldTime;
    private bool _restartHeld;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
        _inputActions.Player.Enable();

        _inputActions.Player.Jump.performed += OnJump;
        _inputActions.Player.Slide.performed += OnSlide;
        _inputActions.Player.MoveLeft.performed += OnMoveLeft;
        _inputActions.Player.MoveRight.performed += OnMoveRight;
        _inputActions.Player.Jump.canceled += OnJumpCanceled;
        _inputActions.Player.Pause.performed += OnPause;
        _inputActions.Player.Restart.started += OnRestartStarted;
        _inputActions.Player.Restart.canceled += OnRestartCanceled;
        _inputActions.Player.DebugPause.performed += OnDebugPause;
        _inputActions.Player.AddCurrency.performed += OnAddCurrency;
        _inputActions.Player.OpenShop.performed += OnOpenShop;
    }

    private void OnRestartStarted(InputAction.CallbackContext ctx) => _restartHeld = true;
    private void OnRestartCanceled(InputAction.CallbackContext ctx) => _restartHeld = false;
    private void OnDebugPause(InputAction.CallbackContext ctx)
    {
        if (_gameManager.CurrentState == GameManager.State.Playing) _gameManager.Pause();
        else if (_gameManager.CurrentState == GameManager.State.Paused) _gameManager.Resume();
    }
    private void OnAddCurrency(InputAction.CallbackContext ctx)
    {
        _meta.AddToBank(1000);
        _player.PlayCoinSound();
    }

    private void OnOpenShop(InputAction.CallbackContext ctx)
    {
        if (_shop.IsOpen)
            _shop.Close();
        else
            _shop.Open();
    }

    private void Update()
    {
        if (_restartHeld)
        {
            _restartHoldTime += Time.unscaledDeltaTime;
            if (_restartHoldTime >= 1.5f)
            {
                _restartHoldTime = 0f;
                _restartHeld = false;
                _gameManager.StartGame();
            }
        }
        else
        {
            _restartHoldTime = 0f;
        }
    }

    private void OnPause(InputAction.CallbackContext ctx)
    {
        switch (_gameManager.CurrentState)
        {
            case GameManager.State.Playing:
                _gameManager.Pause();
                break;

            case GameManager.State.Paused:
                _gameManager.Resume();
                break;

            case GameManager.State.Menu:
                Application.Quit();
                break;
        }
    }

    private void OnEnable()
    {
        _inputActions?.Player.Enable();
    }

    private void OnDisable()
    {
        _inputActions?.Player.Disable();
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
            _inputActions.Player.Pause.performed -= OnPause;
            _inputActions.Player.Disable();
            _inputActions.Dispose();
            _inputActions = null;
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

    private void OnMoveLeft(InputAction.CallbackContext ctx)
    {
        _player.MoveLeft();
    }

    private void OnMoveRight(InputAction.CallbackContext ctx)
    {
        _player.MoveRight();
    }
}