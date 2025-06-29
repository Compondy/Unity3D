using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class Cell : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{

    [Inject]
    private BattleField battlefield;

    public Material hoverMaterial;  
    public Material hoverBlueMaterial;
    public Material redMaterial;
    public Material whiteMaterial;
    public Material selectMaterial;

    public bool IsBlack { get; set; }
    public Material material { get; set; } //original material
    public Cell TopMiddleRight { get; set; }
    public Cell TopMiddleLeft { get; set; }
    public Cell BottomMiddleLeft { get; set; }
    public Cell BottomMiddleRight { get; set; }
    public int row { get; set; }
    public int index { get; set; }
    public Unit checker { get; set; } //if checker is on cell
    public Renderer _renderer { get; set; }

    public void Awake()
    {
        _renderer = gameObject.GetComponent<Renderer>();
        if (_renderer.material.name.StartsWith("Black")) IsBlack = true; else IsBlack = false;
        material = _renderer.material;
        //transform.position = new Vector3((float)Math.Round(transform.position.x, 3), (float)Math.Round(transform.position.y, 3), (float)Math.Round(transform.position.z, 3)); //unity plays with coordinares :(
    }

    public void SelectChecker(Cell cell)
    {
        var moves = battlefield.GetPossibleMoves(cell);
        if (moves != null && moves.Count > 0)
        {
            battlefield.SetSelectedChecker(cell);
            cell.checker.renderer.material = selectMaterial;
            battlefield.SetState((int)GameState.SelectCell);

            if (moves.Any(x => x.attacks > 0)) moves = moves.Where(x => x.attacks > 0).ToList();

            foreach (var move in moves)
            {
                move.moveCell._renderer.material = hoverMaterial;
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        var item = eventData.pointerEnter.gameObject.GetComponent<Cell>();
        if (item != null)
        {
            var cell = battlefield.CellInList(item);
            if (cell != null)
            {
                if (cell.checker != null && battlefield.GetState() == (int)GameState.SelectChecker && cell.checker.isRed == battlefield.Turn)
                {
                    if (battlefield.CanMove(cell))
                        cell.checker.renderer.material = hoverMaterial;
                    else cell.checker.renderer.material = hoverBlueMaterial;
                }
                if (cell.checker == null && battlefield.GetState() == (int)GameState.SelectCell && cell._renderer.material.name.StartsWith("Hove"))
                    cell._renderer.material = selectMaterial;
                
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        var item = eventData.pointerEnter.gameObject.GetComponent<Cell>();
        if (item != null)
        {
            var cell = battlefield.CellInList(item);
            if (cell != null)
            {
                if (cell.checker != null && battlefield.GetState() == (int)GameState.SelectChecker && cell.checker.isRed == battlefield.Turn)
                {
                    if (cell.checker.isRed)
                        cell.checker.renderer.material = redMaterial;
                    else
                        cell.checker.renderer.material = whiteMaterial;
                }
                else if (cell.checker == null && battlefield.GetState() == (int)GameState.SelectCell && cell._renderer.material.name.StartsWith("Sele"))
                    cell._renderer.material = hoverMaterial;
            }
        }
    }

    public void Deselect(Cell cell)
    {
        cell._renderer.material = hoverMaterial;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var item = eventData.pointerEnter.gameObject.GetComponent<Cell>();
        if (item != null)
        {
            var cell = battlefield.CellInList(item);
            if (cell != null)
            {
                if (cell.checker != null && battlefield.GetState() == (int)GameState.SelectChecker && cell.checker.isRed == battlefield.Turn && battlefield.CanMove(cell))
                {
                    SelectChecker(cell);
                }

                if (cell.checker == null && battlefield.GetState() == (int)GameState.SelectCell && cell._renderer.material.name.StartsWith("Sele")) 
                {
                        battlefield.SetSelectedMoveCell(cell);
                        battlefield.SetState((int)GameState.ApproveMove);
                }

            }
        }
    }
}
