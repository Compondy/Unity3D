using System.Xml.Linq;
using UnityEngine;

public class DestroyDetector : MonoBehaviour
{
    void Awake()
    {
        Debug.Log($"[{Time.time}] Объект '{name}' создан", this);
    }

    void OnDestroy()
    {
        string sceneName = gameObject != null ? gameObject.scene.name : "unknown";
        string activeState = gameObject != null ? gameObject.activeInHierarchy.ToString() : "destroyed";

        Debug.LogWarning($"[{Time.time}] Объект '{name}' уничтожен!\n" +
                        $"Сцена: {sceneName}\n" +
                        $"Активен: {activeState}\n" +
                        $"Позиция: {transform.position}", this);

        // Более детальный стек
        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
        for (int i = 0; i < stackTrace.FrameCount; i++)
        {
            var frame = stackTrace.GetFrame(i);
            var method = frame.GetMethod();
            Debug.Log($"Frame {i}: {method.DeclaringType?.Name}.{method.Name}");
        }
    }

    void OnApplicationQuit()
    {
        Debug.Log($"Приложение закрывается, объект '{name}' будет уничтожен");
    }
}