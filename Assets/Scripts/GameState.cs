using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameState
{
public static int State { get; set; }

public static int Score { get; set; }
public static int Bonus { get; set; }
public static int TryCount { get; set; }
}

public enum GameStates
{
    Initial,
    Run,
    Finish
}
