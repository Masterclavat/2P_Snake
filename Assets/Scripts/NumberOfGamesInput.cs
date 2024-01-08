using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NumberOfGamesInput : MonoBehaviour {
   public GameSim Simulator;
   private TMP_InputField inputText;

   private void Awake() {
      inputText = GetComponent<TMP_InputField>();
      inputText.onEndEdit.AddListener(SetGamesToSimulate);
   }

   private void SetGamesToSimulate(string input) {
      if (int.TryParse(input, out int num)) {
         Simulator.GamesToSimulate = num;
      }
   }
}
