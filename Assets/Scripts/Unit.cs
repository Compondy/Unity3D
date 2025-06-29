using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit 
    //does not implement Point events because they are implemented through Cell (no need to process events for Unit also and make more checks)
{
    public GameObject checker { get; set; }
    public Renderer renderer { get; set; }
    public Material material { get; set; }
    public bool isRed {  get; set; }
    public bool isQueen { get; set; }
}
