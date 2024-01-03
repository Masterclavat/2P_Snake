using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogic {

   public GameState CurrentGameState;

   public int MaxFood { get => maxFood; }
   public Vector2Int GridSize { get => gameGridSize; }
   public SnakeData Winner { get; private set; }
   public string WinnerName { 
      get {
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
      CurrentGameState.IsGameInProgress = true;
   }
   public GameLogic(Vector2Int gridSize, SnakeBot bot1, SnakeBot bot2, bool withMemory = true) : this(gridSize, bot1, bot2, Guid.NewGuid().GetHashCode(), withMemory) { }

   private void SetStartingPositions() {
      //Wähle 2 unterschiedliche, zufällige Quadranten aus
      List<int> quadrants = new List<int>(4) { 0, 1, 2, 3 };
      int randIndex = rand.Next(quadrants.Count);
      int snake1Quad = quadrants[randIndex];
      quadrants.RemoveAt(randIndex);
      int snake2Quad = quadrants[rand.Next(quadrants.Count)];

      //Wähle zufällige Startrichtungen
      CurrentGameState.Snake_1.Direction = (SnakeDirection)rand.Next(Enum.GetValues(typeof(SnakeDirection)).Length);
      CurrentGameState.Snake_2.Direction = (SnakeDirection)rand.Next(Enum.GetValues(typeof(SnakeDirection)).Length);

      //Wähle zufällige Startpositionen
      Vector2Int snake1StartPos = getRandomPositionInQuadrant(snake1Quad);
      Vector2Int snake2StartPos = getRandomPositionInQuadrant(snake2Quad);

      //Weist den Schlangen die Startpositionen zu und fügt 2 weitere Segmente in die Richtung
      //der Startrichtung an
      CurrentGameState.Snake_1.Segments.Add(snake1StartPos);
      CurrentGameState.Snake_1.Segments.Add(snake1StartPos + BotUtilities.DirectionChange[CurrentGameState.Snake_1.Direction]);
      CurrentGameState.Snake_1.Segments.Add(snake1StartPos + BotUtilities.DirectionChange[CurrentGameState.Snake_1.Direction] * 2);

      CurrentGameState.Snake_2.Segments.Add(snake2StartPos);
      CurrentGameState.Snake_2.Segments.Add(snake2StartPos + BotUtilities.DirectionChange[CurrentGameState.Snake_2.Direction]);
      CurrentGameState.Snake_2.Segments.Add(snake2StartPos + BotUtilities.DirectionChange[CurrentGameState.Snake_2.Direction] * 2);
   }

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

      throw new ArgumentException("quadrant must be between 0 and 3");
   }

   public void NextTick() {
      while (CurrentGameState.FoodLocations.Count < MaxFood) {
         SpawnFood();
      }
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
            moveSnake(CurrentGameState.Snake_1);
            moveSnake(CurrentGameState.Snake_2);
            SnakeDied(CurrentGameState.Snake_1);
            SnakeDied(CurrentGameState.Snake_2);
         }
      }

      moveSnake(CurrentGameState.Snake_1);
      moveSnake(CurrentGameState.Snake_2);
      if (Ticks >= maxTicks) {
         if (CurrentGameState.Snake_1.Segments.Count <  CurrentGameState.Snake_2.Segments.Count) {
            SnakeDied(CurrentGameState.Snake_1);
         }else if(CurrentGameState.Snake_2.Segments.Count < CurrentGameState.Snake_1.Segments.Count) {
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

      if(storeMemory)
         Memory.Add((GameState)CurrentGameState.Clone());
      Ticks++;
   }

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

   private void moveSnake(SnakeData snake) {
      if (!snake.IsAlive)
         return;
      Vector2Int NextSeg = snake.Head + BotUtilities.DirectionChange[snake.Direction];
      snake.Segments.RemoveAt(0);
      snake.Segments.Add(NextSeg);
   }

   private bool checkSnakeEatsFood(SnakeData snake, out Vector2Int foodEaten) {
      Vector2Int firstSegment = snake.Head;
      foreach (var food in CurrentGameState.FoodLocations) {
         if (food == firstSegment) {
            snake.Segments.Insert(0, new Vector2Int(food.x, food.y));
            foodEaten = food;
            return true;
         }
      }
      foodEaten = Vector2Int.zero;
      return false;
   }

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
