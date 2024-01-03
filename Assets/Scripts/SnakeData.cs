using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnakeData : ICloneable {
   public Color Color;
   public List<Vector2Int> Segments = new List<Vector2Int>();
   public SnakeDirection Direction { get; set; }
   public SnakeBot Owner { get; }
   public bool IsAlive { get; set; }
   public object DebugData;

   /// <summary>
   /// Die Position des Kopfes der Schlange
   /// </summary>
   public Vector2Int Head
   {
      get
      {
         return Segments[Segments.Count - 1];
      }
   }

   /// <summary>
   /// Die Position an der der Kopf der Schlange nach dem nächsten Tick sein wird.
   /// Verwendet dazu die derzeitige Richtung in Direction
   /// </summary>
   public Vector2Int NextHeadPosition
   {
      get
      {
         if (Segments.Count == 0)
            return Vector2Int.zero;

         return Head + BotUtilities.DirectionChange[Direction];
      }
   }

   /// <summary>
   /// Der Konstruktor der Klasse
   /// </summary>
   /// <param name="owner">Der Bot, der die Schlange kontrolliert</param>
   /// <param name="color">Die gewünschte Farbe</param>
   public SnakeData(SnakeBot owner, Color color) {
      Owner = owner;
      Color = color;
      IsAlive = true;
   }

   /// <summary>
   /// Eine Interne Funktion, die für das Spiel wichtig ist. Kann ignoriert werden.
   /// Falls du dich dafür interessierst, was sie macht:
   /// Erstellt eine Kopie der Daten der Schlange. Wird für das Replay von Runden verwendet.
   /// </summary>
   /// <returns>Die Kopie des Objekts</returns>
   public object Clone() {
      SnakeData clone = new SnakeData(Owner, Color);
      clone.Direction = Direction;
      clone.Segments = new List<Vector2Int>(Segments.ToArray());
      clone.IsAlive = IsAlive;
      clone.DebugData = DebugData;

      return clone;
   }
}
