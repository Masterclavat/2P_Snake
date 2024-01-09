using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SaveHistoryToggler : MonoBehaviour {
   public GameSim Simulator;
   private Toggle toggle;
   [SerializeField]
   private string savePath = "UserSettings\\saveHistory.txt";

   private void Awake() {
      toggle = GetComponent<Toggle>();
   }

   private void Start() {
      toggle.onValueChanged.AddListener(x => { Simulator.SaveHistory = x; saveState(x); });
      loadState();
   }

   private void saveState(bool state) {
      File.WriteAllText(savePath, state.ToString());
   }

   private void loadState() {
      if (File.Exists(savePath)) {
         string content = File.ReadAllText(savePath);
         if (bool.TryParse(content, out bool val)) {
            if (val != toggle.isOn) {
               toggle.isOn = val;
            }
         }
      }
   }
}
