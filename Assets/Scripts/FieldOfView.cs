using System.Collections;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class FieldOfView : MonoBehaviour
{
    [SerializeField] private float _radius;
    [SerializeField, Range(0,360)] private float _angle;
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private LayerMask _obstructionMask;
    private bool _canSeePlayer;
    [SerializeField] public Player player;

    public float radius => _radius;
    public float angle => _angle;
    public bool canSeePlayer => _canSeePlayer;
    public LayerMask targetMask => _targetMask;
    public LayerMask obstructionMask => _obstructionMask;

    public void Start()
    {
        StartCoroutine(FOVRoutine());
    }

    private IEnumerator FOVRoutine()
    {
        WaitForSeconds wait = new WaitForSeconds(6.2f);
        while (true)
        {
            yield return null;
            FieldOfCheck();
        }
    }

    public void FieldOfCheck()
    {

        Collider[] rangechecks = Physics.OverlapSphere(transform.position, _radius, targetMask);
        if (rangechecks.Length != 0)
        {
            Transform target = rangechecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    _canSeePlayer = true;
                else
                    _canSeePlayer = false;
            }
            else _canSeePlayer = false;
        }
        else if (_canSeePlayer) _canSeePlayer = false;

    }

    private void OnDrawGizmos()
    {
        Vector3 viewAngle01 = DirectionFromAngle(transform.eulerAngles.y, -angle / 2);
        Vector3 viewAngle02 = DirectionFromAngle(transform.eulerAngles.y, angle / 2);
        Handles.color = Color.yellow;
        Handles.DrawLine(transform.position, transform.position + viewAngle01 * radius);
        Handles.DrawLine(transform.position, transform.position + viewAngle02 * radius);
    }
    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}
