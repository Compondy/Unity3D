using Game.GameEngine.Ecs;
using SampleProject;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class RtsController : MonoBehaviour
{
    private RtsControls controls;
    private List<Entity> selectedUnits = new List<Entity>();
    private Vector2 mouseStartPos;
    private bool isDragging;
    private Camera mainCamera;

    private void Awake()
    {
        this.controls = new RtsControls();
        this.mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        this.controls.Enable();

        this.controls.RTS.Selection.started += this.OnSelectStarted;
        this.controls.RTS.Selection.canceled += this.OnSelectEnded;
        this.controls.RTS.MoveCommand.performed += this.OnMoveCommand;
    }

    private void OnDisable()
    {
        this.controls.RTS.Selection.started -= this.OnSelectStarted;
        this.controls.RTS.Selection.canceled -= this.OnSelectEnded;
        this.controls.RTS.MoveCommand.performed -= this.OnMoveCommand;

        this.controls.Disable();
    }

    private void OnSelectStarted(InputAction.CallbackContext context)
    {
        this.mouseStartPos = Mouse.current.position.ReadValue();
        this.isDragging = true;

        if (!Keyboard.current.leftCtrlKey.isPressed)
        {
            this.ClearSelection();
        }
    }

    private void OnSelectEnded(InputAction.CallbackContext context)
    {
        if (!this.isDragging) return;

        this.isDragging = false;
        this.SelectUnitsInRectangle();
    }

    private void OnMoveCommand(InputAction.CallbackContext context)
    {
        if (this.selectedUnits.Count == 0) return;

        Ray ray = this.mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        int layerMask = LayerMask.GetMask("Ground");

        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
        {
            this.SendMoveCommand(hit.point);
        }
    }

    private void SelectUnitsInRectangle()
    {
        Vector2 mouseEndPos = Mouse.current.position.ReadValue();

        float minX = Mathf.Min(this.mouseStartPos.x, mouseEndPos.x);
        float maxX = Mathf.Max(this.mouseStartPos.x, mouseEndPos.x);
        float minY = Mathf.Min(this.mouseStartPos.y, mouseEndPos.y);
        float maxY = Mathf.Max(this.mouseStartPos.y, mouseEndPos.y);

        Rect selectionRect = new Rect(minX, minY, maxX - minX, maxY - minY);

        var blueUnits = FindObjectsOfType<Faction1Entity>();

        foreach (var unit in blueUnits)
        {
            Vector3 screenPos = this.mainCamera.WorldToScreenPoint(unit.transform.position);

            if (selectionRect.Contains(screenPos))
            {
                if (!this.selectedUnits.Contains(unit))
                {
                    this.selectedUnits.Add(unit);
                    Debug.Log($"Selected: {unit.name}");
                }
            }
        }

        Debug.Log($"Total selected: {this.selectedUnits.Count}");
    }

    private void ClearSelection()
    {
        this.selectedUnits.Clear();
        Debug.Log("Selection cleared");
    }

    private void SendMoveCommand(Vector3 position)
    {
        Debug.Log($"Moving {this.selectedUnits.Count} units to {position}");

        var command = new CommandRequest
        {
            type = CommandType.MOVE_TO_POSITION,
            status = CommandStatus.IDLE,
            args = position
        };

        foreach (var unit in this.selectedUnits)
        {
            if (unit != null && unit.IsExists())
            {
                unit.SetData(command);
            }
        }
    }

    private void OnGUI()
    {
        if (this.isDragging)
        {
            Vector2 mouseEndPos = Mouse.current.position.ReadValue();
            float minX = Mathf.Min(this.mouseStartPos.x, mouseEndPos.x);
            float maxX = Mathf.Max(this.mouseStartPos.x, mouseEndPos.x);
            float minY = Mathf.Min(this.mouseStartPos.y, mouseEndPos.y);
            float maxY = Mathf.Max(this.mouseStartPos.y, mouseEndPos.y);

            Rect rect = new Rect(minX, Screen.height - minY, maxX - minX, minY - maxY);

            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, new Color(0, 1, 0, 0.3f));
            texture.Apply();

            GUI.Box(rect, "", new GUIStyle { normal = { background = texture } });
        }
    }
}