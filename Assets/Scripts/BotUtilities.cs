using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Stellt eine Reihe von nützlichen Funktionen bereit, die bei der Programmierung eines Snake-Bots helfen können.
/// </summary>
public static class BotUtilities {
   /// <summary>
   /// Weist jeder Richtung eine Koordnatenveränderung zu. Daran kann man sehen, in welche Richtung sich die Schlangen
   /// pro Tick bewegen, je nachdem welche SnakeDirection sie haben.
   /// </summary>
   public static Dictionary<SnakeDirection, Vector2Int> DirectionChange = new Dictionary<SnakeDirection, Vector2Int>(){
      {SnakeDirection.Down,   new Vector2Int(0, -1)},
      {SnakeDirection.Left,   new Vector2Int(-1, 0)},
      {SnakeDirection.Up,     new Vector2Int(0, 1)},
      {SnakeDirection.Right,  new Vector2Int(1, 0)}};

   /// <summary>
   /// Berechnet den schnellsten Weg zur nächsten Futterkoordinate. Dafür wird der A* Algorithmus verwendet.
   /// </summary>
   /// <param name="gameState">Der derzeitige Game State</param>
   /// <param name="mySnake">Die Schlange, aus dessen Sicht der Weg berechnet wird</param>
   /// <param name="ignoreFood">Wenn in diesem Parameter eine Futterkoordnate übergeben wird, wird diese Koordinate 
   /// bei der Pfadsuche ignoriert und sich auf die anderen Futterkoordinaten konzentriert.
   /// Wenn dieser Parameter null ist, werden alle Futterkoordinaten in betracht gezogen.</param>
   /// <returns>Gibt eine Liste von Koordinaten zurück, die den Weg beschreiben. Startet vom Kopf der Schlange
   /// und führt bis zum nächsten Futter.</returns>
   public static List<Vector2Int> FindPathToNearestFood_AStar(GameState gameState, SnakeData mySnake, IEnumerable<Vector2Int> ignoreFood = null) {
      List<Vector2Int> validFoodTargets = new List<Vector2Int>();
      if (ignoreFood != null) {
         foreach (Vector2Int foodPos in gameState.FoodLocations) {
            if (ignoreFood.Contains(foodPos))
               continue;
            validFoodTargets.Add(foodPos);
         }
      }
      else {
         validFoodTargets.AddRange(gameState.FoodLocations);
      }

      return FindPathToNearestTargetLocation_AStar(gameState, mySnake, validFoodTargets);
   }

   public static List<Vector2Int> FindPathToTargetFood_AStar(GameState gameState, SnakeData mySnake, Vector2Int targetFood) {
      List<Vector2Int> ignoreFood = new List<Vector2Int>();
      foreach (Vector2Int foodLoc in gameState.FoodLocations) {
         if (foodLoc != targetFood)
            ignoreFood.Add(foodLoc);
      }

      return FindPathToNearestFood_AStar(gameState, mySnake, ignoreFood);
   }

   public static List<Vector2Int> FindPathToTargetLocation_AStar(GameState gameState, SnakeData mySnake, Vector2Int targetLocation) {
      return FindPathToNearestTargetLocation_AStar(gameState, mySnake, new Vector2Int[] { targetLocation });
   }

   public static List<Vector2Int> FindPathToNearestTargetLocation_AStar(GameState gameState, SnakeData mySnake, IEnumerable<Vector2Int> targetLocations) {
      List<Vector2Int> open = new List<Vector2Int>();
      List<Vector2Int> closed = new List<Vector2Int>();
      Dictionary<Vector2Int, Vector2Int> parents = new Dictionary<Vector2Int, Vector2Int>();
      open.Add(mySnake.Head);
      
      while (open.Count > 0) {
         int lowestDistance = int.MaxValue;
         Vector2Int bestNode = Vector2Int.zero;
         foreach (Vector2Int node in open) {
            //Berechne näherungsweise die Entfernung zu allen Zielpositionen
            foreach (Vector2Int locs in targetLocations) {

               int dist = CalculateDistance(node, locs);
               if (dist < lowestDistance) {
                  lowestDistance = dist;
                  bestNode = node;
               }
            }
         }
         closed.Add(bestNode);
         open.Remove(bestNode);

         foreach (Vector2Int dirChange in DirectionChange.Values) {
            Vector2Int next = bestNode + dirChange;
            if (targetLocations.Contains(next)) {
               //Rekonstruiere den Pfad
               if (!parents.ContainsKey(next)) {
                  parents.Add(next, bestNode);
               }
               else {
                  parents[next] = bestNode;
               }
               List<Vector2Int> finalPath = new List<Vector2Int>();
               Vector2Int curr = next;
               while (parents.ContainsKey(curr)) {
                  finalPath.Insert(0, curr);
                  curr = parents[curr];
               }

               return finalPath;
            }
            if (gameState.IsTileSafe(next) && !closed.Contains(next)) {
               if (!parents.ContainsKey(next)) {
                  parents.Add(next, bestNode);
               }
               else {
                  parents[next] = bestNode;
               }
               if (!open.Contains(next))
                  open.Add(next);
            }
         }
      }

      return new List<Vector2Int>();
   }

   /// <summary>
   /// Berechnet die Entfernung einer Koordinate zu einer anderen Koordinate unter der Verwendung der
   /// sogenannten "Manhattan Distance".
   /// Beispielsweise hätten die Koordninaten (5, 6) und (10, 8) eine Entfernung von 7, da die Entfernung
   /// in der x-Richtung 10-5=5 und die Entfernung in der y-Richtung 8-6=2 beträgt.
   /// </summary>
   /// <param name="from">Die erste Koordinate</param>
   /// <param name="to">Die zweite Koordinate</param>
   /// <returns>Die Entfernung zwischen der Koordinaten from und to</returns>
   public static int CalculateDistance(Vector2Int from, Vector2Int to) {
      return Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y);
   }

   public static bool IsSnakeNearABorder(GameState gameState, SnakeData targetSnake, int maxDistance = 1) {
      return targetSnake.Head.x <= maxDistance || targetSnake.Head.x >= gameState.GridSize.x - maxDistance - 1 ||
             targetSnake.Head.y <= maxDistance || targetSnake.Head.y >= gameState.GridSize.y - maxDistance - 1;
   }
}
