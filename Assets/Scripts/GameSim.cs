﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;
using System.IO;

public class GameSim : MonoBehaviour {
   public ConcurrentQueue<GameLogic> Games = new ConcurrentQueue<GameLogic>();
   public Vector2Int GridSize = new Vector2Int(30, 20);
   public float SimulationStarted = 0f;
   public float SimulationEnded = 0f;
   public int MaxThreads = 6;
   public string BotDirectory = "Assets\\Scripts\\Bots";
   public int GamesToSimulate { get => gamesToSimulate; set => gamesToSimulate = value; }
   public bool SaveHistory { get => saveHistory; set => saveHistory = value; }
   public List<Type> BotTypes = new List<Type>();
   public Type Bot1 { get; set; }
   public Type Bot2 { get; set; }

   private int gamesToSimulate = 1000;
   private bool saveHistory;

   private void Start() {
      if (Directory.Exists(BotDirectory)) {
         foreach (string filePath in Directory.EnumerateFiles(BotDirectory, "*.cs")) {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            Type t = Type.GetType(fileName);
            if (t != null) {
               BotTypes.Add(t);
            }
         }
      }
   }

   private void Update() {
      if (Input.GetKeyDown(KeyCode.F12)) {
         print(Bot1.Name);
         print(Bot2.Name);
      }
   }

   /// <summary>
   /// Simuliert Spiele unter der Verwendung von Unity-Coroutines. Dies ist eine Single-Thread Funktion (langsam).
   /// Sollte nur zum Debuggen verwendet werden!
   /// </summary>
   /// <returns></returns>
   public IEnumerator SimulateGames() {
      SimulationStarted = Time.time;
      ClearGamesQueue();
      Type tBot1 = Bot1;
      Type tBot2 = Bot2;
      while (Games.Count < GamesToSimulate) {
         StartGame(tBot1, tBot2);
         yield return null;
      }
      SimulationEnded = Time.time;
   }

   /// <summary>
   /// Simuliert Spiele unter der Verwendung von C# Async. Dies ist eine Single-Thread Funktion (langsam).
   /// Sollte nur zum Debuggen verwendet werden!
   /// </summary>
   public async void SimulateGames_Async() {
      SimulationStarted = Time.time;
      int numberOfGames = gamesToSimulate;
      ClearGamesQueue();
      Type tBot1 = Bot1;
      Type tBot2 = Bot2;
      for (int i = 0; i < numberOfGames; i++) {
         await Task.Run(() => StartGame(tBot1, tBot2));
      }
      SimulationEnded = Time.time;
   }

   /// <summary>
   /// Simuliert Spiele unter der Verwendung von Hintergrund-Threads. Dies ist eine Multi-Thread Funktion
   /// und ist daher viel schneller auf Prozessoren, die viele Threads besitzen.
   /// </summary>
   public async void SimulateGames_Threaded() {
      SimulationStarted = Time.time;
      int numberOfGames = gamesToSimulate;
      ClearGamesQueue();
      Type tBot1 = Bot1;
      Type tBot2 = Bot2;
      int numberOfThreads = MaxThreads;
      List<Thread> threads = new List<Thread>();
      for (int ts = 0; ts < numberOfThreads; ts++) {
         Thread t = new Thread(new ThreadStart(() => {
            for (int i = 0; i < numberOfGames / numberOfThreads; i++) {
               StartGame(tBot1, tBot2);
            }
         }));
         threads.Add(t);
         t.Start();
      }
      int extraGames = numberOfGames - (numberOfGames / numberOfThreads) * numberOfThreads;
      Thread t2 = new Thread(new ThreadStart(() => {
         for (int i = 0; i < extraGames; i++) {
            StartGame(tBot1, tBot2);
         }
      }));
      threads.Add(t2);
      t2.Start();

      await Task.Run(() => {
         bool stillWorking;
         while (true) {
            stillWorking = false;
            foreach (Thread th in threads) {
               if (th.IsAlive) {
                  stillWorking = true;
                  break;
               }
            }
            if (!stillWorking) {
               break;
            }
         }
      });
      SimulationEnded = Time.time;
   }

   /// <summary>
   /// Simuliert ein Spiel, bis es endet.
   /// </summary>
   /// <param name="tBot1">Der Typ des ersten Bots</param>
   /// <param name="tBot2">Der Typ des zweiten Bots</param>
   private void StartGame(Type tBot1, Type tBot2) {
      if (tBot1 == null || tBot2 == null)
         return;
      SnakeBot bot1 = Activator.CreateInstance(tBot1) as SnakeBot;
      SnakeBot bot2 = Activator.CreateInstance(tBot2) as SnakeBot;
      GameLogic Game = new GameLogic(GridSize, bot1, bot2, SaveHistory);
      Games.Enqueue(Game);
      while (Game.CurrentGameState.IsGameInProgress) {
         Game.NextTick();
      }
   }

   /// <summary>
   /// Leert die ConcurrentQueue der bereits simulierten Spiele
   /// </summary>
   private void ClearGamesQueue() {
      while (Games.Count > 0) {
         if (Games.TryDequeue(out GameLogic item)) { }
      }
   }

}
