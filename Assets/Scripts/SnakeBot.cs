using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die Basisklasse, von dem jeder Bot erben muss
/// </summary>
public abstract class SnakeBot {
   /// <summary>
   /// Dies ist die Hauptfunktion jedes SnakeBots. Hier wird anhand des Game States die nächste Aktion der Schlange festgelegt.
   /// Wird in jedem Tick ein Mal aufgerufen und erwartet als Ergebnis eine Richtung, in die sich die Schlange bewegen soll.
   /// </summary>
   /// <param name="gameState">Der derzeitige Game State</param>
   /// <param name="mySnake">Die Schlange, die von diesem Bot gesteuert wird</param>
   /// <param name="otherSnake">Die gegnerische Schlange</param>
   /// <returns>Eine Richtung, in die sich die Schlange in diesem Tick bewegen soll</returns>
   public abstract SnakeDirection Tick(GameState gameState, SnakeData mySnake, SnakeData otherSnake);
}
