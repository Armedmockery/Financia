using System;
using System.Collections.Generic;

namespace Game.StockMarket
{
    [Serializable]
    public class PortfolioSaveData
    {
        public float cashBalance;
        public List<HoldingData> holdings;
        public List<PricePoint> performanceHistory;

        public PortfolioSaveData(
        decimal cashBalance,
        List<HoldingData> holdings,
        List<PricePoint> performanceHistory)
        {
            this.cashBalance = (float)cashBalance;
            this.holdings = holdings;
            this.performanceHistory = performanceHistory;
        }
    }
}
