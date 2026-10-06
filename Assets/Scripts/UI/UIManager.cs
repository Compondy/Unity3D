using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIManager : MonoBehaviour
{
    [Inject] private GameManager _gameManager;
    [Inject] private ShopUI shopUI;
    [Inject] private ISaveService _save;

    [Header("UI Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gameUIPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject pausePanel;

    [Header("Game UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text speedText;

    [Header("Game Over UI")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text highScoreText;

    private int _lastScore = -1;
    private int _lastCoins = -1;
    private int _lastLives = -1;
    private float _lastSpeed = -1f;

    public void OnShopPressed() => shopUI.Open();
    public void OnStartPressed() => _gameManager.StartGame();
    public void OnRestartPressed() {
        _gameManager.StartGame();
    }
    public void OnMenuPressed()
    {
        Time.timeScale = 1f;
        _gameManager.SetState(GameManager.State.Menu);
        _gameManager.StopGame();
        ShowMenu(true);
    }
    public void OnPausePressed() => _gameManager.Pause();
    public void OnResumePressed() => _gameManager.Resume();
    public void OnExitPressed() => Application.Quit();

    public void ShowPause(bool show)
    {
        pausePanel.SetActive(show);
    }

    private void Update()
    {
        if (_gameManager.CurrentState != GameManager.State.Playing) return;

        if (_gameManager.Score != _lastScore)
        {
            _lastScore = _gameManager.Score;
            scoreText.text = $"Score: {_lastScore}";
        }

        if (_gameManager.Coins != _lastCoins)
        {
            _lastCoins = _gameManager.Coins;
            coinsText.text = $"Coins: {_lastCoins}";
        }

        if (_gameManager.Lives != _lastLives)
        {
            _lastLives = _gameManager.Lives;
            livesText.text = $"Lives: {_lastLives}";
        }
        if (Mathf.Abs(_gameManager.Speed - _lastSpeed) > 0.1f)
        {
            _lastSpeed = _gameManager.Speed;
            speedText.text = $"Speed: {_lastSpeed:F1}";
        }
    }

    public void ShowMenu(bool show)
    {
        menuPanel.SetActive(show);
        gameUIPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameUI(bool show)
    {
        menuPanel.SetActive(false);
        gameUIPanel.SetActive(show);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver(bool show, int score)
    {
        menuPanel.SetActive(false);
        gameUIPanel.SetActive(false);
        pausePanel.SetActive(false);
        gameOverPanel.SetActive(show);

        if (show)
        {
            finalScoreText.text = $"Score: {score}";
            highScoreText.text = $"Best: {_save.GetInt("HighScore", 0)}";
        }
    }

    public void UpdateLives(int lives)
    {
        livesText.text = $"Lives: {lives}";
    }

}