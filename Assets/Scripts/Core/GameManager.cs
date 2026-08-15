using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    public enum State { Menu, Playing, GameOver }

    [Inject] private PlayerController _player;
    [Inject] private TrackGenerator _track;
    [Inject] private UIManager _ui;

    public State CurrentState { get; private set; } = State.Menu;

    public int Score { get; private set; }
    public int Coins { get; private set; }
    public int Lives { get; private set; } = 3;
    public float Speed { get; private set; } = 5f;
    public float RunTime { get; private set; }
    public int ComboMultiplier { get; private set; }


    public float MinSpeed = 5f;
    public float MaxSpeed = 12f;
    public float Acceleration = 0.2f;
    public int MaxLives = 3;

    private void Start()
    {
        _ui.ShowMenu(true);
    }

    public void StartGame()
    {
        CurrentState = State.Playing;
        Score = 0;
        Coins = 0;
        Lives = MaxLives;
        Speed = MinSpeed;
        RunTime = 0;
        ComboMultiplier = 1;

        _player.ResetPosition();
        _player.StartRunning();
        _track.StartGeneration();
        _ui.ShowGameUI(true);
        MusicManager.Instance?.PlayGameMusic();
    }

    public void GameOver()
    {
        CurrentState = State.GameOver;
        _track.StopGeneration();
        _ui.ShowGameOver(true, Score);
        SaveScore();
    }

    public void AddScore(int amount) => Score += amount;
    public void AddCoins(int amount)
    {
        Coins += amount;
        ComboMultiplier += 1;
    }
    public void AddLife(int amount = 1)
    {
        Lives = Mathf.Min(Lives + amount, MaxLives);
    }
    public void SetState(State newState)
    {
        CurrentState = newState;

        if (newState == State.Menu)
        {
            _player.FullReset();
        }

    }
    public void TakeDamage()
    {
        if (CurrentState != State.Playing) return;

        Lives--;

        ComboMultiplier = 1;

        _ui.UpdateLives(Lives);

        if (Lives <= 0)
        {
            GameOver();
        }
    }

    private void UpdateScore()
    {
        float timeScore = RunTime * 100f;
        float speedBonus = Speed * 0.5f;
        float livesBonus = Lives * 200f;
        float coinBonus = Coins * 50f;
        float comboBonus = ComboMultiplier * 50f;
        Score = (int)(timeScore + speedBonus + livesBonus + coinBonus + comboBonus);
    }

    private void Update()
    {
        if (CurrentState == State.Playing)
        {
            RunTime += Time.deltaTime;

            UpdateScore();

            if (Speed < MaxSpeed)
                Speed += Acceleration * Time.deltaTime;
        }
    }

    private void SaveScore()
    {
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (Score > highScore)
            PlayerPrefs.SetInt("HighScore", Score);
    }
}