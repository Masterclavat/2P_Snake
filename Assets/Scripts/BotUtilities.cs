﻿using System;
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
   /// Berechnet den schnellsten Weg vom Kopf einer Schlange zur nächsten Futterkoordinate. Dafür wird der A* Algorithmus verwendet.
   /// </summary>
   /// <param name="gameState">Der derzeitige Game State</param>
   /// <param name="mySnake">Die Schlange, aus dessen Sicht der Weg berechnet wird</param>
   /// <param name="ignoreFood">Wenn in diesem Parameter eine Futterkoordnate übergeben wird, wird diese Koordinate 
   /// bei der Pfadsuche ignoriert und sich auf die anderen Futterkoordinaten konzentriert.
   /// Wenn dieser Parameter null ist, werden alle Futterkoordinaten in betracht gezogen.</param>
   /// <returns>Gibt eine Liste von Koordinaten zurück, die den Weg beschreiben. Startet vom Kopf der Schlange
   /// und führt bis zum nächsten Futter.
   /// Wenn es keinen gültigen Weg gibt, wird eine leere Liste zurückgegeben</returns>
   public static List<Vector2Int> FindPathToNearestFood(GameState gameState, SnakeData mySnake, IEnumerable<Vector2Int> ignoreFood = null) {
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

      return FindPathToNearestTargetLocation(gameState, mySnake, validFoodTargets);
   }

   /// <summary>
   /// Berechnet den schnellsten Weg vom Kopf einer Schlange zu einer bestimmten Futterkoordinate. Dazu wird der A* Algorithmus verwendet.
   /// </summary>
   /// <param name="gameState">Der derzeitige Game State</param>
   /// <param name="mySnake">Die Schlange, aus dessen Sicht der Weg berechnet wird</param>
   /// <param name="targetFood">Die Futterkoordinate, zu der der schnellste Weg berechnet werden soll</param>
   /// <returns>Gibt eine Liste von Koordinaten zurück, die den Weg beschreiben. Startet vom Kopf der Schlange
   /// und führt bis zur Koordinate in targetFood.
   /// Wenn es keinen gültigen Weg gibt, wird eine leere Liste zurückgegeben</returns>
   public static List<Vector2Int> FindPathToTargetFood(GameState gameState, SnakeData mySnake, Vector2Int targetFood) {
      List<Vector2Int> ignoreFood = new List<Vector2Int>();
      foreach (Vector2Int foodLoc in gameState.FoodLocations) {
         if (foodLoc != targetFood)
            ignoreFood.Add(foodLoc);
      }

      return FindPathToNearestFood(gameState, mySnake, ignoreFood);
   }

   /// <summary>
   /// Berechnet den schnellsten Weg vom Kopf einer Schlange zu einer bestimmten Koordinate.  Dazu wird der A* Algorithmus verwendet.
   /// </summary>
   /// <param name="gameState">Der derzeitige Game State</param>
   /// <param name="mySnake">Die Schlange, aus dessen Sicht der Weg berechnet wird</param>
   /// <param name="targetLocation">Die Koordinate, zu der der schnellste Weg berechnet werden soll</param>
   /// <param name="ignore">Eine Auflistung von Koordinaten, die nicht Teil des Wegs sein dürfen.
   /// Wenn dieser Parameter null ist, gibt es keine Restriktionen.</param>
   /// <returns>Gibt eine Liste von Koordinaten zurück, die den Weg beschreiben. Startet vom Kopf der Schlange
   /// und führt bis zur Koordinate in targetLocation.
   /// Wenn es keinen gültigen Weg gibt, wird eine leere Liste zurückgegeben</returns>
   public static List<Vector2Int> FindPathToTargetLocation(GameState gameState, SnakeData mySnake, Vector2Int targetLocation, IEnumerable<Vector2Int> ignore = null) {
      return FindPathToNearestTargetLocation(gameState, mySnake, new Vector2Int[] { targetLocation }, ignore);
   }

   /// <summary>
   /// Berechnet den schnellsten Weg vom Kopf einer Schlange zu einer bestimmten Koordinate. Dazu wird der A* Algorithmus verwendet.
   /// </summary>
   /// <param name="gameState">Der derzeitige Game State</param>
   /// <param name="mySnake">Die Schlange, aus dessen Sicht der Weg berechnet wird</param>
   /// <param name="targetLocations">Eine Auflistung von Koordinaten, zu denen der schnellste Weg berechnet werden soll</param>
   /// <param name="ignore">Eine Auflistung von Koordinaten, die nicht Teil des Wegs sein dürfen.
   /// Wenn dieser Parameter null ist, gibt es keine Restriktionen.</param>
   /// <returns>Gibt eine Liste von Koordinaten zurück, die den Weg beschreiben. Startet vom Kopf der Schlange
   /// und führt bis zur nächsten Koordinate in targetLocations.
   /// Wenn es keinen gültigen Weg gibt, wird eine leere Liste zurückgegeben</returns>
   public static List<Vector2Int> FindPathToNearestTargetLocation(GameState gameState, SnakeData mySnake, IEnumerable<Vector2Int> targetLocations, IEnumerable<Vector2Int> ignore = null) {
      return FindNearestPathFromPointToPoint(gameState, mySnake.Head, targetLocations, ignore);
   }

   /// <summary>
   /// Berechnet den schnellsten Weg von einer bestimmten Koordinate zu einer anderen Koordinate. Dazu wird der A* Algorithmus verwendet.
   /// </summary>
   /// <param name="gameState">Der derzeitige Game State</param>
   /// <param name="startPoint">Die Koordinate, von der aus der Weg berechnet werden soll.</param>
   /// <param name="targetLocations">Eine Auflistung von Koordinaten, zu denen der schnellste Weg berechnet werden soll</param>
   /// <param name="ignore">Eine Auflistung von Koordinaten, die nicht Teil des Wegs sein dürfen.
   /// Wenn dieser Parameter null ist, gibt es keine Restriktionen.</param>
   /// <returns>Gibt eine Liste von Koordinaten zurück, die den Weg beschreiben. Startet vom Kopf der Schlange
   /// und führt bis zur nächsten Koordinate in targetLocations.
   /// Wenn es keinen gültigen Weg gibt, wird eine leere Liste zurückgegeben</returns>
   public static List<Vector2Int> FindNearestPathFromPointToPoint(GameState gameState, Vector2Int startPoint, IEnumerable<Vector2Int> targetLocations, IEnumerable<Vector2Int> ignore = null) {
      if (ignore == null)
         ignore = new Vector2Int[] { };
      List<Vector2Int> open = new List<Vector2Int>();
      List<Vector2Int> closed = new List<Vector2Int>();
      Dictionary<Vector2Int, Vector2Int> parents = new Dictionary<Vector2Int, Vector2Int>();
      open.Add(startPoint);

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
                  if (finalPath.Contains(parents[curr]))
                     return new List<Vector2Int>();
                  finalPath.Insert(0, curr);
                  curr = parents[curr];
               }

               return finalPath;
            }
            if (gameState.IsTileSafe(next) && !closed.Contains(next)) {
               if (ignore.Contains(next)) {
                  //remove pls
                  Debug.Log("illegal");
               }
               else {
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

   /// <summary>
   /// Prüft, ob der Kopf einer Schlange in der Nähe des Spielfeldrandes ist. Der Abstand, der in Betracht gezogen wird,
   /// kann mit dem Parameter "maxDistance" beeinflusst werden.
   /// </summary>
   /// <param name="gameState">Der derzeitige Game State</param>
   /// <param name="targetSnake">Die Schlange, die geprüft werden soll</param>
   /// <param name="maxDistance">Die Entfernung vom Spielfeldrand, die als "in der Nähe" gelten soll</param>
   /// <returns>True, wenn der Kopf der Schlange höchstens so weit vom Spielfeldrand entfernt ist, wie in "maxDistance" angegeben
   /// Ansonsten False</returns>
   public static bool IsSnakeNearABorder(GameState gameState, SnakeData targetSnake, int maxDistance = 1) {
      return IsLocationNearABorder(gameState, targetSnake.Head, maxDistance);
   }

   /// <summary>
   /// Prüft, ob eine Koordinate in der Nähe des Spielfeldrandes ist. Der Abstand, der in Betracht gezogen wird,
   /// kann mit dem Parameter "maxDistance" beeinflusst werden.
   /// </summary>
   /// <param name="gameState">Der derzeitige Game State</param>
   /// <param name="location">Die Koordinate, die geprüft werden soll</param>
   /// <param name="maxDistance">Die Entfernung vom Spielfeldrand, die als "in der Nähe" gelten soll</param>
   /// <returns>True, wenn die Koordinate höchstens so weit vom Spielfeldrand entfernt ist, wie in "maxDistance" angegeben
   /// Ansonsten False</returns>
   public static bool IsLocationNearABorder(GameState gameState, Vector2Int location, int maxDistance = 1) {
      return location.x <= maxDistance || location.x >= gameState.GridSize.x - maxDistance - 1 ||
             location.y <= maxDistance || location.y >= gameState.GridSize.y - maxDistance - 1;
   }

   /// <summary>
   /// Gibt die Nachbarkoordinaten einer Koordinate zurück. Diagonale Nachbarn werden dabei nicht berücksichtigt.
   /// </summary>
   /// <param name="targetLocation">Die Koordinate, dessen Nachbarn gefunden werden sollen</param>
   /// <returns>Ein Array von Koordinaten, die den Nachbarkoordinaten von "targetLocation" sind.</returns>
   public static Vector2Int[] GetAdjacentLocations(Vector2Int targetLocation) {
      List<Vector2Int> adjLocations = new List<Vector2Int>();
      foreach (Vector2Int loc in DirectionChange.Values) {
         adjLocations.Add(loc + targetLocation);
      }

      return adjLocations.ToArray();
   }

}
