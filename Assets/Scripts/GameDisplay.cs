using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Text;
using System;

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
   public TMP_Dropdown[] SnakeBotSelects = new TMP_Dropdown[2];
   public PercentageGraph Bot1Graph;
   public PercentageGraph Bot2Graph;
   public PercentageGraph DrawGraph;
   public GameObject GraphPanel;
   public GameObject ReplayControlGroup;
   public Toggle GraphEnableCheckbox;

   public float GameTickRate = 0.1f;
   private int currentMemoryIndex = 0;
   private int currentGameIndex = -1;
   private float lastTick = 0f;
   public bool Paused;
   bool advanceSingleTick = false;
   private StringBuilder statusSB = new StringBuilder();
   private int lastNumberOfCompletedGames = 0;
   private float lastSimulationStarted = 0f;

   void Start() {
      Vector2Int gridSize = Simulator.GridSize;
      GridTiles = new GameObject[gridSize.x * gridSize.y];
      Vector2 tileSize = (TilePrefab.transform as RectTransform).rect.size;
      float offsetX = 100f;
      float offsetY = (GridParent.GetComponentInParent<CanvasScaler>().referenceResolution.y - (GridMargin * (gridSize.y + 1) + tileSize.y * gridSize.y)) / 2;
      for (int i = 0; i < gridSize.x; i++) {
         for (int j = 0; j < gridSize.y; j++) {
            GameObject tile = Instantiate(TilePrefab, GridParent);
            (tile.transform as RectTransform).anchoredPosition = new Vector3(offsetX + (i - gridSize.x / 2) * tileSize.x + GridMargin * i, j * tileSize.y + GridMargin * (j + 1) + offsetY, 0);
            tile.name = string.Format("Tile ({0}/{1})", i, j);
            GridTiles[i * gridSize.y + j] = tile;
         }
      }
      List<string> botNames = new List<string>(Simulator.BotTypes.Select(x => x.Name));

      SnakeBotSelects[0].onValueChanged.AddListener((i) => {
         Simulator.Bot1 = Simulator.BotTypes.First(x => x.Name == SnakeBotSelects[0].options[i].text);
         SaveDropdownState();
         Bot1Graph.Label = string.Format("Bot 1 ({0})", Simulator.Bot1.Name);
      });
      SnakeBotSelects[1].onValueChanged.AddListener((i) => {
         Simulator.Bot2 = Simulator.BotTypes.First(x => x.Name == SnakeBotSelects[1].options[i].text);
         SaveDropdownState();
         Bot2Graph.Label = string.Format("Bot 2 ({0})", Simulator.Bot2.Name);
      });
      string[] saved = null;
      if (System.IO.File.Exists("UserSettings\\botselect.txt")) {
         saved = System.IO.File.ReadAllLines("UserSettings\\botselect.txt");

      }

      for (int i = 0; i < SnakeBotSelects.Length; i++) {
         TMP_Dropdown select = SnakeBotSelects[i];
         if (select == null)
            continue;
         select.ClearOptions();
         select.AddOptions(botNames);
         int index = 0;
         if (saved != null) {
            for (index = 0; index < select.options.Count; index++) {
               if (select.options[index].text == saved[i])
                  break;
            }
         }
         select.value = index;
         select.onValueChanged.Invoke(index);
      }

      DrawGraph.Label = "Draw %";
   }

   void Update() {
      if (Input.GetKeyDown(KeyCode.D)) {
         SingleTickAdvance();
      }
      else if (Input.GetKeyDown(KeyCode.A)) {
         SingleTickBack();
      }
      if (lastSimulationStarted != Simulator.SimulationStarted) {
         lastSimulationStarted = Simulator.SimulationStarted;
         currentGameIndex = -1;
      }

      ReplayControlGroup.SetActive(!Simulator.Games.IsEmpty && Simulator.Games.First().Memory.Count > 0);
      if (Game == null)
         return;
      if (!advanceSingleTick && (Time.time - lastTick < GameTickRate || Paused))
         return;
      if (currentMemoryIndex >= Game.Memory.Count) {
         currentMemoryIndex = Game.Memory.Count - 1;
         Paused = true;
      }
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
            Color.RGBToHSV(Snake1Color, out float h, out float s, out float v);
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
            Color.RGBToHSV(Snake2Color, out float h, out float s, out float v);
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
      if (GraphPanel != null)
         GraphPanel.SetActive(GraphEnableCheckbox.isOn && Simulator.SimulationEnded == 0f && Simulator.SimulationStarted > 0f);
   }

   /// <summary>
   /// R�ckt das Replay um einen Schritt vor
   /// </summary>
   public void SingleTickAdvance() {
      if (Game != null && currentMemoryIndex < Game.Memory.Count) {
         advanceSingleTick = true;
         Paused = true;
      }
   }

   /// <summary>
   /// R�ckt das Replay um einen Schritt zur�ck
   /// </summary>
   public void SingleTickBack() {
      if (Game != null && currentMemoryIndex > 0) {
         currentMemoryIndex -= 2;
         advanceSingleTick = true;
         Paused = true;
      }
   }

   /// <summary>
   /// F�gt den Graphen einen weiteren Datenpunkt hinzu
   /// </summary>
   /// <param name="bot1Wins">Anzahl der Siege des ersten Bots</param>
   /// <param name="bot2Wins">Anzahl der Siege des zweiten Bots</param>
   /// <param name="draws">Anzahl der Spiele, die unentschieden geendet sind</param>
   /// <param name="total">Anzahl aller Spiele</param>
   private void drawGraph(int bot1Wins, int bot2Wins, int draws, int total) {
      if (Simulator.GamesToSimulate == 0 || total == 0)
         return;
      Bot1Graph.AddDataPoint(bot1Wins / (float)total);
      Bot2Graph.AddDataPoint(bot2Wins / (float)total);
      DrawGraph.AddDataPoint(draws / (float)total);
   }

   /// <summary>
   /// Sammelt Informationen und stellt sie im Status-Label dar
   /// </summary>
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

      if (Simulator.Games.Count > 0 && GraphEnableCheckbox.isOn) {
         if (lastNumberOfCompletedGames < gamesFinished) {
            drawGraph(snake1Win, snake2Win, draw, gamesFinished);
         }
         else if (lastNumberOfCompletedGames > gamesFinished) {
            Bot1Graph.ClearDataPoints();
            Bot2Graph.ClearDataPoints();
            DrawGraph.ClearDataPoints();
         }
         lastNumberOfCompletedGames = gamesFinished;
      }
   }

   /// <summary>
   /// Speichert die ausgew�hlten Werte, die in den Bot-Auswahl-Dropdown Men�s stehen
   /// </summary>
   private void SaveDropdownState() {
      string text = SnakeBotSelects[0].options[SnakeBotSelects[0].value].text + "\n" + SnakeBotSelects[1].options[SnakeBotSelects[1].value].text;

      System.IO.File.WriteAllText("UserSettings\\botselect.txt", text);
   }

   /// <summary>
   /// Stellt das Replay auf das n�chste Spiel in der Reihenfolge ein
   /// </summary>
   public void ShowNextGame() {
      Game = Simulator.Games.ToArray()[++currentGameIndex];
      lastTick = 0f;
      currentMemoryIndex = 0;
      Paused = false;
   }

   /// <summary>
   /// Stellt das Replay auf das n�chste Spiel in der Reihenfolge ein, in dem der erste Bot gewonnen hat
   /// </summary>
   public void ShowNextGame_WinnerSnake_1() {
      GameLogic[] games = Simulator.Games.ToArray();
      for (int i = currentGameIndex + 1; i < Simulator.Games.Count; i++) {
         if (games[i].Winner == games[i].CurrentGameState.Snake_1) {
            Game = games[i];
            currentGameIndex = i;
            currentMemoryIndex = 0;
            lastTick = 0f;
            Paused = false;
            return;
         }
      }
   }

   /// <summary>
   /// Stellt das Replay auf das n�chste Spiel in der Reihenfolge ein, in dem der zweite Bot gewonnen hat
   /// </summary>
   public void ShowNextGame_WinnerSnake_2() {
      GameLogic[] games = Simulator.Games.ToArray();
      for (int i = currentGameIndex + 1; i < games.Length; i++) {
         if (games[i].Winner == games[i].CurrentGameState.Snake_2) {
            Game = games[i];
            currentGameIndex = i;
            currentMemoryIndex = 0;
            lastTick = 0f;
            Paused = false;
            return;
         }
      }
   }

   /// <summary>
   /// Stellt das Replay auf das n�chste Spiel in der Reihenfolge ein, das in unentschieden geendet hat
   /// </summary>
   public void ShowNextGame_Draw() {
      GameLogic[] games = Simulator.Games.ToArray();
      for (int i = currentGameIndex + 1; i < Simulator.Games.Count; i++) {
         if (games[i].Winner == null) {
            Game = games[i];
            currentGameIndex = i;
            currentMemoryIndex = 0;
            lastTick = 0f;
            Paused = false;
            return;
         }
      }
   }

   /// <summary>
   /// Stellt das Replay auf das n�chste Spiel in der Reihenfolge ein, das geendet hat, weil das Tick-Limit erreicht wurde
   /// </summary>
   public void ShowNextGame_TickLimit() {
      GameLogic[] games = Simulator.Games.ToArray();
      for (int i = currentGameIndex + 1; i < Simulator.Games.Count; i++) {
         if (games[i].Ticks >= games[i].MaxTicks) {
            Game = games[i];
            currentGameIndex = i;
            currentMemoryIndex = 0;
            lastTick = 0f;
            Paused = false;
            return;
         }
      }
   }

   /// <summary>
   /// Startet das derzeitige Replay von vorn
   /// </summary>
   public void ReplayCurrentGame() {
      GameLogic[] games = Simulator.Games.ToArray();
      if (currentGameIndex >= 0) {
         Game = games[currentGameIndex];
         lastTick = 0f;
         currentMemoryIndex = 0;
         Paused = false;
      }
   }
}
