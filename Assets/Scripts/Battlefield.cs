using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class BattleField : MonoBehaviour
{
    public GameObject redCheckerPrefab;
    public GameObject whiteCheckerPrefab;
    public GameObject redQueenCheckerPrefab;
    public GameObject whiteQueenCheckerPrefab;

    private List<Cell> cells;
    private List<PossibleMove> possibleMoves;

    private Cell selectedCheckerCell;
    private Cell selectedMoveCell;

    [Inject]
    private State _state;

    private GameObject turnIndicatorWhite;
    private GameObject turnIndicatorRed;

    private GameObject redCamera;
    private GameObject whiteCamera;

    public bool Turn { get; set; } //false = White, true = Red

    public void NextTurn()
    {
        if (Turn)
        {
            Turn = false;
            turnIndicatorRed.SetActive(false);
            turnIndicatorWhite.SetActive(true);
            redCamera.SetActive(false);
            whiteCamera.SetActive(true);
        }
        else
        {
            turnIndicatorRed.SetActive(true);
            turnIndicatorWhite.SetActive(false);
            redCamera.SetActive(true);
            whiteCamera.SetActive(false);
            Turn = true;
        }

    }

    public void Init()
    {
        turnIndicatorWhite = GameObject.Find("TurnIndicatorWhite");
        turnIndicatorRed = GameObject.Find("TurnIndicatorRed");
        turnIndicatorRed.SetActive(false);

        redCamera = GameObject.Find("RedCamera");
        whiteCamera = GameObject.Find("Main Camera");

        redCamera.SetActive(false);

        possibleMoves = new List<PossibleMove>();
        GameObject board = GameObject.Find("Board");
        //add all cells and their base params
        List<Cell> boardCells = new List<Cell>();
        for (int i = 0; i != board.transform.childCount; i++) //iterate rows
        {
            var row = board.transform.GetChild(i).gameObject;
            int cellsInRow = row.transform.childCount;
            for (int h = 0; h != cellsInRow; h++)
            {
                var cellObject = row.transform.GetChild(h).gameObject;
                var cellComponent = cellObject.GetComponent<Cell>();
                boardCells.Add(cellComponent);
            }
        }
        cells = boardCells;

        //populate neighbors
        foreach (var cell in boardCells)
        {
            cell.TopMiddleRight = GetTopMiddleRight(cell);
            cell.TopMiddleLeft = GetTopMiddleLeft(cell);
            cell.BottomMiddleLeft = GetBottomMiddleLeft(cell);
            cell.BottomMiddleRight = GetBottomMiddleRight(cell);
        }

        //populate row/index info
        float rowX = -3.5f;
        for (int i = 0; i != 8; i++)
        {
            var rowcells = boardCells.Where(x => Math.Round(x.transform.position.x, 2) == rowX).OrderByDescending(x => x.transform.position.z).ToList();
            for (int h = 0; h != rowcells.Count; h++)
            {
                var cell = rowcells[h];
                if (cell.GetComponent<Renderer>().material.name.StartsWith("Black")) cell.IsBlack = true; else cell.IsBlack = false; // cell Awake is executed later
                cell.row = i;
                cell.index = h;
            }
            rowX += 1.0f;
        }

        //instantiate checkers

        foreach (var cell in cells.Where(x => (x.row == 0 || x.row == 1 || x.row == 2) && x.IsBlack))
        {
            var checker = Instantiate(whiteCheckerPrefab, cell.transform.position, new Quaternion());
            var renderer = checker.GetComponent<Renderer>();
            cell.checker = new Unit() { checker = checker, isRed = false, renderer = renderer, material = renderer.material };
        }

        foreach (var cell in cells.Where(x => (x.row == 5 || x.row == 6 || x.row == 7) && x.IsBlack))
        {
            var checker = Instantiate(redCheckerPrefab, cell.transform.position, new Quaternion());
            var renderer = checker.GetComponent<Renderer>();
            cell.checker = new Unit() { checker = checker, isRed = true, renderer = renderer, material = renderer.material };
        }


        //get all possible moves for the first time
        possibleMoves = GetPossibleMoves();
    }

    public Cell GetTopMiddleRight(Cell cell)
    {
        return cells.Where(x => Math.Round(x.transform.position.x, 3) == Math.Round(cell.transform.position.x + 1, 3) && Math.Round(x.transform.position.z, 3) == Math.Round(cell.transform.position.z - 1, 3)).FirstOrDefault();
    }

    public Cell GetBottomMiddleRight(Cell cell)
    {
        return cells.Where(x => Math.Round(x.transform.position.x, 3) == Math.Round(cell.transform.position.x - 1, 3) && Math.Round(x.transform.position.z, 3) == Math.Round(cell.transform.position.z - 1, 3)).FirstOrDefault();
    }

    public Cell GetTopMiddleLeft(Cell cell)
    {
        return cells.Where(x => Math.Round(x.transform.position.x, 3) == Math.Round(cell.transform.position.x + 1, 3) && Math.Round(x.transform.position.z, 3) == Math.Round(cell.transform.position.z + 1, 3)).FirstOrDefault();
    }

    public Cell GetBottomMiddleLeft(Cell cell)
    {
        return cells.Where(x => Math.Round(x.transform.position.x, 3) == Math.Round(cell.transform.position.x - 1, 3) && Math.Round(x.transform.position.z, 3) == Math.Round(cell.transform.position.z + 1, 3)).FirstOrDefault();
    }

    public List<PossibleMove> GetPossibleMoves()
    {
        //true = Red
        //false = White
        var moves = new List<PossibleMove>();
        foreach (var cell in cells.Where(x => x.checker != null && x.checker.isRed == Turn))
        {
            moves.AddRange(GetPossibleMoves(cell));
        }
        //remove moves without attacks if attack is possible
        if (moves.Any(x => x.attacks > 0)) moves = moves.Where(x => x.attacks > 0).ToList();
        possibleMoves = moves;
        return possibleMoves;
    }

    private List<PossibleMove> GetQueenMoves(Cell cell)
    {
        List<PossibleMove> moves = new List<PossibleMove>();

        int enemyCounter = 0;
        Cell current = cell;
        while (true)
        {
            if (current.TopMiddleRight != null)
            {
                if (current.TopMiddleRight.checker != null)
                {
                    if (current.TopMiddleRight.checker.isRed == Turn) break; //friend
                    else
                    {
                        if (enemyCounter == 1) break;
                        enemyCounter += 1;

                    }
                }
                else
                {
                    moves.Add(new PossibleMove() { attacks = enemyCounter, checkerCell = cell, moveCell = current.TopMiddleRight });
                }

            }
            else break;
            current = current.TopMiddleRight;
        }

        enemyCounter = 0;
        current = cell;
        while (true)
        {
            if (current.TopMiddleLeft != null)
            {
                if (current.TopMiddleLeft.checker != null)
                {
                    if (current.TopMiddleLeft.checker.isRed == Turn) break; //friend
                    else
                    {
                        if (enemyCounter == 1) break;
                        enemyCounter += 1;

                    }
                }
                else
                {
                    moves.Add(new PossibleMove() { attacks = enemyCounter, checkerCell = cell, moveCell = current.TopMiddleLeft });
                }

            }
            else break;
            current = current.TopMiddleLeft;
        }

        enemyCounter = 0;
        current = cell;
        while (true)
        {
            if (current.BottomMiddleLeft != null)
            {
                if (current.BottomMiddleLeft.checker != null)
                {
                    if (current.BottomMiddleLeft.checker.isRed == Turn) break; //friend
                    else
                    {
                        if (enemyCounter == 1) break;
                        enemyCounter += 1;

                    }
                }
                else
                {
                    moves.Add(new PossibleMove() { attacks = enemyCounter, checkerCell = cell, moveCell = current.BottomMiddleLeft });
                }

            }
            else break;
            current = current.BottomMiddleLeft;
        }

        enemyCounter = 0;
        current = cell;
        while (true)
        {
            if (current.BottomMiddleRight != null)
            {
                if (current.BottomMiddleRight.checker != null)
                {
                    if (current.BottomMiddleRight.checker.isRed == Turn) break; //friend
                    else
                    {
                        if (enemyCounter == 1) break;
                        enemyCounter += 1;

                    }
                }
                else
                {
                    moves.Add(new PossibleMove() { attacks = enemyCounter, checkerCell = cell, moveCell = current.BottomMiddleRight });
                }

            }
            else break;
            current = current.BottomMiddleRight;
        }

        return moves;
    }

    public List<PossibleMove> GetPossibleMoves(Cell cell)
    {
        if (cell.checker == null) return null;

        List<PossibleMove> moves = new List<PossibleMove>();
        if (!Turn) //White
        {
            if (!cell.checker.isQueen)
            {

                if (cell.BottomMiddleLeft != null)
                {
                    //check backward move with attack possibility

                    if (cell.BottomMiddleLeft.checker != null //checker exist
                        && cell.BottomMiddleLeft.checker.isRed != cell.checker.isRed //checker is enemy
                        && cell.BottomMiddleLeft.BottomMiddleLeft != null && cell.BottomMiddleLeft.BottomMiddleLeft.checker == null) //next cell exist and empty
                        moves.Add(new PossibleMove() { moveCell = cell.BottomMiddleLeft.BottomMiddleLeft, checkerCell = cell, attacks = 1 });
                }

                if (cell.BottomMiddleRight != null)
                {
                    if (cell.BottomMiddleRight.checker != null //checker exist
                       && cell.BottomMiddleRight.checker.isRed != cell.checker.isRed //checker is enemy
                       && cell.BottomMiddleRight.BottomMiddleRight != null && cell.BottomMiddleRight.BottomMiddleRight.checker == null) //next cell exist and empty
                        moves.Add(new PossibleMove() { moveCell = cell.BottomMiddleRight.BottomMiddleRight, checkerCell = cell, attacks = 1 });
                }

                if (cell.TopMiddleLeft != null)
                {
                    //forward move check
                    if (cell.TopMiddleLeft.checker == null)
                        moves.Add(new PossibleMove() { moveCell = cell.TopMiddleLeft, checkerCell = cell, attacks = 0 }); //free cell
                    else if (cell.TopMiddleLeft.TopMiddleLeft != null && cell.TopMiddleLeft.checker.isRed != cell.checker.isRed && cell.TopMiddleLeft.TopMiddleLeft.checker == null) //not free and enemy and next one is free
                        moves.Add(new PossibleMove() { moveCell = cell.TopMiddleLeft.TopMiddleLeft, checkerCell = cell, attacks = 1 });
                }

                if (cell.TopMiddleRight != null)
                {
                    if (cell.TopMiddleRight.checker == null)
                        moves.Add(new PossibleMove() { moveCell = cell.TopMiddleRight, checkerCell = cell, attacks = 0 }); //free cell
                    else if (cell.TopMiddleRight.TopMiddleRight != null && cell.TopMiddleRight.checker.isRed != cell.checker.isRed && cell.TopMiddleRight.TopMiddleRight.checker == null) //not free and enemy and next one is free
                        moves.Add(new PossibleMove() { moveCell = cell.TopMiddleRight.TopMiddleRight, checkerCell = cell, attacks = 1 });
                }
            }
            else
                moves.AddRange(GetQueenMoves(cell));

        }
        else
        {
            if (!cell.checker.isQueen)
            {
                if (cell.TopMiddleLeft != null)
                {
                    //check backward move with attack possibility

                    if (cell.TopMiddleLeft.checker != null //checker exist
                        && cell.TopMiddleLeft.checker.isRed != cell.checker.isRed //checker is enemy
                        && cell.TopMiddleLeft.TopMiddleLeft != null && cell.TopMiddleLeft.TopMiddleLeft.checker == null) //next cell exist and empty
                        moves.Add(new PossibleMove() { moveCell = cell.TopMiddleLeft.TopMiddleLeft, checkerCell = cell, attacks = 1 });
                }

                if (cell.TopMiddleRight != null)
                {
                    if (cell.TopMiddleRight.checker != null //checker exist
                       && cell.TopMiddleRight.checker.isRed != cell.checker.isRed //checker is enemy
                       && cell.TopMiddleRight.TopMiddleRight != null && cell.TopMiddleRight.TopMiddleRight.checker == null) //next cell exist and empty
                        moves.Add(new PossibleMove() { moveCell = cell.TopMiddleRight.TopMiddleRight, checkerCell = cell, attacks = 1 });
                }

                if (cell.BottomMiddleLeft != null)
                {
                    //forward move check
                    if (cell.BottomMiddleLeft.checker == null)
                        moves.Add(new PossibleMove() { moveCell = cell.BottomMiddleLeft, checkerCell = cell, attacks = 0 }); //free cell
                    else if (cell.BottomMiddleLeft.BottomMiddleLeft != null && cell.BottomMiddleLeft.checker.isRed != cell.checker.isRed && cell.BottomMiddleLeft.BottomMiddleLeft.checker == null) //not free and enemy and next one is free
                        moves.Add(new PossibleMove() { moveCell = cell.BottomMiddleLeft.BottomMiddleLeft, checkerCell = cell, attacks = 1 });
                }

                if (cell.BottomMiddleRight != null)
                {
                    if (cell.BottomMiddleRight.checker == null)
                        moves.Add(new PossibleMove() { moveCell = cell.BottomMiddleRight, checkerCell = cell, attacks = 0 }); //free cell
                    else if (cell.BottomMiddleRight.BottomMiddleRight != null && cell.BottomMiddleRight.checker.isRed != cell.checker.isRed && cell.BottomMiddleRight.BottomMiddleRight.checker == null) //not free and enemy and next one is free
                        moves.Add(new PossibleMove() { moveCell = cell.BottomMiddleRight.BottomMiddleRight, checkerCell = cell, attacks = 1 });
                }
            }
            else
                moves.AddRange(GetQueenMoves(cell));
        }

        return moves;
    }

    public void SetSelectedChecker(Cell cell)
    {
        selectedCheckerCell = cell;
    }
    public void SetSelectedMoveCell(Cell cell)
    {
        selectedMoveCell = cell;
    }

    public Cell GetSelectedChecker()
    {
        return selectedCheckerCell;
    }
    public Cell GetSelectedMoveCell()
    {
        return selectedMoveCell;
    }

    public Cell CellInList(Cell cell)
    {
        return cells.Where(x => x == cell).FirstOrDefault();
    }

    public void ClearSelection(bool force = false)
    {
        if ((checkerToQueen != 1 || force) && !(itWasAttack && possibleMoves.Any(x=>x.attacks>0)))
        {
            _state.flag = (int)GameState.SelectChecker;
            selectedCheckerCell = null;
            selectedMoveCell = null;
            foreach (var cell in cells)
            {
                if (cell.checker != null && cell.checker.renderer.material != cell.checker.material) cell.checker.renderer.material = cell.checker.material;
                if (cell._renderer.material != cell.material) cell._renderer.material = cell.material;
            }
        } else if (itWasAttack && possibleMoves.Any(x => x.attacks > 0)) //can remove only target selection
        {
            selectedMoveCell = null;
            _state.flag = (int)GameState.SelectCell;
            foreach (var cell in possibleMoves.Select(x => x.moveCell))
            {
                cell.Deselect(cell);
            }
        }
    }

    public void SetState(int state)
    {
        _state.flag = state;
    }
    public int GetState()
    {
        return _state.flag;
    }

    private int checkerToQueen;
    public bool itWasAttack;
    public void ApproveSelection()
    {
        itWasAttack = false;
        if (_state.flag != (int)GameState.ApproveMove || selectedCheckerCell == null) return;

        var checker = selectedCheckerCell.checker;

        selectedCheckerCell.checker = null;
        if (checkerToQueen == 1) checkerToQueen = 2;
        if ((!checker.isQueen && checker.isRed && selectedMoveCell.row == 0) || (!checker.isQueen && !checker.isRed && selectedMoveCell.row == 7)) //replace with queen
        {
            checkerToQueen = 1;
            Destroy(checker.checker);
            if (checker.isRed) { checker.checker = Instantiate(redQueenCheckerPrefab, selectedMoveCell.transform.position, new Quaternion()); } else { checker.checker = Instantiate(whiteQueenCheckerPrefab, selectedMoveCell.transform.position, new Quaternion()); }
            checker.renderer = checker.checker.GetComponent<Renderer>();
            selectedMoveCell.checker = checker;
            checker.isQueen = true;
            //do not turn side and next move is by queen
        }
        else
        {
            checker.checker.transform.position = selectedMoveCell.transform.position;
            selectedMoveCell.checker = checker;
        }

        int difference = Math.Abs(selectedMoveCell.index - selectedCheckerCell.index) - 1;
        if (difference > 0) //remove eaten checkers
        {
            itWasAttack = true;
            if (selectedMoveCell.index > selectedCheckerCell.index && selectedMoveCell.row > selectedCheckerCell.row) //topright move
            {
                int index = selectedCheckerCell.index + 1;
                for (int r = selectedCheckerCell.row + 1; r < selectedMoveCell.row; r++)
                {
                    var cell = cells.Where(x => x.row == r && x.index == index).FirstOrDefault();
                    if (cell != null && cell.checker != null)
                    {
                        Destroy(cell.checker.checker);
                        cell.checker = null;
                    }
                    index += 1;
                }
            }
            else

            if (selectedMoveCell.index > selectedCheckerCell.index && selectedMoveCell.row < selectedCheckerCell.row) //bottomright move
            {
                int index = selectedCheckerCell.index + 1;
                for (int r = selectedCheckerCell.row - 1; r > selectedMoveCell.row; r--)
                {
                    var cell = cells.Where(x => x.row == r && x.index == index).FirstOrDefault();
                    if (cell != null && cell.checker != null)
                    {
                        Destroy(cell.checker.checker);
                        cell.checker = null;
                    }
                    index += 1;
                }
            }
            else
            if (selectedMoveCell.index < selectedCheckerCell.index && selectedMoveCell.row > selectedCheckerCell.row) //topleft move
            {
                int index = selectedCheckerCell.index - 1;
                for (int r = selectedCheckerCell.row + 1; r < selectedMoveCell.row; r++)
                {
                    var cell = cells.Where(x => x.row == r && x.index == index).FirstOrDefault();
                    if (cell != null && cell.checker != null)
                    {
                        Destroy(cell.checker.checker);
                        cell.checker = null;
                    }
                    index -= 1;
                }
            }
            else
            if (selectedMoveCell.index < selectedCheckerCell.index && selectedMoveCell.row < selectedCheckerCell.row) //bottomleft move
            {
                int index = selectedCheckerCell.index - 1;
                for (int r = selectedCheckerCell.row - 1; r > selectedMoveCell.row; r--)
                {
                    var cell = cells.Where(x => x.row == r && x.index == index).FirstOrDefault();
                    if (cell != null && cell.checker != null)
                    {
                        Destroy(cell.checker.checker);
                        cell.checker = null;
                    }
                    index -= 1;
                }
            }
        }


        switch (checkerToQueen)
        {
            case 0:
                {
                    var temp = selectedMoveCell;
                    possibleMoves = GetPossibleMoves(selectedMoveCell);
                    if (possibleMoves.Any(x => x.attacks > 0) && itWasAttack) //next attack
                    {
                        ClearSelection(force: true);
                        selectedCheckerCell = temp;
                        temp.SelectChecker(temp);
                    }
                    else
                    {
                        ClearSelection();
                        NextTurn();
                        GetPossibleMoves();
                    }
                    break;
                }
            case 1:
                {
                    var temp = selectedMoveCell;
                    possibleMoves = GetPossibleMoves(selectedMoveCell); //next move is by Queen
                    ClearSelection(force: true);
                    selectedCheckerCell = temp;
                    temp.SelectChecker(temp);
                    break;
                }
            case 2:
                {
                    var temp = selectedMoveCell;
                    possibleMoves = GetPossibleMoves(selectedMoveCell);
                    if (possibleMoves.Any(x => x.attacks > 0) && itWasAttack) //next attack
                    {
                        ClearSelection(force: true);
                        selectedCheckerCell = temp;
                        temp.SelectChecker(temp);
                    }
                    else
                    {
                        ClearSelection();
                        NextTurn();
                        GetPossibleMoves();
                    }
                    checkerToQueen = 0;
                    break;
                }
        }

    }

    public bool CanMove(Cell cell)
    {
        return possibleMoves.Any(x => x.checkerCell == cell);
    }


}
