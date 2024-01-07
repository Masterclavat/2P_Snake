using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AggroBot : SnakeBot {
   private int defensiveSegments = 6;
   Vector2Int targetFood;


   public override SnakeDirection Tick(GameState gameState, SnakeData mySnake, SnakeData otherSnake) {
      if (mySnake.Segments.Count < defensiveSegments) {
         return PlayDefensively(gameState, mySnake, otherSnake);
      }
      else {
         List<Vector2Int> path;
         int distance = BotUtilities.CalculateDistance(mySnake.Head, otherSnake.Head);
         SnakeDirection? prefDir = null;
         if (distance <= 5 && distance > 2) {
            if (otherSnake.Head.y - mySnake.Head.y == -1) {
               prefDir = SnakeDirection.Down;
            }
            else if (otherSnake.Head.y - mySnake.Head.y == 1) {
               prefDir = SnakeDirection.Up;
            }
            else if (otherSnake.Head.x - mySnake.Head.x == -1) {
               prefDir = SnakeDirection.Left;
            }
            else if (otherSnake.Head.x - mySnake.Head.x == 1) {
               prefDir = SnakeDirection.Right;
            }
            if (prefDir != null) {
               Vector2Int nextPos = mySnake.Head + BotUtilities.DirectionChange[prefDir.Value];
               if (gameState.IsTileSafe(nextPos)) {
                  return prefDir.Value;
               }
            }
         }
         //if(CanCutOffEnemy(gameState, mySnake, otherSnake)) {
         //   return mySnake.Direction;
         //}
         path = BotUtilities.FindPathToTargetFood_AStar(gameState, mySnake, gameState.FindNearestFood(mySnake));
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
               //var newPath = BotUtilities.FindPathToNearestFood_AStar(gameState, mySnake, new Vector2Int[] { gameState.FindNearestFood(mySnake) });
               //if (newPath != null && newPath.Count > 0)
               //   path = newPath;
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
      //Finde einen Pfad zur nächsten Futterkoordinate
      path = BotUtilities.FindPathToNearestFood_AStar(gameState, mySnake);

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

   private bool CanCutOffEnemy(GameState gameState, SnakeData mySnake, SnakeData otherSnake) {
      GameState tmpState = (GameState)gameState.Clone();
      int i = 1;
      while (tmpState.IsTileSafe(mySnake.Head + BotUtilities.DirectionChange[mySnake.Direction] * i)) {
         SnakeData tmpSnake = tmpState.FindMySnake(mySnake.Owner);
         tmpSnake.Move();
      }
      var tmpPath = BotUtilities.FindPathToNearestFood_AStar(tmpState, tmpState.FindMySnake(mySnake.Owner));

      return tmpPath == null || tmpPath.Count <= 0;
   }
}
