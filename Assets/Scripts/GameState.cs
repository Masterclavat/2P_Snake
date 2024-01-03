using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : ICloneable {
   public SnakeData Snake_1;
   public SnakeData Snake_2;
   public List<Vector2Int> FoodLocations = new List<Vector2Int>();
   public Vector2Int GridSize = new Vector2Int(30, 20);

   public bool IsGameInProgress { get; set; }

   /// <summary>
   /// Findet heraus, welche der beiden Schlangen zu einem SnakeBot geh�rt.
   /// </summary>
   /// <param name="bot">Der SnakeBot, dessen Schlange gefunden werden soll.</param>
   /// <returns>Die Schlange, die zum SnakeBot im Parameter bot geh�rt.</returns>
   public SnakeData FindMySnake(SnakeBot bot) {
      if (Snake_1.Owner == bot)
         return Snake_1;
      else if (Snake_2.Owner == bot)
         return Snake_2;
      else
         return null;
   }

   /// <summary>
   /// Findet die n�chste Futterkoordinate zum Kopf einer Schlange.
   /// </summary>
   /// <param name="snake">Die Schlange, von der aus die Suche stattfinden soll.</param>
   /// <returns>Gibt die Koordinate des n�chsten Futters zur�ck. Wenn es kein Futter gibt,
   /// wird die Koordinate (0, 0) zur�ckgegeben.
   /// </returns>
   public Vector2Int FindNearestFood(SnakeData snake) {
      int lowestDistance = int.MaxValue;
      Vector2Int nearest = Vector2Int.zero;
      int dist;
      foreach (Vector2Int food in FoodLocations) {
         dist = BotUtilities.CalculateDistance(food, snake.Head);
         if (dist < lowestDistance) {
            lowestDistance = dist;
            nearest = food;
         }
      }

      return nearest;
   }

   /// <summary>
   /// Gibt an, ob eine Koordinate als sicher gilt. Eine sichere Koordinate ist eine, die kein
   /// Segment einer Schlange enth�lt und sich innerhalb des Spielfelds befindet.
   /// Eine unsichere Koordinate f�hrt zum Tod einer Schlange.
   /// </summary>
   /// <param name="tile">Die Koordinate, die auf Sicherheit gepr�ft werden soll.</param>
   /// <returns>Gibt true zur�ck, wenn die Koordinate sicher ist, andernfalls gibt false zur�ck.</returns>
   public bool IsTileSafe(Vector2Int tile) {
      if (Snake_1.Segments.Contains(tile))
         return false;
      if (Snake_2.Segments.Contains(tile))
         return false;
      if (tile.x < 0 || tile.x >= GridSize.x || tile.y < 0 || tile.y >= GridSize.y)
         return false;

      return true;
   }

   /// <summary>
   /// Eine Interne Funktion, die f�r das Spiel wichtig ist. Kann ignoriert werden.
   /// Falls du dich daf�r interessierst, was sie macht:
   /// Erstellt eine Kopie des derzeitigen Game State. Wird f�r das Replay von Runden verwendet.
   /// </summary>
   /// <returns>Die Kopie des Objekts</returns>
   public object Clone() {
      GameState clone = new GameState();
      clone.Snake_1 = (SnakeData)Snake_1.Clone();
      clone.Snake_2 = (SnakeData)Snake_2.Clone();
      clone.FoodLocations = new List<Vector2Int>(FoodLocations.ToArray());
      clone.GridSize = GridSize;

      return clone;
   }
}
