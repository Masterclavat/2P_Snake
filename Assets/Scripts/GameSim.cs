using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;

public class GameSim : MonoBehaviour {
   public ConcurrentQueue<GameLogic> Games = new ConcurrentQueue<GameLogic>();
   public Vector2Int GridSize = new Vector2Int(30, 20);
   public float SimulationStarted = 0f;
   public float SimulationEnded = 0f;
   public int MaxThreads = 6;

   public int GamesToSimulate { get; private set; }
   public bool SaveHistory { get; private set; }

   private void Start() {
      GamesToSimulate = 10000;
      SaveHistory = false;
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
   }

   private IEnumerator SimulateGames(int numberOfGames) {
      SimulationStarted = Time.time;
      GamesToSimulate = numberOfGames;
      ClearGamesQueue();
      while (Games.Count < GamesToSimulate) {
         StartGame();
         yield return null;
      }
      SimulationEnded = Time.time;
   }

   private async void SimulateGames_Async(int numberOfGames) {
      SimulationStarted = Time.time;
      GamesToSimulate = numberOfGames;
      ClearGamesQueue();
      for (int i = 0; i < numberOfGames; i++) {
         await Task.Run(StartGame);
      }
      SimulationEnded = Time.time;
   }

   private async void SimulateGames_Threaded(int numberOfGames) {
      SimulationStarted = Time.time;
      GamesToSimulate = numberOfGames;
      ClearGamesQueue();
      int numberOfThreads = MaxThreads;
      List<Thread> threads = new List<Thread>();
      for (int ts = 0; ts < numberOfThreads; ts++) {
         Thread t = new Thread(new ThreadStart(() => {
            for (int i = 0; i < numberOfGames / numberOfThreads; i++) {
               StartGameThreadSafe();
            }
         }));
         threads.Add(t);
         t.Start();
      }
      int extraGames = numberOfGames - (numberOfGames / numberOfThreads) * numberOfThreads;
      Thread t2 = new Thread(new ThreadStart(() => {
         for (int i = 0; i < extraGames; i++) {
            StartGameThreadSafe();
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
      SnakeBot bot1 = new DumbBot();
      SnakeBot bot2 = new AggroBot();
      GameLogic Game = new GameLogic(GridSize, bot1, bot2, SaveHistory);
      Games.Enqueue(Game);
      while (Game.CurrentGameState.IsGameInProgress) {
         Game.NextTick();
      }
   }

   private void StartGameThreadSafe() {
      SnakeBot bot1 = new DumbBot();
      SnakeBot bot2 = new AggroBot();

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
