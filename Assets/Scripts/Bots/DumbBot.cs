using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DumbBot : SnakeBot {

   public override SnakeDirection Tick(GameState gameState, SnakeData mySnake, SnakeData otherSnake) {
      Vector2Int nearestFood = gameState.FindNearestFood(mySnake);
      SnakeDirection? desiredDirection;
      if (nearestFood.y == mySnake.Head.y) {
         if (nearestFood.x < mySnake.Head.x) {
            if (mySnake.Direction != SnakeDirection.Right) {
               desiredDirection = SnakeDirection.Left;
            }
            else {
               desiredDirection = SnakeDirection.Up;
            }
         }
         else {
            if (mySnake.Direction != SnakeDirection.Left) {
               desiredDirection = SnakeDirection.Right;
            }
            else {
               desiredDirection = SnakeDirection.Up;
            }
         }
      }
      else {
         if (nearestFood.y < mySnake.Head.y) {
            if (mySnake.Direction != SnakeDirection.Up) {
               desiredDirection = SnakeDirection.Down;
            }
            else {
               if (nearestFood.x < mySnake.Head.x) {
                  desiredDirection = SnakeDirection.Left;
               }
               else {
                  desiredDirection = SnakeDirection.Right;
               }
            }
         }
         else {
            if (mySnake.Direction != SnakeDirection.Down) {
               desiredDirection = SnakeDirection.Up;
            }
            else {
               if (nearestFood.x < mySnake.Head.x) {
                  desiredDirection = SnakeDirection.Left;
               }
               else {
                  desiredDirection = SnakeDirection.Right;
               }
            }
         }
      }

      if (desiredDirection == null) {
         return mySnake.Direction;
      }
      else {
         if (gameState.IsTileSafe(mySnake.Head + BotUtilities.DirectionChange[desiredDirection.Value])) {
            return desiredDirection.Value;
         }
         else {
            foreach (var dirPair in BotUtilities.DirectionChange) {
               if (gameState.IsTileSafe(mySnake.Head + dirPair.Value))
                  return dirPair.Key;
            }
         }
      }

      return desiredDirection.Value;
   }



}
