using System.Collections.Generic;
using System;
using UnityEngine;

namespace Game.StockMarket
{
    [Serializable]
    public class PortfolioData
    {
        public decimal CashBalance;          // available money
        public decimal TotalInvested;        // sum of buy costs
        public decimal NetWorth;             // cash + holdings value

        public List<HoldingData> Holdings;
        public List<PricePoint> NetWorthHistory;
    }
}
