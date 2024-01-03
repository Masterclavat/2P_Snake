using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AggroBot : SnakeBot {
   private int defensiveSegments = 6;
   List<Vector2Int> lastPath;

   public override SnakeDirection Tick(GameState gameState, SnakeData mySnake, SnakeData otherSnake) {
      if (mySnake.Segments.Count < defensiveSegments) {
         return PlayDefensively(gameState, mySnake, otherSnake);
      }
      else {
         List<Vector2Int> path = null;
         int distanceToEnemy = BotUtilities.CalculateDistance(mySnake.Head, otherSnake.Head);
         if ( distanceToEnemy <5) {
            Vector2Int nearestEnemyFood = gameState.FindNearestFood(otherSnake);
            //if (BotUtilities.IsLocationNearABorder(gameState, nearestEnemyFood, 4))
            path = BotUtilities.FindPathToTargetFood_AStar(gameState, mySnake, nearestEnemyFood);
            if (path.Count == 1 && path[0] == nearestEnemyFood) {
               int numUnsafeTilesNextToFood = 0;
               Vector2Int[] adjLocations = BotUtilities.GetAdjacentLocations(path[0]);
               List<Vector2Int> safeAdjLocations = new List<Vector2Int>();
               foreach (Vector2Int loc in adjLocations) {
                  if (!gameState.IsTileSafe(loc)) {
                     numUnsafeTilesNextToFood++;
                  }
                  else {
                     safeAdjLocations.Add(loc);
                  }
               }
               if (numUnsafeTilesNextToFood < 3) {
                  path = BotUtilities.FindPathToTargetLocation_AStar(gameState, mySnake, safeAdjLocations[0]);
               }
               else {
                  Vector2Int nearestFood = gameState.FindNearestFood(mySnake);
                  path = BotUtilities.FindPathToNearestFood_AStar(gameState, mySnake, new Vector2Int[] { nearestFood });
               }
            }
         }
         else {
            //if (BotUtilities.IsSnakeNearABorder(gameState, otherSnake, 2)) {
            //   Vector2Int otherSnakePosInTwoTicks = otherSnake.Head + BotUtilities.DirectionChange[otherSnake.Direction] * 2;
            //   if (gameState.IsTileSafe(otherSnakePosInTwoTicks)) {
            //      path = BotUtilities.FindPathToTargetLocation_AStar(gameState, mySnake, otherSnakePosInTwoTicks);
            //   }
            //   else {
            //      foreach (Vector2Int dirChange in BotUtilities.DirectionChange.Values) {
            //         if (gameState.IsTileSafe(otherSnake.NextHeadPosition + dirChange)) {
            //            path = BotUtilities.FindPathToTargetLocation_AStar(gameState, mySnake, otherSnake.NextHeadPosition + dirChange);
            //            break;
            //         }
            //      }
            //   }
            //}
         }
         if (path == null) {
            //Vector2Int nearestFood = gameState.FindNearestFood(mySnake);
            //path = BotUtilities.FindPathToTargetFood_AStar(gameState, mySnake, nearestEnemyFood);
            path = BotUtilities.FindPathToNearestFood_AStar(gameState, mySnake);
         }

         //Speichere den Pfad, um ihn anzeigen lassen zu können
         mySnake.DebugData = path.ToArray();
         if (path.Count > 0) {
            //Wenn der Kopf der gegnerischen Schlange direkt neben der nächsten Koordinate des Pfades liegt,
            //folge nicht dem Pfad und weiche auf ein sicheres Feld aus
            //Dies verhindert in den meisten Fällen, dass die Runde unentschieden endet
            if (BotUtilities.CalculateDistance(otherSnake.Head, path[0]) == 1) {
               int highestDistance = -1;
               SnakeDirection bestDir = mySnake.Direction;
               foreach (var dir in BotUtilities.DirectionChange.Keys) {
                  Vector2Int next = BotUtilities.DirectionChange[dir] + mySnake.Head;
                  if (next != path[0] && gameState.IsTileSafe(next)) {
                     int d = BotUtilities.CalculateDistance(otherSnake.Head, next);
                     if (highestDistance < d) {
                        highestDistance = d;
                        bestDir = dir;
                     }
                  }
               }

               return bestDir;
            }

            //Ansonsten, folge dem Pfad
            foreach (var dirChange in BotUtilities.DirectionChange) {
               if (dirChange.Value == path[0] - mySnake.Head)
                  return dirChange.Key;
            }
         }
         return mySnake.Direction;
      }
   }

   private SnakeDirection PlayDefensively(GameState gameState, SnakeData mySnake, SnakeData otherSnake) {
      List<Vector2Int> path = null;
      //Finde einen Pfad zur nächsten Futterkoordinate, falls noch kein gültiger Pfad
      //aus einem vorherigen Tick vorhanden ist
      if (lastPath == null || lastPath.Count == 0 || lastPath[0] != mySnake.Head || !gameState.FoodLocations.Contains(lastPath.Last())) {
         path = BotUtilities.FindPathToNearestFood_AStar(gameState, mySnake);
         lastPath = path;
      }
      else {
         //Finde heraus, ob der Pfad, der in einem früheren Tick gefunden wurde noch gültig ist
         //Ein Pfad wird ungültig, wenn eine seiner Koordinaten von einer Schlange belegt wurde
         //Ist dies der Fall, berechne einen neuen Pfad
         for (int i = 1; i < lastPath.Count; i++) {
            if (!gameState.IsTileSafe(lastPath[i])) {
               path = BotUtilities.FindPathToNearestFood_AStar(gameState, mySnake);
               lastPath = path;
               break;
            }
         }
         //Wenn der alte Pfad noch gültig ist, verwende ihn als derzeitigen Pfad
         if (path == null) {
            path = lastPath;
            path.RemoveAt(0);
         }
      }
      //Speichere den Pfad, um ihn anzeigen lassen zu können
      mySnake.DebugData = path.ToArray();

      if (path.Count > 0) {
         //Wenn der Kopf der gegnerischen Schlange direkt neben der nächsten Koordinate des Pfades liegt,
         //folge nicht dem Pfad und weiche auf ein sicheres Feld aus
         //Dies verhindert in den meisten Fällen, dass die Runde unentschieden endet
         if (BotUtilities.CalculateDistance(otherSnake.Head, path[0]) == 1) {
            foreach (var dir in BotUtilities.DirectionChange.Keys) {
               Vector2Int next = BotUtilities.DirectionChange[dir] + mySnake.Head;
               if (next != path[0] && gameState.IsTileSafe(next)) {
                  return dir;
               }
            }
         }

         //Ansonsten, folge dem Pfad
         foreach (var dirChange in BotUtilities.DirectionChange) {
            if (dirChange.Value == path[0] - mySnake.Head)
               return dirChange.Key;
         }
      }
      return mySnake.Direction;
   }
}
