using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    public enum State { Menu, Playing, Paused, GameOver }

    [Inject] private PlayerController _player;
    [Inject] private TrackGenerator _track;
    [Inject] private UIManager _ui;
    [Inject] private MusicManager _musicManager;
    [Inject] private MetaProgress _meta;
    [Inject] private ISaveService _save;

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

    private float _scoreTickTimer;
    private const float ScoreTickInterval = 1f;
    public void Pause()
    {
        if (CurrentState != State.Playing) return;
        Time.timeScale = 0f;
        CurrentState = State.Paused;
        _ui.ShowPause(true);
    }
    public void Resume()
    {
        if (CurrentState != State.Paused) return;
        Time.timeScale = 1f;
        CurrentState = State.Playing;
        _ui.ShowPause(false);
    }

    private void Start()
    {
        _ui.ShowMenu(true);
    }

    public void StartGame()
    {
        _track.StopGeneration();
        Time.timeScale = 1f;

        CurrentState = State.Playing;

        Score = 0;
        Coins = 0;
        Lives = MaxLives;
        Speed = MinSpeed;
        RunTime = 0;
        ComboMultiplier = 1;

        _player.ResetPosition();
        _player.StartRunning();

        if (_meta.DoubleScorePurchased)
        {
            _player.DoubleScoreActive = true;
            _player.LockDoubleScoreForever();
        }

        _track.StartGeneration();
        _ui.ShowGameUI(true);
        _ui.ShowPause(false);
        _musicManager.PlayGameMusic();
    }

    public void GameOver()
    {
        CurrentState = State.GameOver;
        _track.StopGeneration();
        _meta.AddToBank(Score);
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
    }


    private void UpdateScore(float deltaTime)
    {
        _scoreTickTimer += deltaTime;
        if (_scoreTickTimer < ScoreTickInterval) return;
        _scoreTickTimer -= ScoreTickInterval;

        float timeScore = RunTime * 10f;
        float speedBonus = Speed;
        float livesBonus = Lives * 10f;
        float coinBonus = Coins * 5f;
        float comboBonus = ComboMultiplier * 2f;

        float runScore = timeScore + speedBonus;
        if (_player.DoubleScoreActive) runScore *= 2f;

        int gained = Mathf.RoundToInt(runScore + livesBonus + coinBonus + comboBonus);
        Score += gained;
    }

    private void Update()
    {
        if (CurrentState == State.Playing)
        {
            RunTime += Time.deltaTime;

            UpdateScore(Time.deltaTime);

            if (Speed < MaxSpeed)
                Speed += Acceleration * Time.deltaTime;
        }
    }

    private void SaveScore()
    {
        int highScore = _save.GetInt("HighScore", 0);
        if (Score > highScore)
            _save.SetInt("HighScore", Score);
    }

    public void StopGame()
    {
        Time.timeScale = 1f;
        _track.StopGeneration();
        _player.FullReset();
        _musicManager.PlayMenuMusic();
    }
}