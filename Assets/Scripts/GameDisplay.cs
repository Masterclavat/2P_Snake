using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;

public class GameDisplay : MonoBehaviour {
   public GameObject TilePrefab;
   public Transform GridParent;
   public int GridMargin = 3;
   public GameObject[] GridTiles;
   public GameLogic Game;
   public GameSim Simulator;
   public Color NeutralColor;
   public Color FoodColor;
   public Color Snake1Color = Color.cyan;
   public Color Snake2Color = Color.green;
   public TextMeshProUGUI StatusTextMesh;

   [SerializeField]
   private float GameTickRate = 0.1f;
   private int currentMemoryIndex = 0;
   private int currentGameIndex = -1;
   private float lastTick = 0f;
   private bool paused = false;
   bool advanceSingleTick = false;
   private StringBuilder statusSB = new StringBuilder();

   void Start() {
      Vector2Int gridSize = Simulator.GridSize;
      GridTiles = new GameObject[gridSize.x * gridSize.y];
      float tileSize = 30;
      float offsetX = 100f;
      for (int i = 0; i < gridSize.x; i++) {
         for (int j = 0; j < gridSize.y; j++) {
            GameObject tile = Instantiate(TilePrefab, GridParent);
            (tile.transform as RectTransform).localPosition = new Vector3(offsetX + (i - gridSize.x / 2) * tileSize + GridMargin * i, (j - gridSize.y / 2) * tileSize + GridMargin * j, 0);
            GridTiles[i * gridSize.y + j] = tile;
         }
      }
   }

   void Update() {
      if (Input.GetKeyDown(KeyCode.F1)) {
         ShowNextGame();
      }
      else if (Input.GetKeyDown(KeyCode.F2)) {
         ShowNextGame_WinnerSnake_1();
      }
      else if (Input.GetKeyDown(KeyCode.F3)) {
         ShowNextGame_WinnerSnake_2();
      }
      else if (Input.GetKeyDown(KeyCode.F4)) {
         ShowNextGame_Draw();
      }
      else if (Input.GetKeyDown(KeyCode.F5)) {
         ShowNextGame_TickLimit();
      }
      else if (Input.GetKeyDown(KeyCode.F6)) {
         ReplayCurrentGame();
      }
      else if (Input.GetKeyDown(KeyCode.Space)) {
         paused = !paused;
      }
      else if (Input.GetKeyDown(KeyCode.D)) {
         if (Game != null && currentMemoryIndex < Game.Memory.Count) {
            advanceSingleTick = true;
         }
      }
      else if (Input.GetKeyDown(KeyCode.A)) {
         if (Game != null && currentMemoryIndex > 0) {
            currentMemoryIndex-=2;
            advanceSingleTick = true;
         }
      }
      if (Game == null)
         return;
      if (!advanceSingleTick && (Time.time - lastTick < GameTickRate || paused))
         return;
      if (currentMemoryIndex >= Game.Memory.Count)
         currentMemoryIndex = Game.Memory.Count - 1;
      else if (currentMemoryIndex < 0)
         currentMemoryIndex = 0;
      advanceSingleTick = false;
      GameState currentMem = Game.Memory[currentMemoryIndex];
      foreach (GameObject tile in GridTiles) {
         tile.GetComponent<Image>().color = NeutralColor;
         tile.GetComponentInChildren<TextMeshProUGUI>().text = "";
      }

      foreach (Vector2Int segment in currentMem.Snake_1.Segments) {
         if (!currentMem.Snake_1.IsAlive) {
            GridTiles[segment.x * currentMem.GridSize.y + segment.y].GetComponentInChildren<TextMeshProUGUI>().text = "X";
         }
         if (segment == currentMem.Snake_1.Head) {
            float h;
            float s;
            float v;
            Color.RGBToHSV(Snake1Color, out h, out s, out v);
            Color headColor = Color.HSVToRGB(h, s * 0.3f, v);
            GridTiles[segment.x * currentMem.GridSize.y + segment.y].GetComponent<Image>().color = headColor;
         }
         else {
            GridTiles[segment.x * currentMem.GridSize.y + segment.y].GetComponent<Image>().color = Snake1Color;
         }
      }

      foreach (Vector2Int segment in currentMem.Snake_2.Segments) {
         if (!currentMem.Snake_2.IsAlive) {
            GridTiles[segment.x * currentMem.GridSize.y + segment.y].GetComponentInChildren<TextMeshProUGUI>().text = "X";
         }
         if (segment == currentMem.Snake_2.Head) {
            float h;
            float s;
            float v;
            Color.RGBToHSV(Snake2Color, out h, out s, out v);
            Color headColor = Color.HSVToRGB(h, s * 0.3f, v);
            GridTiles[segment.x * currentMem.GridSize.y + segment.y].GetComponent<Image>().color = headColor;
         }
         else {
            GridTiles[segment.x * currentMem.GridSize.y + segment.y].GetComponent<Image>().color = Snake2Color;
         }
      }
      IEnumerable<Vector2Int> l = currentMem.Snake_1.DebugData as IEnumerable<Vector2Int>;
      if (l != null) {
         foreach (Vector2Int pathTile in l) {
            GridTiles[pathTile.x * currentMem.GridSize.y + pathTile.y].GetComponentInChildren<TextMeshProUGUI>().text = "*";
         }
      }

      l = currentMem.Snake_2.DebugData as IEnumerable<Vector2Int>;
      if (l != null) {
         foreach (Vector2Int pathTile in l) {
            GridTiles[pathTile.x * currentMem.GridSize.y + pathTile.y].GetComponentInChildren<TextMeshProUGUI>().text = "*";
         }
      }

      foreach (Vector2Int food in currentMem.FoodLocations) {
         GridTiles[food.x * currentMem.GridSize.y + food.y].GetComponent<Image>().color = FoodColor;
      }
      currentMemoryIndex++;
      lastTick = Time.time;
   }

   private void FixedUpdate() {
      aggregateAndDisplayStatus();
   }

   private void aggregateAndDisplayStatus() {
      int gamesFinished = 0;
      int snake1Win = 0;
      int snake2Win = 0;
      int draw = 0;
      int tickLimit = 0;
      foreach (GameLogic game in Simulator.Games) {
         if (game.CurrentGameState.IsGameInProgress)
            continue;
         gamesFinished++;
         if (game.Ticks >= game.MaxTicks)
            tickLimit++;
         if (game.Winner == game.CurrentGameState.Snake_1)
            snake1Win++;
         else if (game.Winner == game.CurrentGameState.Snake_2)
            snake2Win++;
         else if (game.Winner == null)
            draw++;
      }
      float simTime = Simulator.SimulationEnded - Simulator.SimulationStarted;
      if (simTime < 0f)
         simTime = Time.time - Simulator.SimulationStarted;
      statusSB.AppendLine("<b>Input Guide</b>");
      statusSB.AppendLine("Press F1 for next game");
      statusSB.AppendLine("Press F2 for next game where Snake_1 won");
      statusSB.AppendLine("Press F3 for next game where Snake_2 won");
      statusSB.AppendLine("Press F4 for next game that ended in a draw");
      statusSB.AppendLine("Press F5 for next game that ended by tick limit");
      statusSB.AppendLine("Press F6 to replay the current game");
      statusSB.AppendFormat("Press F11 to simulate {0} games\n", Simulator.GamesToSimulate);
      statusSB.AppendLine();
      if (Simulator.Games.Count > 0) {
         statusSB.AppendLine("<b>Simulation Stats</b>");
         statusSB.AppendFormat("Simulating {0} Games\n", Simulator.GamesToSimulate);
         statusSB.AppendFormat("Games Running: {0}\n", Simulator.Games.Count - gamesFinished);
         statusSB.AppendFormat("Games Finished: {0}/{1} ({2}%)\n", gamesFinished, Simulator.GamesToSimulate, ((float)gamesFinished / Simulator.GamesToSimulate * 100).ToString("N2"));
         statusSB.AppendFormat("Snake1 Won: {0} ({1}%)\n", snake1Win, (snake1Win / (float)gamesFinished * 100).ToString("N2"));
         statusSB.AppendFormat("Snake2 Won: {0} ({1}%)\n", snake2Win, (snake2Win / (float)gamesFinished * 100).ToString("N2"));
         statusSB.AppendFormat("Draws: {0} ({1}%)\n", draw, (draw / (float)gamesFinished * 100).ToString("N2"));
         statusSB.AppendFormat("Ended in Tick Limit: {0} ({1}%)\n", tickLimit, (tickLimit / (float)gamesFinished * 100).ToString("N2"));
         statusSB.AppendFormat("Simulation Time: {0}s\n", (simTime).ToString("N2"));
         statusSB.AppendLine();
      }

      if (currentGameIndex >= 0 && Game != null) {
         statusSB.AppendLine("<b>Game Stats</b>");
         statusSB.AppendFormat("Current Game: {0}\n", currentGameIndex);
         statusSB.AppendFormat("Winner: {0}\n", Game.WinnerName);
         statusSB.AppendFormat("Game Length: {0} ticks\n", Game.Ticks);
         statusSB.AppendFormat("Game Random Seed: {0}\n", Game.RandomSeed);
      }

      StatusTextMesh.text = statusSB.ToString();
      statusSB.Clear();
   }

   private void ShowNextGame() {
      Game = Simulator.Games.ToArray()[++currentGameIndex];
      lastTick = 0f;
      currentMemoryIndex = 0;
   }

   private void ShowNextGame_WinnerSnake_1() {
      GameLogic[] games = Simulator.Games.ToArray();
      for (int i = currentGameIndex + 1; i < Simulator.Games.Count; i++) {
         if (games[i].Winner == games[i].CurrentGameState.Snake_1) {
            Game = games[i];
            currentGameIndex = i;
            currentMemoryIndex = 0;
            lastTick = 0f;
            return;
         }
      }
   }

   private void ShowNextGame_WinnerSnake_2() {
      GameLogic[] games = Simulator.Games.ToArray();
      for (int i = currentGameIndex + 1; i < games.Length; i++) {
         if (games[i].Winner == games[i].CurrentGameState.Snake_2) {
            Game = games[i];
            currentGameIndex = i;
            currentMemoryIndex = 0;
            lastTick = 0f;
            return;
         }
      }
   }

   private void ShowNextGame_Draw() {
      GameLogic[] games = Simulator.Games.ToArray();
      for (int i = currentGameIndex + 1; i < Simulator.Games.Count; i++) {
         if (games[i].Winner == null) {
            Game = games[i];
            currentGameIndex = i;
            currentMemoryIndex = 0;
            lastTick = 0f;
            return;
         }
      }
   }

   private void ShowNextGame_TickLimit() {
      GameLogic[] games = Simulator.Games.ToArray();
      for (int i = currentGameIndex + 1; i < Simulator.Games.Count; i++) {
         if (games[i].Ticks >= games[i].MaxTicks) {
            Game = games[i];
            currentGameIndex = i;
            currentMemoryIndex = 0;
            lastTick = 0f;
            return;
         }
      }
   }

   private void ReplayCurrentGame() {
      GameLogic[] games = Simulator.Games.ToArray();
      if (currentGameIndex > 0) {
         Game = games[currentGameIndex];
         lastTick = 0f;
         currentMemoryIndex = 0;
      }
   }
}
