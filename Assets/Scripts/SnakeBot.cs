using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SnakeBot
{
   public abstract SnakeDirection Tick(GameState gameState, SnakeData mySnake, SnakeData otherSnake);
}
