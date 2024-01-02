using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeData : ICloneable {
   public Color Color;
   public List<Vector2Int> Segments = new List<Vector2Int>();
   public SnakeDirection Direction;
   public SnakeBot Owner;
   public bool IsAlive;
   public object DebugData;

   private Dictionary<SnakeDirection, Vector2Int> directionChange = new Dictionary<SnakeDirection, Vector2Int>(){
      {SnakeDirection.Down, new Vector2Int(0,-1)},
      {SnakeDirection.Left, new Vector2Int(-1,0)},
      {SnakeDirection.Up, new Vector2Int(0, 1)},
      {SnakeDirection.Right, new Vector2Int(1,0)}};
   public Vector2Int Head
   {
      get
      {
         return Segments[Segments.Count - 1];
      }
   }

   public Vector2Int NextHeadPosition
   {
      get
      {
         if (Segments.Count == 0)
            return Vector2Int.zero;

         return Head + directionChange[Direction];
      }
   }

   public SnakeData(SnakeBot owner, Color color) {
      Owner = owner;
      Color = color;
      IsAlive = true;
   }

   public object Clone() {
      SnakeData clone = new SnakeData(Owner, Color);
      clone.Direction = Direction;
      clone.Segments = new List<Vector2Int>(Segments.ToArray());
      clone.IsAlive = IsAlive;
      clone.DebugData = DebugData;

      return clone;
   }
}
