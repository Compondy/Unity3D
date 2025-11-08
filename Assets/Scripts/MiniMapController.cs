using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MiniMapController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera miniMapCamera;
    public Transform player;

    [Header("Icons")]
    public RectTransform playerIcon;
    public RectTransform enemyIcon; // Базовая иконка врага

    [Header("Enemies")]
    public Transform[] enemies;

    private List<RectTransform> activeEnemyIcons = new List<RectTransform>();

    private void Start()
    {
        // Сразу скрываем оригинальную иконку
        enemyIcon.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (player == null) return;

        UpdateCameraPosition();
        UpdatePlayerIcon();
        UpdateEnemyIcons();
    }

    private void UpdateCameraPosition()
    {
        if (miniMapCamera != null)
        {
            Vector3 newPos = player.position;
            newPos.y = miniMapCamera.transform.position.y;
            miniMapCamera.transform.position = newPos;
        }
    }

    private void UpdatePlayerIcon()
    {
        if (playerIcon != null)
        {
            playerIcon.anchoredPosition = Vector2.zero;
            playerIcon.localEulerAngles = new Vector3(0, 0, -player.eulerAngles.y);
        }
    }

    private void UpdateEnemyIcons()
    {
        if (enemyIcon == null || enemies == null) return;

        // Создаем нужное количество иконок
        while (activeEnemyIcons.Count < enemies.Length)
        {
            RectTransform newIcon = Instantiate(enemyIcon, transform);
            newIcon.gameObject.SetActive(true);
            activeEnemyIcons.Add(newIcon);
        }

        // Обновляем позиции для всех активных врагов
        int activeEnemyCount = 0;
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null && enemies[i].gameObject.activeInHierarchy)
            {
                activeEnemyIcons[activeEnemyCount].gameObject.SetActive(true);

                Vector3 relativePos = enemies[i].position - player.position;
                RectTransform mapRect = GetComponent<RectTransform>();
                float scaleFactor = mapRect.rect.width / (miniMapCamera.orthographicSize * 2f);

                Vector2 mapPos = new Vector2(
                    relativePos.x * scaleFactor,
                    relativePos.z * scaleFactor
                );

                // Ограничиваем позицию в пределах миникарты
                float maxDistance = mapRect.rect.width * 0.45f; // С запасом от краев
                mapPos = Vector2.ClampMagnitude(mapPos, maxDistance);

                activeEnemyIcons[activeEnemyCount].anchoredPosition = mapPos;
                activeEnemyCount++;
            }
        }

        // Скрываем неиспользуемые иконки
        for (int i = activeEnemyCount; i < activeEnemyIcons.Count; i++)
        {
            activeEnemyIcons[i].gameObject.SetActive(false);
        }
    }
}