using UnityEngine;
using SampleProject;

public class SimpleSelection : MonoBehaviour
{
    private void Update()
    {
        // Левая кнопка мыши
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                // Проверяем, попали ли в юнита
                var unit = hit.collider.GetComponent<Faction1Entity>();
                if (unit != null)
                {
                    Debug.Log($"Hit! Selected unit: {unit.name}");

                    // Меняем цвет для визуального подтверждения
                    var renderer = unit.GetComponentInChildren<Renderer>();
                    if (renderer != null)
                    {
                        renderer.material.color = Color.green;
                    }
                }
                else
                {
                    Debug.Log($"Hit: {hit.collider.name} (not a unit)");
                }
            }
            else
            {
                Debug.Log("Raycast hit nothing");
            }
        }
    }
}