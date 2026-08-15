using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Audio Settings")]
    [SerializeField] private AudioClip menuMusic;
    [SerializeField] private AudioClip gameMusic;
    [SerializeField] private float volume = 0.5f;

    private AudioSource _audioSource;
    private bool _isGameMusicPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            _audioSource = gameObject.AddComponent<AudioSource>();
        }

        _audioSource.loop = true;
        _audioSource.volume = volume;
        _audioSource.spatialBlend = 0f;
    }

    public void PlayMenuMusic()
    {
        if (menuMusic == null) return;

        if (_audioSource.clip != menuMusic || !_audioSource.isPlaying)
        {
            _audioSource.clip = menuMusic;
            _audioSource.Play();
            _isGameMusicPlaying = false;
        }
    }

    public void PlayGameMusic()
    {
        if (gameMusic == null) return;

        if (_audioSource.clip != gameMusic || !_audioSource.isPlaying)
        {
            _audioSource.clip = gameMusic;
            _audioSource.Play();
            _isGameMusicPlaying = true;
        }
    }

    public void StopMusic()
    {
        _audioSource.Stop();
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        _audioSource.volume = volume;
    }

    public bool IsGameMusicPlaying => _isGameMusicPlaying;
    public bool IsPlaying => _audioSource.isPlaying;
}