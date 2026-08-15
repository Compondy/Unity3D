using System.Collections.Generic;
using UnityEngine;

public class TrackSegment : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private Transform pathParent;

    [Header("Settings")]
    [SerializeField] private float worldLength = 27f;

    public List<GameObject> coinsInSegment = new List<GameObject>();
    public List<GameObject> obstaclesInSegment = new List<GameObject>();
    public List<GameObject> powerupsInSegment = new List<GameObject>();

    public Transform PathParent => pathParent;
    public float WorldLength => worldLength;

    private void OnValidate()
    {
        if (pathParent != null && pathParent.childCount > 1)
            UpdateWorldLength();
    }

    public void GetPointAt(float t, out Vector3 position, out Quaternion rotation)
    {
        if (pathParent == null || pathParent.childCount == 0)
        {
            position = transform.position;
            rotation = transform.rotation;
            return;
        }

        if (pathParent.childCount == 1)
        {
            position = pathParent.GetChild(0).position;
            rotation = pathParent.GetChild(0).rotation;
            return;
        }

        float clampedT = Mathf.Clamp01(t);
        float scaledT = (pathParent.childCount - 1) * clampedT;
        int index = Mathf.FloorToInt(scaledT);
        float segmentT = scaledT - index;

        if (index >= pathParent.childCount - 1)
        {
            position = pathParent.GetChild(pathParent.childCount - 1).position;
            rotation = pathParent.GetChild(pathParent.childCount - 1).rotation;
            return;
        }

        Transform pointA = pathParent.GetChild(index);
        Transform pointB = pathParent.GetChild(index + 1);

        position = Vector3.Lerp(pointA.position, pointB.position, segmentT);
        rotation = Quaternion.Slerp(pointA.rotation, pointB.rotation, segmentT);
    }

    public void GetPointAtInWorldUnits(float worldDistance, out Vector3 position, out Quaternion rotation)
    {
        float t = worldDistance / worldLength;
        GetPointAt(t, out position, out rotation);
    }

    private void UpdateWorldLength()
    {
        if (pathParent == null || pathParent.childCount < 2)
        {
            worldLength = 27f;
            return;
        }

        float totalLength = 0f;
        for (int i = 1; i < pathParent.childCount; i++)
        {
            Transform prev = pathParent.GetChild(i - 1);
            Transform current = pathParent.GetChild(i);
            totalLength += Vector3.Distance(prev.position, current.position);
        }
        worldLength = totalLength;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (pathParent == null || pathParent.childCount < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 1; i < pathParent.childCount; i++)
        {
            Transform prev = pathParent.GetChild(i - 1);
            Transform current = pathParent.GetChild(i);
            Gizmos.DrawLine(prev.position, current.position);
            Gizmos.DrawSphere(prev.position, 0.2f);
            Gizmos.DrawSphere(current.position, 0.2f);
        }
    }
#endif
}