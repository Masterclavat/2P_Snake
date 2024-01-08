using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PercentageGraph : MonoBehaviour {
   LineRenderer lr;
   private TextMeshProUGUI labelText;
   private List<float> dataPoints = new List<float>();
   [SerializeField]
   private string label;
   public string Label { get => label; set => label = value; }

   private void Awake() {
      lr = GetComponent<LineRenderer>();
      labelText = GetComponentInChildren<TextMeshProUGUI>();
   }

   void Update() {
      lr.positionCount = dataPoints.Count;
      if (dataPoints.Count > 0) {
         Vector2 size = (transform as RectTransform).rect.size;
         Vector3[] points = new Vector3[dataPoints.Count];
         for (int i = 0; i < dataPoints.Count; i++) {
            points[i] = new Vector3(i / (float)dataPoints.Count * size.x, size.y * dataPoints[i], -1);
         }
         lr.SetPositions(points);
         labelText.rectTransform.localPosition = points.Last() + new Vector3(0, 15, 0);
         labelText.text = string.Format("{0}: {1}%", Label, (dataPoints.Last() * 100).ToString("N2"));
      }
   }

   public void AddDataPoint(float percentage) {
      dataPoints.Add(percentage);
   }
   public void ClearDataPoints() {
      dataPoints.Clear();
   }
}
