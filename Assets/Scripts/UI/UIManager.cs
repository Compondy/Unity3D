using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UIManager : MonoBehaviour
{
    [Inject] private GameManager _gameManager;

    [Header("UI Panels")]
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gameUIPanel;
    [SerializeField] private GameObject gameOverPanel;

    [Header("Game UI")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text coinsText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text speedText;

    [Header("Game Over UI")]
    [SerializeField] private TMP_Text finalScoreText;
    [SerializeField] private TMP_Text highScoreText;

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
        gameOverPanel.SetActive(false);
    }

    public void ShowGameUI(bool show)
    {
        menuPanel.SetActive(false);
        gameUIPanel.SetActive(show);
        gameOverPanel.SetActive(false);
    }

    public void ShowGameOver(bool show, int score)
    {
        menuPanel.SetActive(false);
        gameUIPanel.SetActive(false);
        gameOverPanel.SetActive(show);

        if (show)
        {
            finalScoreText.text = $"Score: {score}";
            highScoreText.text = $"Best: {PlayerPrefs.GetInt("HighScore", 0)}";
        }
    }

    public void UpdateLives(int lives)
    {
        livesText.text = $"Lives: {lives}";
    }

    // Кнопки
    public void OnStartPressed() => _gameManager.StartGame();
    public void OnRestartPressed() => _gameManager.StartGame();
    public void OnMenuPressed()
    {
        _gameManager.SetState(GameManager.State.Menu);
        ShowMenu(true);
    }
}