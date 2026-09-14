using UnityEngine;
using Zenject;

public class Coin : MonoBehaviour, IPickupable
{
    [Inject] private GameManager _gameManager;

    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 180f;
    [SerializeField] private float magnetSpeed = 10f;
    [SerializeField] private float magnetRange = 15f;

    private Transform _player;
    private bool _isCollected;

    private void Start()
    {
        var playerController = FindObjectOfType<PlayerController>();
        if (playerController != null)
            _player = playerController.transform;

        if (_gameManager == null)
            _gameManager = FindObjectOfType<GameManager>();

    }

    private void Update()
    {
        transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        if (_isCollected) return;

        if (PlayerController.MagnetActive && _player != null)
        {
            float distance = Vector3.Distance(transform.position, _player.position);

            if (distance < magnetRange)
            {
                Vector3 targetPosition = new Vector3(_player.position.x,transform.position.y, _player.position.z);

                Vector3 direction = (targetPosition - transform.position).normalized;
                transform.position += direction * magnetSpeed * Time.deltaTime;

                if (distance < 0.5f)
                {
                    Collect();
                }
            }
        }
    }

    public void Collect()
    {
        if (_isCollected) return;
        _isCollected = true;
        _gameManager.AddCoins(1);
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        _isCollected = false;
    }

}