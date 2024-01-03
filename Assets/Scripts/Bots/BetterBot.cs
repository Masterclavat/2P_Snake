using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BetterBot : SnakeBot {
   List<Vector2Int> lastPath;

   public override SnakeDirection Tick(GameState gameState, SnakeData mySnake, SnakeData otherSnake) {
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
         for(int i = 1; i < lastPath.Count; i++) {
            if (!gameState.IsTileSafe(lastPath[i])) {
               path = BotUtilities.FindPathToNearestFood_AStar(gameState, mySnake);
               lastPath = path;
               break;
            }
         }
         //Wenn der alte Pfad noch gültig ist, verwende ihn als derzeitigen Pfad
         if(path == null) {
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
