using UnityEngine;
using Zenject;

public class Powerup : MonoBehaviour, IPickupable
{
    [Inject] private GameManager _gameManager;

    public enum Type { Magnet, DoubleScore, ExtraLife, Invincibility }

    [Header("Settings")]
    [SerializeField] private Type powerupType;
    [SerializeField] private float duration = 5f;

    public void Start()
    {
        if (_gameManager == null)
            _gameManager = FindObjectOfType<GameManager>();
    }

    public void Collect()
    {
        switch (powerupType)
        {
            case Type.Magnet:
                PlayerController.MagnetActive = true;
                Invoke(nameof(DeactivateMagnet), duration);
                break;
            case Type.DoubleScore:
                PlayerController.DoubleScoreActive = true;
                Invoke(nameof(DeactivateDoubleScore), duration);
                break;
            case Type.ExtraLife:
                _gameManager.AddLife(1);
                break;
            case Type.Invincibility:
                PlayerController.InvincibilityActive = true;
                Invoke(nameof(DeactivateInvincibility), duration);
                break;
        }
        gameObject.SetActive(false);
    }

    private void DeactivateMagnet() => PlayerController.MagnetActive = false;
    private void DeactivateDoubleScore() => PlayerController.DoubleScoreActive = false;
    private void DeactivateInvincibility() => PlayerController.InvincibilityActive = false;
}