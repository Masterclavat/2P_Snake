using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SaveHistoryToggler : MonoBehaviour {
   public GameSim Simulator;
   private Toggle toggle;

   private void Awake() {
      toggle = GetComponent<Toggle>();
   }

   private void Start() {
      toggle.onValueChanged.AddListener(x => Simulator.SaveHistory = x);
   }
}
