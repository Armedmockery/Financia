using System;
using UnityEngine;
namespace Game.StockMarket
{
    [Serializable]
    public class HoldingData
    {
        public string Symbol;
        public string CompanyName;

        public int Quantity;
        public float AverageBuyPrice;
    }
}
