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
      if (lastPath == null || lastPath.Count == 0 || !gameState.FoodLocations.Contains(lastPath.Last())) {
         path = BotUtilities.FindPathToNearestFood_AStar(gameState, mySnake);
         lastPath = path;
      }
      else {
         for(int i = 1; i < lastPath.Count; i++) {
            if (!gameState.IsTileSafe(lastPath[i])) {
               path = BotUtilities.FindPathToNearestFood_AStar(gameState, mySnake);
               lastPath = path;
               break;
            }
         }
         if(path == null) {
            path = lastPath;
            path.RemoveAt(0);
         }
      }
      mySnake.DebugData = path.ToArray();
      if (path.Count > 0) {
         if (BotUtilities.CalculateDistance(otherSnake.Head, path[0]) == 1) {
            foreach (var dir in BotUtilities.DirectionChange.Keys) {
               Vector2Int next = BotUtilities.DirectionChange[dir] + mySnake.Head;
               if (next != path[0] && gameState.IsTileSafe(next)) {
                  return dir;
               }
            }
         }
         foreach (var dirChange in BotUtilities.DirectionChange) {
            if (dirChange.Value == path[0] - mySnake.Head)
               return dirChange.Key;
         }
      }
      return mySnake.Direction;
   }
}
