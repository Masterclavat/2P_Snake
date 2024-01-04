using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BetterBot : SnakeBot {

   public override SnakeDirection Tick(GameState gameState, SnakeData mySnake, SnakeData otherSnake) {
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
}
