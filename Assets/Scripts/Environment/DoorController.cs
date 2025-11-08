using UnityEngine;

public class SimpleDoorController : MonoBehaviour
{
    [Header("Door Settings")]
    public float openDistance = 3f;
    public float openSpeed = 2f;
    public float openAmount = 1f;

    [Header("Door Parts")]
    public Transform leftDoor;
    public Transform rightDoor;

    private AudioSource doorAudioSource;

    private Vector3 leftDoorClosedPos;
    private Vector3 rightDoorClosedPos;
    private Vector3 leftDoorOpenPos;
    private Vector3 rightDoorOpenPos;

    private Transform player;
    private bool shouldBeOpen = false;
    private bool wasOpen = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        leftDoorClosedPos = leftDoor.localPosition;
        rightDoorClosedPos = rightDoor.localPosition;

        leftDoorOpenPos = leftDoorClosedPos + Vector3.forward * openAmount;
        rightDoorOpenPos = rightDoorClosedPos + Vector3.back * openAmount;

        if (doorAudioSource == null)
        {
            doorAudioSource = GetComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        shouldBeOpen = distanceToPlayer <= openDistance;

        if (shouldBeOpen != wasOpen)
        {
            PlayDoorSound(shouldBeOpen);
        }

        wasOpen = shouldBeOpen;

        Vector3 leftTarget = shouldBeOpen ? leftDoorOpenPos : leftDoorClosedPos;
        Vector3 rightTarget = shouldBeOpen ? rightDoorOpenPos : rightDoorClosedPos;

        leftDoor.localPosition = Vector3.MoveTowards(leftDoor.localPosition, leftTarget, openSpeed * Time.deltaTime);
        rightDoor.localPosition = Vector3.MoveTowards(rightDoor.localPosition, rightTarget, openSpeed * Time.deltaTime);
    }

    void PlayDoorSound(bool opening)
    {
        if (doorAudioSource == null) return;
        doorAudioSource.Play();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, openDistance);
    }
}