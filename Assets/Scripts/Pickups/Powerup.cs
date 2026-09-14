using UnityEngine;
using Zenject;
using static UnityEditor.Experimental.GraphView.GraphView;

public class Powerup : MonoBehaviour, IPickupable
{
    [Inject] private GameManager _gameManager;
    [Inject] private PlayerController _player;

    public enum Type { Magnet, DoubleScore, ExtraLife, Invincibility }

    [Header("Settings")]
    [SerializeField] private Type powerupType;
    [SerializeField] private float duration = 5f;

    public void Awake()
    {
        if (_gameManager == null)
            _gameManager = FindObjectOfType<GameManager>();
        if (_player == null)
            _player = FindObjectOfType<PlayerController>();
    }

    public void Collect()
    {
        switch (powerupType)
        {
            case Type.Magnet: _player.ActivateMagnet(duration); break;
            case Type.DoubleScore: _player.ActivateDoubleScore(duration); break;
            case Type.ExtraLife: _gameManager.AddLife(1); break;
            case Type.Invincibility: _player.ActivateInvincibility(duration); break;
        }
        gameObject.SetActive(false);
    }
}