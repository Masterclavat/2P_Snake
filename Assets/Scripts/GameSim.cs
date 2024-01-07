using System;
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
   public int GamesToSimulate { get => gamesToSimulate; }
   public bool SaveHistory { get; private set; }
   public List<Type> BotTypes = new List<Type>();
   public Type Bot1 { get; set; }
   public Type Bot2 { get; set; }

   private int gamesToSimulate = 1000;

   private void Start() {
      gamesToSimulate = 1000;
      SaveHistory = true;
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
      if (Input.GetKeyDown(KeyCode.F9)) {
         StartCoroutine(SimulateGames(GamesToSimulate));
      }
      else if (Input.GetKeyDown(KeyCode.F10)) {
         SimulateGames_Async(GamesToSimulate);
      }
      else if (Input.GetKeyDown(KeyCode.F11)) {
         SimulateGames_Threaded(GamesToSimulate);
      }
      if (Input.GetKeyDown(KeyCode.F12)) {
         print(Bot1.Name);
         print(Bot2.Name);
      }
   }

   private IEnumerator SimulateGames(int numberOfGames) {
      SimulationStarted = Time.time;
      gamesToSimulate = numberOfGames;
      ClearGamesQueue();
      while (Games.Count < GamesToSimulate) {
         StartGame();
         yield return null;
      }
      SimulationEnded = Time.time;
   }

   private async void SimulateGames_Async(int numberOfGames) {
      SimulationStarted = Time.time;
      gamesToSimulate = numberOfGames;
      ClearGamesQueue();
      for (int i = 0; i < numberOfGames; i++) {
         await Task.Run(StartGame);
      }
      SimulationEnded = Time.time;
   }

   private async void SimulateGames_Threaded(int numberOfGames) {
      SimulationStarted = Time.time;
      gamesToSimulate = numberOfGames;
      ClearGamesQueue();
      int numberOfThreads = MaxThreads;
      List<Thread> threads = new List<Thread>();
      for (int ts = 0; ts < numberOfThreads; ts++) {
         Thread t = new Thread(new ThreadStart(() => {
            for (int i = 0; i < numberOfGames / numberOfThreads; i++) {
               StartGame();
            }
         }));
         threads.Add(t);
         t.Start();
      }
      int extraGames = numberOfGames - (numberOfGames / numberOfThreads) * numberOfThreads;
      Thread t2 = new Thread(new ThreadStart(() => {
         for (int i = 0; i < extraGames; i++) {
            StartGame();
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

   private void StartGame() {
      if (Bot1 == null || Bot2 == null)
         return;
      SnakeBot bot1 = Activator.CreateInstance(Bot1) as SnakeBot;
      SnakeBot bot2 = Activator.CreateInstance(Bot2) as SnakeBot;
      GameLogic Game = new GameLogic(GridSize, bot1, bot2, SaveHistory);
      Games.Enqueue(Game);
      while (Game.CurrentGameState.IsGameInProgress) {
         Game.NextTick();
      }
   }

   private void ClearGamesQueue() {
      while (Games.Count > 0) {
         if (Games.TryDequeue(out GameLogic item)) {

         }
      }
   }

}
