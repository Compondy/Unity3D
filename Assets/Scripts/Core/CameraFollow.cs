using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform target;

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 5, -8);
    [SerializeField] private float smoothSpeed = 5f;
    [SerializeField] private float fixedY = 5f;

    private float _fixedX = 0f;
    private Vector3 _velocity = Vector3.zero;

    private void Start()
    {
        _fixedX = 0f;

        transform.position = new Vector3(_fixedX, fixedY, target.position.z + offset.z);
    }

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 targetPosition = new Vector3(
            _fixedX,
            fixedY,
            target.position.z + offset.z
        );

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref _velocity,
            1f / smoothSpeed
        );

        transform.rotation = Quaternion.Euler(15f, 0f, 0f);
    }
}