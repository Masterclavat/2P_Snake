using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Die Repr�sentation einer einzelnen Spielrunde
/// </summary>
public class GameLogic {

   public GameState CurrentGameState;

   public int MaxFood { get => maxFood; }
   public Vector2Int GridSize { get => gameGridSize; }
   public SnakeData Winner { get; private set; }
   public string WinnerName
   {
      get
      {
         if (Winner == null)
            return "Draw";
         else if (Winner == CurrentGameState.Snake_1)
            return "Snake 1";
         else if (Winner == CurrentGameState.Snake_2)
            return "Snake 2";
         else
            return "Unknown";
      }
   }
   public List<GameState> Memory = new List<GameState>();
   public int Ticks { get; private set; }
   public int MaxTicks { get => maxTicks; }
   public int RandomSeed { get; }

   [SerializeField]
   private int maxFood = 3;
   private Vector2Int gameGridSize;
   private bool storeMemory;
   private int maxTicks = 1000;
   System.Random rand;

   public GameLogic(Vector2Int gridSize, SnakeBot bot1, SnakeBot bot2, int randomSeed, bool withMemory = true) {
      storeMemory = withMemory;
      RandomSeed = randomSeed;
      rand = new System.Random(RandomSeed);
      gameGridSize = gridSize;

      CurrentGameState = new GameState();
      CurrentGameState.Snake_1 = new SnakeData(bot1, Color.cyan);
      CurrentGameState.Snake_2 = new SnakeData(bot2, Color.green);
      SetStartingPositions();
      while (CurrentGameState.FoodLocations.Count < MaxFood) {
         SpawnFood();
      }
      CurrentGameState.IsGameInProgress = true;
   }
   public GameLogic(Vector2Int gridSize, SnakeBot bot1, SnakeBot bot2, bool withMemory = true) : this(gridSize, bot1, bot2, Guid.NewGuid().GetHashCode(), withMemory) { }

   /// <summary>
   /// Setzt die Startpositionen beider Schlangen auf eine zuf�llige Koordinate in zuf�lligen Quadranten.
   /// Die Schlangen spawnen immer in unterschiedlichen Quadranten.
   /// </summary>
   private void SetStartingPositions() {
      //W�hle 2 unterschiedliche, zuf�llige Quadranten aus
      List<int> quadrants = new List<int>(4) { 0, 1, 2, 3 };
      int randIndex = rand.Next(quadrants.Count);
      int snake1Quad = quadrants[randIndex];
      quadrants.RemoveAt(randIndex);
      int snake2Quad = quadrants[rand.Next(quadrants.Count)];

      //W�hle zuf�llige Startrichtungen
      CurrentGameState.Snake_1.Direction = (SnakeDirection)rand.Next(Enum.GetValues(typeof(SnakeDirection)).Length);
      CurrentGameState.Snake_2.Direction = (SnakeDirection)rand.Next(Enum.GetValues(typeof(SnakeDirection)).Length);

      //W�hle zuf�llige Startpositionen
      Vector2Int snake1StartPos = getRandomPositionInQuadrant(snake1Quad);
      Vector2Int snake2StartPos = getRandomPositionInQuadrant(snake2Quad);

      //Weist den Schlangen die Startpositionen zu und f�gt 2 weitere Segmente in die Richtung
      //der Startrichtung an
      CurrentGameState.Snake_1.Segments.Add(snake1StartPos);
      CurrentGameState.Snake_1.Segments.Add(snake1StartPos + BotUtilities.DirectionChange[CurrentGameState.Snake_1.Direction]);
      CurrentGameState.Snake_1.Segments.Add(snake1StartPos + BotUtilities.DirectionChange[CurrentGameState.Snake_1.Direction] * 2);

      CurrentGameState.Snake_2.Segments.Add(snake2StartPos);
      CurrentGameState.Snake_2.Segments.Add(snake2StartPos + BotUtilities.DirectionChange[CurrentGameState.Snake_2.Direction]);
      CurrentGameState.Snake_2.Segments.Add(snake2StartPos + BotUtilities.DirectionChange[CurrentGameState.Snake_2.Direction] * 2);
   }

   /// <summary>
   /// Berechnet eine zuf�llige Koordinate in einem bestimmten Quadranten
   /// </summary>
   /// <param name="quadrant">Der Quadrant, in dem die zuf�llige Koordinate liegen soll</param>
   /// <returns>Eine zuf�llige Koordinate aus dem Quadranten in <paramref name="quadrant"/></returns>
   private Vector2Int getRandomPositionInQuadrant(int quadrant) {
      //Die Quadranten sind die folgenden:
      //|1|2|
      //|0|3|
      switch (quadrant) {
         case 0:
            return new Vector2Int(rand.Next(3, gameGridSize.x / 2 - 3), rand.Next(3, gameGridSize.y / 2 - 3));
         case 1:
            return new Vector2Int(rand.Next(3, gameGridSize.x / 2 - 3), rand.Next(gameGridSize.y / 2 + 3, gameGridSize.y - 3));
         case 2:
            return new Vector2Int(rand.Next(gameGridSize.x / 2 + 3, gameGridSize.x - 3), rand.Next(gameGridSize.y / 2 + 3, gameGridSize.y - 3));
         case 3:
            return new Vector2Int(rand.Next(gameGridSize.x / 2 + 3, gameGridSize.x - 3), rand.Next(3, gameGridSize.y / 2 - 3));
      }

      throw new ArgumentException("Quadrant must be between 0 and 3", "quadrant");
   }

   /// <summary>
   /// Das Herzst�ck der GameLogic, f�hrt einen Tick aus.
   /// Ein Tick besteht aus der Abfrage der n�chsten Aktion beider Bots,
   /// der �berpr�fung, ob dadurch das Spiel endet.
   /// Wenn nicht, bewegt beider Schlangen in ihre jeweils gew�hlten Richtungen.
   /// Dann werden neue Futterkoordinaten festgelegt, falls es weniger als die maximale Anzahl sind und
   /// speichert den derzeitigen GameState in der Replay-History.
   /// </summary>
   public void NextTick() {
      SnakeDirection prefDirection = CurrentGameState.Snake_1.Owner.Tick(CurrentGameState, CurrentGameState.Snake_1, CurrentGameState.Snake_2);
      CurrentGameState.Snake_1.Direction = isDirectionChangeAllowed(CurrentGameState.Snake_1.Direction, prefDirection) ?
                                                                     prefDirection : CurrentGameState.Snake_1.Direction;

      prefDirection = CurrentGameState.Snake_2.Owner.Tick(CurrentGameState, CurrentGameState.Snake_2, CurrentGameState.Snake_1);
      CurrentGameState.Snake_2.Direction = isDirectionChangeAllowed(CurrentGameState.Snake_2.Direction, prefDirection) ?
                                                                     prefDirection : CurrentGameState.Snake_2.Direction;
      if (!CurrentGameState.IsTileSafe(CurrentGameState.Snake_1.NextHeadPosition)) {
         SnakeDied(CurrentGameState.Snake_1);
      }
      if (!CurrentGameState.IsTileSafe(CurrentGameState.Snake_2.NextHeadPosition)) {
         SnakeDied(CurrentGameState.Snake_2);
      }
      if (CurrentGameState.Snake_1.IsAlive && CurrentGameState.Snake_2.IsAlive) {
         if (CurrentGameState.Snake_1.NextHeadPosition == CurrentGameState.Snake_2.NextHeadPosition) {
            CurrentGameState.Snake_1.Move();
            CurrentGameState.Snake_2.Move();
            SnakeDied(CurrentGameState.Snake_1);
            SnakeDied(CurrentGameState.Snake_2);
         }
      }

      CurrentGameState.Snake_1.Move();
      CurrentGameState.Snake_2.Move();
      if (Ticks >= maxTicks) {
         if (CurrentGameState.Snake_1.Segments.Count < CurrentGameState.Snake_2.Segments.Count) {
            SnakeDied(CurrentGameState.Snake_1);
         }
         else if (CurrentGameState.Snake_2.Segments.Count < CurrentGameState.Snake_1.Segments.Count) {
            SnakeDied(CurrentGameState.Snake_2);
         }
         else {
            SnakeDied(CurrentGameState.Snake_1);
            SnakeDied(CurrentGameState.Snake_2);
         }
      }

      Vector2Int foodEaten;
      if (checkSnakeEatsFood(CurrentGameState.Snake_1, out foodEaten)) {
         CurrentGameState.FoodLocations.Remove(foodEaten);
      }
      if (checkSnakeEatsFood(CurrentGameState.Snake_2, out foodEaten)) {
         CurrentGameState.FoodLocations.Remove(foodEaten);
      }

      while (CurrentGameState.FoodLocations.Count < MaxFood) {
         SpawnFood();
      }

      if (storeMemory)
         Memory.Add((GameState)CurrentGameState.Clone());
      Ticks++;
   }

   /// <summary>
   /// Legt eine zuf�llige freie Koordinate als Futterkoordinate fest
   /// </summary>
   private void SpawnFood() {
      List<Vector2Int> allSegments = new List<Vector2Int>();
      foreach (var seg in CurrentGameState.Snake_1.Segments) {
         allSegments.Add(seg);
      }
      foreach (var seg in CurrentGameState.Snake_2.Segments) {
         allSegments.Add(seg);
      }
      foreach (var food in CurrentGameState.FoodLocations) {
         allSegments.Add(food);
      }

      if (allSegments.Count < gameGridSize.x * gameGridSize.y) {
         Vector2Int foodLoc;
         do {
            foodLoc = new Vector2Int(rand.Next(GridSize.x), rand.Next(GridSize.y));
         } while (allSegments.Contains(foodLoc));
         CurrentGameState.FoodLocations.Add(foodLoc);
      }
   }

   /// <summary>
   /// Wird aufgerufen, wenn eine Schlange stirbt und legt als Gewinner dieser Runde die gegnerische Schlange fest.
   /// Wenn die gegnerische Schlange im selben Tick auch gestorben ist, legt das Ergebnis dieser Runde auf unentschieden fest.
   /// </summary>
   /// <param name="snake">Die Schlange, die stirbt</param>
   private void SnakeDied(SnakeData snake) {
      snake.IsAlive = false;
      CurrentGameState.IsGameInProgress = false;
      if (Winner == null) {
         if (snake == CurrentGameState.Snake_1) {
            Winner = CurrentGameState.Snake_2;
         }
         else {
            Winner = CurrentGameState.Snake_1;
         }
      }
      else {
         Winner = null;
      }
   }

   /// <summary>
   /// Pr�ft, ob der Kopf der in <paramref name="snake"/> angegebenen Schlange auf einer Futterkoordinate liegt
   /// und gibt das Ergebnis zur�ck.
   /// </summary>
   /// <param name="snake">Die Schlange, dessen Kopf �berpr�ft werden soll</param>
   /// <param name="foodEaten">Wenn das Ergebnis der Funktion true ist, liegt in diesem Parameter die Futterkoordinate</param>
   /// <returns>Gibt <c>true</c> zur�ck, wenn der Kopf der Schlange auf einer Futterkoordinate liegt,
   /// andernfalls <c>false</c>.</returns>
   private bool checkSnakeEatsFood(SnakeData snake, out Vector2Int foodEaten) {
      foreach (var food in CurrentGameState.FoodLocations) {
         if (food == snake.Head) {
            snake.Segments.Insert(0, new Vector2Int(food.x, food.y));
            foodEaten = food;
            return true;
         }
      }
      foodEaten = Vector2Int.zero;
      return false;
   }

   /// <summary>
   /// Pr�ft, ob ein Richtungswechsel erlaubt ist.
   /// </summary>
   /// <param name="current">Die derzeitige Richtung</param>
   /// <param name="next">Die Richtung, in die gewechselt werden soll</param>
   /// <returns>Gibt <c>false</c> zur�ck, wenn <paramref name="next"/> genau die gegen�berliegende Richtung von 
   /// <paramref name="current"/> ist. Andernfalls gibt <c>true</c> zur�ck. </returns>
   private bool isDirectionChangeAllowed(SnakeDirection current, SnakeDirection next) {
      if (current == SnakeDirection.Down && next == SnakeDirection.Up)
         return false;
      if (current == SnakeDirection.Up && next == SnakeDirection.Down)
         return false;
      if (current == SnakeDirection.Left && next == SnakeDirection.Right)
         return false;
      if (current == SnakeDirection.Right && next == SnakeDirection.Left)
         return false;

      return true;
   }
}
