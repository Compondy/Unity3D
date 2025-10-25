using UnityEngine;

public class LayerDebug : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers;

    void Start()
    {
        Debug.Log($"Target layers value: {targetLayers.value}");
        Debug.Log($"My layer: {gameObject.layer}");

        // Проверяем, включен ли наш слой в маску
        bool isInLayerMask = (targetLayers.value & (1 << gameObject.layer)) != 0;
        Debug.Log($"My layer is in target mask: {isInLayerMask}");

        // Проверяем все слои в маске
        for (int i = 0; i < 32; i++)
        {
            if ((targetLayers.value & (1 << i)) != 0)
            {
                Debug.Log($"Layer in mask: {LayerMask.LayerToName(i)} (index: {i})");
            }
        }
    }
}