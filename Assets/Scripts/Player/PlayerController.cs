using UnityEngine;
using Zenject;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Inject] private GameManager _gameManager;

    [Header("Movement")]
    [SerializeField] private float jumpHeight = 2.4f;
    [SerializeField] private float jumpDuration = 1.0f;
    [SerializeField] private float slideDuration = 0.6f;
    [SerializeField] private float laneSwitchSpeed = 8f;
    [SerializeField] private float laneOffset = 1.5f;

    [Header("References")]
    [SerializeField] private Collider playerCollider;
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Animator animator;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip coinSound;
    [SerializeField] private AudioClip powerupSound;

    private AudioSource _audioSource;

    private int _currentLane = 1;
    private Vector3 _targetPosition;
    private float _jumpProgress;
    private float _slideTimer;
    private bool _isJumping;
    private bool _isSliding;
    private bool _isInvincible;
    private bool _isDead;
    private Material _material;
    private Color _originalColor;

    public static bool MagnetActive { get; set; }
    public static bool DoubleScoreActive { get; set; }
    public static bool InvincibilityActive { get; set; }

    public int CurrentLane => _currentLane;
    public bool IsJumping => _isJumping;
    public bool IsSliding => _isSliding;
    public bool IsInvincible => _isInvincible;

    private float BaseY => 0.527f;

    // === Инициализация ===
    private void Start()
    {
        _targetPosition = new Vector3(0, BaseY, 0);
        transform.position = _targetPosition;

        if (animator == null)
            animator = GetComponent<Animator>();

        if (meshRenderer == null)
            meshRenderer = GetComponent<MeshRenderer>();

        if (meshRenderer != null)
        {
            _material = meshRenderer.material;
            _originalColor = _material.color;
        }

        if (playerCollider == null)
            playerCollider = GetComponent<Collider>();

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        _audioSource.spatialBlend = 0f;
        _audioSource.volume = 0.8f;


        if (_gameManager.CurrentState == GameManager.State.Playing)
        {
            animator?.Play("Run");
        }
        else
            animator?.Play("Idle");

    }

    private void PlaySound(AudioClip clip, float volume = 0.8f)
    {
        if (clip == null || _audioSource == null) return;
        _audioSource.PlayOneShot(clip, volume);
    }

    private void Update()
    {
        if (_gameManager.CurrentState != GameManager.State.Playing) return;
        if (_isDead) return;

        transform.position = Vector3.Lerp(
            transform.position,
            new Vector3(_targetPosition.x, transform.position.y, transform.position.z),
            laneSwitchSpeed * Time.deltaTime
        );

        UpdateJump();
        //UpdateSlide();
    }

    private void UpdateJump()
    {
        if (!_isJumping) return;

        _jumpProgress += Time.deltaTime / jumpDuration;
        float heightOffset = Mathf.Sin(_jumpProgress * Mathf.PI) * jumpHeight;

        transform.position = new Vector3(
            transform.position.x,
            BaseY + heightOffset,
            transform.position.z
        );

        if (_jumpProgress >= 1f)
        {
            _isJumping = false;
            transform.position = new Vector3(transform.position.x, BaseY, transform.position.z);
            animator?.SetBool("IsJumping", false);
        }
    }

    public void Jump()
    {
        if (_isJumping || _isSliding || _gameManager.CurrentState != GameManager.State.Playing || _isDead) return;
        PlaySound(jumpSound);
        _isJumping = true;
        _jumpProgress = 0;
        animator?.SetBool("IsJumping", true);
        animator?.SetTrigger("Jump");
    }

    public void StopJump()
    {
        if (_isJumping)
        {
            _isJumping = false;
            transform.position = new Vector3(transform.position.x, BaseY, transform.position.z);
            animator?.SetBool("IsJumping", false);
        }
    }

    private void UpdateSlide()
    {
        if (!_isSliding) return;

        _slideTimer += Time.deltaTime;
        if (_slideTimer >= slideDuration)
        {
            _isSliding = false;
            if (playerCollider != null)
                playerCollider.transform.localScale = Vector3.one;
            animator?.SetBool("IsSliding", false);
        }
    }

    public void Slide()
    {
        if (_isJumping || _isSliding || _gameManager.CurrentState != GameManager.State.Playing || _isDead) return;

        _isSliding = true;
        _slideTimer = 0;
        if (playerCollider != null)
            playerCollider.transform.localScale = new Vector3(1, 0.5f, 1);
        animator?.SetBool("IsSliding", true);
        animator?.SetTrigger("Slide");
    }

    public void StopSlide()
    {
        if (_isSliding)
        {
            _isSliding = false;
            if (playerCollider != null)
                playerCollider.transform.localScale = Vector3.one;
            animator?.SetBool("IsSliding", false);
        }
    }

    public void MoveLeft() => ChangeLane(-1);
    public void MoveRight() => ChangeLane(1);

    private void ChangeLane(int direction)
    {
        if (_gameManager.CurrentState != GameManager.State.Playing || _isDead) return;
        int newLane = Mathf.Clamp(_currentLane + direction, 0, 2);
        if (newLane == _currentLane) return;
        _currentLane = newLane;
        _targetPosition.x = (_currentLane - 1) * laneOffset;
    }

    public void ResetPosition()
    {
        _currentLane = 1;
        _targetPosition = new Vector3(0, BaseY, 0);
        transform.position = _targetPosition;
        _isJumping = false;
        _isSliding = false;
        _isInvincible = false;
        _isDead = false;

        if (playerCollider != null)
            playerCollider.transform.localScale = Vector3.one;

        if (_material != null)
            _material.color = _originalColor;

        enabled = true;
        transform.rotation = Quaternion.identity;
        animator?.ResetTrigger("Stumble");
        animator?.ResetTrigger("Fall");
        animator?.ResetTrigger("Jump");
        animator?.SetBool("IsRunning", false);
        animator?.SetBool("IsJumping", false);
        animator?.SetBool("IsSliding", false);
        animator?.Play("Idle", 0, 0f);
    }

    public void StartRunning()
    {
        _isDead = false;
        enabled = true;

        animator?.ResetTrigger("Stumble");
        animator?.ResetTrigger("Fall");
        animator?.ResetTrigger("Jump");
        animator?.SetBool("IsJumping", false);
        animator?.SetBool("IsSliding", false);

        animator?.SetBool("IsRunning", true);
        animator?.Play("Run", 0, 0f);
    }

    public void OnHit()
    {
        if (_isInvincible || InvincibilityActive || _isDead) return;
        PlaySound(hitSound);
        _gameManager.TakeDamage();

        if (_gameManager.Lives <= 0)
        {
            Die();
            return;
        }

        animator?.SetTrigger("Stumble");
        StartCoroutine(InvincibilityRoutine(0.5f));
    }

    private void Die()
    {
        _isDead = true;
        MusicManager.Instance?.StopMusic();
        PlaySound(deathSound);
        _gameManager.GameOver();
        animator?.SetTrigger("Fall");

        enabled = false;

        if (playerCollider != null)
            playerCollider.transform.localScale = Vector3.one;

        StartCoroutine(SlowMotionEffect());
    }

    public void FullReset()
    {
        _currentLane = 1;
        _targetPosition = new Vector3(0, BaseY, 0);
        transform.position = _targetPosition;

        _isJumping = false;
        _isSliding = false;
        _isInvincible = false;
        _isDead = false;
        _jumpProgress = 0;
        _slideTimer = 0;

        if (playerCollider != null)
            playerCollider.transform.localScale = Vector3.one;

        if (_material != null)
            _material.color = _originalColor;

        enabled = true;
        
        transform.rotation = Quaternion.identity;

        animator?.ResetTrigger("Stumble");
        animator?.ResetTrigger("Fall");
        animator?.ResetTrigger("Jump");
        animator?.SetBool("IsRunning", false);
        animator?.SetBool("IsJumping", false);
        animator?.SetBool("IsSliding", false);
        animator?.Play("Idle", 0, 0f);

        StopAllCoroutines();
    }

    private IEnumerator SlowMotionEffect()
    {
        Time.timeScale = 0.3f;
        yield return new WaitForSecondsRealtime(0.8f);
        Time.timeScale = 1f;
    }

    private IEnumerator InvincibilityRoutine(float duration)
    {
        _isInvincible = true;
        float timer = 0;

        while (timer < duration)
        {
            if (_material != null)
                _material.color = timer % 0.3f < 0.15f ? Color.red : _originalColor;
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        if (_material != null)
            _material.color = _originalColor;
        _isInvincible = false;
    }

    public void ActivateMagnet(float duration)
    {
        MagnetActive = true;
        StartCoroutine(DeactivateMagnet(duration));
    }

    private IEnumerator DeactivateMagnet(float duration)
    {
        yield return new WaitForSeconds(duration);
        MagnetActive = false;
    }

    public void ActivateDoubleScore(float duration)
    {
        DoubleScoreActive = true;
        StartCoroutine(DeactivateDoubleScore(duration));
    }

    private IEnumerator DeactivateDoubleScore(float duration)
    {
        yield return new WaitForSeconds(duration);
        DoubleScoreActive = false;
    }

    public void ActivateInvincibility(float duration)
    {
        InvincibilityActive = true;
        StartCoroutine(DeactivateInvincibility(duration));
    }

    private IEnumerator DeactivateInvincibility(float duration)
    {
        yield return new WaitForSeconds(duration);
        InvincibilityActive = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (_isDead) return;

        if (other.TryGetComponent<Coin>(out var coin))
        {
            PlaySound(coinSound);
            _gameManager.AddCoins(1);
            other.gameObject.SetActive(false);
            return;
        }

        if (other.TryGetComponent<Obstacle>(out var obstacle))
        {
            if (_isInvincible || InvincibilityActive) return;
            OnHit();
            other.gameObject.SetActive(false);
            return;
        }

        if (other.TryGetComponent<Powerup>(out var powerup))
        {
            PlaySound(powerupSound);
            powerup.Activate();
            other.gameObject.SetActive(false);
            return;
        }
    }
}