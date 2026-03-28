using System;
using System.Collections.Generic;

namespace Game.StockMarket
{
    public class MockMarketDataProvider : IMarketDataProvider
    {
        private List<StockData> _stocks;

        public MockMarketDataProvider()
        {
            _stocks = CreateMockStocks();
        }

        public List<StockData> GetMarketStocks()
        {
            return _stocks;
        }

        public StockData GetStockBySymbol(string symbol)
        {
            return _stocks.Find(s => s.Symbol == symbol);
        }

        // ---------------------------
        // Mock data creation
        // ---------------------------

        private List<StockData> CreateMockStocks()
        {
            return new List<StockData>
            {
                CreateStock(
                    "INFY",
                    "Infosys Limited",
                    6969.90m,
                    +8.90m,
                    +0.57m,
                    1572.50m,
                    1556.30m
                ),
                CreateStock(
                    "INFY",
                    "Tata Consultancy Services",
                    6969.90m,
                    -12.50m,
                    -0.36m,
                    3478.90m,
                    3445.20m
                ),
                CreateStock(
                    "RELIANCE",
                    "Reliance Industries",
                    6969.90m,
                    +12.30m,
                    +0.50m,
                    2468.90m,
                    2445.20m
                )
            };
        }

        private StockData CreateStock(
            string symbol,
            string companyName,
            decimal currentPrice,
            decimal changeValue,
            decimal changePercent,
            decimal dayHigh,
            decimal dayLow
        )
        {
            return new StockData
            {
                Symbol = symbol,
                CompanyName = companyName,
                CurrentPrice = currentPrice,
                ChangeValue = changeValue,
                ChangePercent = changePercent,
                DayHigh = dayHigh,
                DayLow = dayLow,
                PriceHistory = GeneratePriceHistory(currentPrice)
            };
        }

        private List<PricePoint> GeneratePriceHistory(decimal basePrice)
        {
            var history = new List<PricePoint>();
            var random = new Random();
            var time = DateTime.Now.AddDays(-10);

            decimal price = basePrice;

            for (int i = 0; i < 20; i++)
            {
                price += (decimal)(random.NextDouble() * 20 - 10);
                history.Add(new PricePoint
                {
                    Time = time,
                    Price = Math.Round(price, 2)
                });
                time = time.AddHours(6);
            }

            return history;
        }
    }
}
