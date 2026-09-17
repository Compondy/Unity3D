using UnityEngine;
using Zenject;

public class Powerup : MonoBehaviour, IPickupable
{
    [Inject] private GameManager _gameManager;
    [Inject] private PlayerController _playerController;

    public enum Type { Magnet, DoubleScore, ExtraLife, Invincibility }

    [Header("Settings")]
    [SerializeField] private Type powerupType;
    [SerializeField] private float duration = 5f;

    public void Awake()
    {
        if (_gameManager == null)
            _gameManager = FindObjectOfType<GameManager>();
        if (_playerController == null)
            _playerController = FindObjectOfType<PlayerController>();
    }

    public void Collect()
    {
        switch (powerupType)
        {
            case Type.Magnet: _playerController.ActivateMagnet(duration); break;
            case Type.DoubleScore: _playerController.ActivateDoubleScore(duration); break;
            case Type.ExtraLife: _gameManager.AddLife(1); break;
            case Type.Invincibility: _playerController.ActivateInvincibility(duration); break;
        }
        gameObject.SetActive(false);
    }
}