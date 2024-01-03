using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die Basisklasse, von dem jeder Bot erben muss
/// </summary>
public abstract class SnakeBot
{
   public abstract SnakeDirection Tick(GameState gameState, SnakeData mySnake, SnakeData otherSnake);
}
