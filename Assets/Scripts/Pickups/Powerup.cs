using UnityEngine;
using Zenject;

public class Powerup : MonoBehaviour, IPickupable
{
    [Inject] private GameManager _gameManager;
    [Inject] private PlayerController _playerController;
    [Inject] private MetaProgress _meta;

    public enum Type { Magnet, DoubleScore, ExtraLife, Invincibility }

    [Header("Settings")]
    [SerializeField] private Type powerupType;
    [SerializeField] private float duration = 5f;

    private bool _isCollected;

    public void Collect()
    {
        if (_isCollected || !gameObject.activeInHierarchy) return;
        _isCollected = true;

        float actualDuration = duration * (_meta != null ? _meta.BonusDurationMultiplier : 1f);

        switch (powerupType)
        {
            case Type.Magnet: _playerController.ActivateMagnet(actualDuration); break;
            case Type.DoubleScore: _playerController.ActivateDoubleScore(actualDuration); break;
            case Type.ExtraLife: _gameManager.AddLife(1); break;
            case Type.Invincibility: _playerController.ActivateInvincibility(actualDuration); break;
        }
        gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        _isCollected = false;
    }
}