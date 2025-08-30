using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;


public class BallController : MonoBehaviour
{
    // Start is called before the first frame update
    LineRenderer lineRenderer;
    InputAction target;
    InputAction attack;
    float targetDirection;
    [SerializeField] float rotationSpeed;
    [SerializeField] float runSpeed;
    Rigidbody body;
    List<MeshRenderer> pins = new List<MeshRenderer>();

    [SerializeField] GameObject ballPrefab;
    [SerializeField] GameObject pinPrefab;

    TMP_Text score;
    Transform motherTransform;

    void Awake()
    {
        score = GameObject.Find("Score").GetComponent<TMP_Text>();
        GameState.State = (int)GameStates.Initial;
        lineRenderer = GetComponent<LineRenderer>();
        target = InputSystem.actions.FindAction("Target");
        attack = InputSystem.actions.FindAction("Attack");
        body = GetComponent<Rigidbody>();
        motherTransform = GameObject.Find("Pins").GetComponent<Transform>();
        for (int i = 0; i < motherTransform.childCount; i++) pins.Add(motherTransform.GetChild(i).GetComponent<MeshRenderer>());
        var down = Count();
        var up = CountUp();
        if (down == 10)
        {
            if (GameState.Bonus > 0)
                GameState.Score += down * 2;
            else
                GameState.Score += down;
            score.text = $"Score: {GameState.Score}. Strike!";
            GameState.Bonus = 2;
        }
        else
        {
            if (GameState.Bonus > 0)
                GameState.Score += down * 2;
            else
                GameState.Score += down;
            score.text = $"Score: {GameState.Score}";
            if (GameState.TryCount == 2 && up == 0)
            { score.text += ". Spare!"; GameState.Bonus = 1; }
        }
        if (up == 0)
        {
            ResetPins();
        }
    }

    // Update is called once per frame

    float targetAngle;
    float playTime;
    void Update()
    {
        if (GameState.State == (int)GameStates.Initial)
        {
            var mouse = target.ReadValue<float>();
            targetAngle += mouse * Time.deltaTime * rotationSpeed;
            var angle = Mathf.Deg2Rad * Mathf.Clamp(targetAngle, -90 + 15, 90 - 15);
            lineRenderer.SetPositions(new Vector3[] { transform.position, new Vector3(transform.position.x + 5 * Mathf.Sin(angle), transform.position.y, transform.position.z + 5 * Mathf.Cos(angle)) });
            lineRenderer.positionCount = 2;
            if (attack.WasPerformedThisFrame())
            {
                playTime = 0;
                body.AddForce(new Vector3(runSpeed * Mathf.Sin(angle), 0, runSpeed * Mathf.Cos(angle)));
                GameState.State = (int)GameStates.Run;
                lineRenderer.positionCount = 0;
            }
        }
        else if (GameState.State == (int)GameStates.Run)
        {
            playTime += Time.deltaTime;
            if ((body.velocity == Vector3.zero || body.position.y < 0) && body.position.x != 0 && body.position.y != 1.58f || playTime > 10)
            {
                Instantiate(ballPrefab, new Vector3(0, 1.58f, -24), Quaternion.identity);
                GameObject.Destroy(gameObject);
                if (GameState.Bonus > 0) GameState.Bonus -= 1;
                GameState.TryCount += 1;
            }
        }
    }

    int Count()
    {
        int down = 0;
        foreach (var pin in pins)
        {
            if (pin != null && pin.bounds.max.y < 4.31) { down += 1; GameObject.Destroy(pin.gameObject); }
        }
        return down;
    }

    int CountUp()
    {
        int down = 0;
        foreach (var pin in pins)
        {
            if (pin != null && pin.bounds.max.y > 4.31) { down += 1; }
        }
        return down;
    }


    void ResetPins()
    {
        Instantiate(pinPrefab, new Vector3(0, 0.50012f, 5 + 0), Quaternion.identity, motherTransform);
        Instantiate(pinPrefab, new Vector3(-1, 0.50012f, 5 + 2), Quaternion.identity, motherTransform);
        Instantiate(pinPrefab, new Vector3(1, 0.50012f, 5 + 2), Quaternion.identity, motherTransform);
        Instantiate(pinPrefab, new Vector3(0, 0.50012f, 5 + 4), Quaternion.identity, motherTransform);
        Instantiate(pinPrefab, new Vector3(-2, 0.50012f, 5 + 4), Quaternion.identity, motherTransform);
        Instantiate(pinPrefab, new Vector3(2, 0.50012f, 5 + 4), Quaternion.identity, motherTransform);
        Instantiate(pinPrefab, new Vector3(-1, 0.50012f, 5 + 6), Quaternion.identity, motherTransform);
        Instantiate(pinPrefab, new Vector3(-3, 0.50012f, 5 + 6), Quaternion.identity, motherTransform);
        Instantiate(pinPrefab, new Vector3(3, 0.50012f, 5 + 6), Quaternion.identity, motherTransform);
        Instantiate(pinPrefab, new Vector3(1, 0.50012f, 5 + 6), Quaternion.identity, motherTransform);
        GameState.TryCount = 0;
    }
}
