using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGameplayCommand
{
    abstract public void Interact(Cell cell);
}
