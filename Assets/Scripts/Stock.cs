using System;
using System.Collections.Generic;

namespace Game.StockMarket
{
    [Serializable]
    public class StockData
    {
        public string Symbol;          // INFY
        public string CompanyName;     // Infosys Limited

        public decimal CurrentPrice;   // 1567.80
        public decimal ChangeValue;    // +8.90
        public decimal ChangePercent;  // +0.57

        public decimal DayHigh;         // 1572.50
        public decimal DayLow;          // 1556.30

        public List<PricePoint> PriceHistory; // For chart
    }
}
