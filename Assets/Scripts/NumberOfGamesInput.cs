using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

public class NumberOfGamesInput : MonoBehaviour {
   public GameSim Simulator;
   private TMP_InputField inputText;
   public string savePath = "UserSettings\\numGames.txt";

   private void Awake() {
      inputText = GetComponent<TMP_InputField>();
   }

   private void Start() {
      inputText.onEndEdit.AddListener(SetGamesToSimulate);
      if (File.Exists(savePath)) {
         string content = File.ReadAllText(savePath);
         if(int.TryParse(content, out int result)) {
            inputText.text = content;
            SetGamesToSimulate(content);
         }
      }
   }

   private void SetGamesToSimulate(string input) {
      if (int.TryParse(input, out int num)) {
         Simulator.GamesToSimulate = num;
         try {
            File.WriteAllText(savePath, num.ToString());
         }
         catch(Exception ex) {
            Debug.LogError("Failed to save number of games: " + ex.Message);
         }         
      }
   }
}
