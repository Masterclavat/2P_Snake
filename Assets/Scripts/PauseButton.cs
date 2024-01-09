using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PauseButton : MonoBehaviour {
   private Button pauseButton;
   private TextMeshProUGUI text;
   public GameDisplay Display;
   const string PlayIcon = "\u25B6";
   const string PauseIcon = "\u23F8";

   private void Awake() {
      pauseButton = GetComponent<Button>();
      text = GetComponentInChildren<TextMeshProUGUI>();
   }

   private void Start() {
      pauseButton.onClick.AddListener(() => Display.Paused = !Display.Paused);
   }

   private void Update() {
      if (Input.GetKeyDown(KeyCode.Space)) {
         pauseButton.onClick.Invoke();
      }
      text.text = Display.Paused ? PlayIcon : PauseIcon;
   }
}
