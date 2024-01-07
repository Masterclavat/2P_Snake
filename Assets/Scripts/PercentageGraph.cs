using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PercentageGraph : MonoBehaviour {
   LineRenderer lr;
   private List<float> dataPoints = new List<float>();

   private void Awake() {
      lr = GetComponent<LineRenderer>();
   }
   void Start() {

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
      }
   }

   public void AddDataPoint(float percentage) {
      dataPoints.Add(percentage);
   }
   public void ClearDataPoints() {
      dataPoints.Clear();
   }
}
