using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCell
{
    public GameObject cell { get; set; }
    public bool IsBlack {  get; set; }
    public Renderer renderer { get; set; } //renderer cache
    public Material material { get; set; } //original material

    public BoardCell TopMiddleRight { get; set; }
    public BoardCell TopMiddleLeft { get; set; }
    public BoardCell BottomMiddleLeft { get; set; }
    public BoardCell BottomMiddleRight { get; set; }
    public int row { get; set; }
    public int index { get; set; }
    public Unit checker { get; set; } //if checker is on cell

}
