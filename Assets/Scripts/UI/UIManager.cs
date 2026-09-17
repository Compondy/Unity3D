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
        if (_gameManager.CurrentState == GameManager.State.Playing)
        {
            scoreText.text = $"Score: {_gameManager.Score}";
            coinsText.text = $"Coins: {_gameManager.Coins}";
            livesText.text = $"Lives: {_gameManager.Lives}";
            speedText.text = $"Speed: {_gameManager.Speed:F1}";
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