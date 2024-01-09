using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TickSpeedSlider : MonoBehaviour {
   private Slider slider;
   public GameDisplay display;
   public TextMeshProUGUI tickSpeedText;


   private void Awake() {
      slider = GetComponent<Slider>();
   }

   private void Start() {
      slider.onValueChanged.AddListener(x => { display.GameTickRate = x;
                                               tickSpeedText.text = string.Format("Tick Speed: {0}s", x.ToString("N2")); });
   }
}
