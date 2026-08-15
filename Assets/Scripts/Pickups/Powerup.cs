using UnityEngine;

public class Powerup : MonoBehaviour
{
    public enum Type { Magnet, DoubleScore, ExtraLife, Invincibility }

    [Header("Settings")]
    [SerializeField] private Type powerupType;
    [SerializeField] private float duration = 5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<PlayerController>(out _))
        {
            Activate();
            gameObject.SetActive(false);
        }
    }

    public void Activate()
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
                GameManager gameManager = FindObjectOfType<GameManager>();
                gameManager?.AddLife(1);
                break;
            case Type.Invincibility:
                PlayerController.InvincibilityActive = true;
                Invoke(nameof(DeactivateInvincibility), duration);
                break;
        }
    }

    private void DeactivateMagnet() => PlayerController.MagnetActive = false;
    private void DeactivateDoubleScore() => PlayerController.DoubleScoreActive = false;
    private void DeactivateInvincibility() => PlayerController.InvincibilityActive = false;
}