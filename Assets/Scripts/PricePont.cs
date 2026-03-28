using System;
using UnityEngine;
namespace Game.StockMarket
{
    [Serializable]
    public class PricePoint
    {
        public DateTime Time;   // timestamp
        public decimal Price;   // price at that time
    }
}
