using UnityEngine;

public class SwordCollision : MonoBehaviour
{
    [SerializeField] private Transform bladeTip; // Точка на кончике лезвия
    [SerializeField] private Transform bladeBase; // Точка у основания лезвия
    [SerializeField] private LayerMask targetLayers;
    [SerializeField] private int damage = 5;

    private Vector3 previousBladeTipPosition;

    void Start()
    {
        if (bladeTip == null)
        {
            // Автоматически ищем дочерний объект с меткой
            bladeTip = transform.Find("BladeTip");
            if (bladeTip == null)
            {
                Debug.LogError("Assign bladeTip transform!");
                return;
            }
        }

        previousBladeTipPosition = bladeTip.position;
    }

    void Update()
    {
        if (bladeTip == null) return;

        DetectSwingWithBlade();
        previousBladeTipPosition = bladeTip.position;
    }

    bool inCollider = false;
    private void DetectSwingWithBlade()
    {
        Vector3 currentTipPos = bladeTip.position;
        Vector3 movement = currentTipPos - previousBladeTipPosition;
        float distance = movement.magnitude;

        if (distance > 0.01f)
        {
/*            // Raycast вдоль движения кончика лезвия
            RaycastHit[] hits = Physics.RaycastAll(
                previousBladeTipPosition,
                movement.normalized,
                distance,
                targetLayers
            );

            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.gameObject != gameObject)
                {
                    Debug.Log($"Blade hit: {hit.collider.name}");
                    //ProcessHit(hit);
                }
            }*/

            // Дополнительно: Raycast между основанием и кончиком лезвия
            if (bladeBase != null)
            {
                Vector3 bladeDirection = (bladeTip.position - bladeBase.position).normalized;
                float bladeLength = Vector3.Distance(bladeBase.position, bladeTip.position);

                RaycastHit[] bladeHits = Physics.RaycastAll(
                    bladeBase.position,
                    bladeDirection,
                    bladeLength,
                    targetLayers
                );

                if (bladeHits.Length > 0)
                    foreach (RaycastHit hit in bladeHits)
                    {
                        if (hit.collider.gameObject != gameObject)
                        {
                            if (!inCollider)
                            {
                                inCollider = true;
                                var health = hit.collider.gameObject.GetComponentInParent<Health>();
                                if (health != null) health.TakeDamage(damage, movement);
                                
                            }
                            
                        }
                    }
                else inCollider = false;
            }
        }
    }
}