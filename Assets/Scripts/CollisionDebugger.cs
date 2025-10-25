using UnityEngine;

public class CollisionDebugger : MonoBehaviour
{
    void Start()
    {
        Debug.Log($"{gameObject.name} setup:");
        Debug.Log($"- Layer: {LayerMask.LayerToName(gameObject.layer)}");

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Debug.Log($"- Collider: {col.GetType().Name}, IsTrigger: {col.isTrigger}, Enabled: {col.enabled}");
        }

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            Debug.Log($"- Rigidbody: IsKinematic: {rb.isKinematic}, Detection: {rb.collisionDetectionMode}");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"🚨 TRIGGER ENTER: {gameObject.name} with {other.name}");
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"🚨 COLLISION ENTER: {gameObject.name} with {collision.gameObject.name}");
    }
}