using System;
using UnityEngine;

namespace RocketRaja
{
    /// <summary>32 persistent atlas-region sprites, shared by shelves and product details.</summary>
    public sealed class RedesignProductArt : ScriptableObject
    {
        public Sprite[] appearances = new Sprite[32];
        public Sprite At(int product, int tier)
        {
            if (product < 0 || product >= 8 || tier < 0 || tier > 3) throw new ArgumentOutOfRangeException();
            return appearances[product * 4 + tier];
        }
        public static string Label(int tier) => tier < 0 ? "Empty" : tier == 0 ? "Basic" : tier == 1 ? "Signature I" : tier == 2 ? "Premium · Signature IV" : "Showcase · Signature X";
    }
}
