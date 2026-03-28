using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.StockMarket
{
    public class MarketDataManager : MonoBehaviour, IMarketDataProvider
    {
        public static MarketDataManager Instance { get; private set; }

        // Event fired whenever stock prices are updated
        public event Action OnStocksUpdated;

        private List<StockData> stocks = new List<StockData>();
        private System.Random random = new System.Random();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject); // keep across scenes
            InitializeMockStocks();
        }

        // Build initial 10 Indian stocks with random change percent
        private void InitializeMockStocks()
        {
            // Base prices for each stock (current price)
            var stockBase = new[]
            {
                ("AARAV", "Aarav Technologies", 850m),
                ("VRINDA", "Vrinda Pharmaceuticals", 1200m),
                ("SHAKTI", "Shakti Energy Limited", 600m),
                ("NANDAN", "Nandan Motors", 450m),
                ("KAILASH", "Kailash Cements", 300m),
                ("ANAND", "Anand Finance", 900m),
                ("SUVARNA", "Suvarna Jewellers", 700m),
                ("DHRUV", "Dhruv Steels", 550m),
                ("GAURAV", "Gaurav Telecom", 1100m),
                ("KAVYA", "Kavya Foods", 400m)
            };

            stocks = new List<StockData>();
            var random = new System.Random();

            foreach (var (symbol, company, price) in stockBase)
            {
                // Random change percent between -3% and +3%
                float changePercent = (float)(random.NextDouble() * 6 - 3); // -3 to +3
                decimal changeValue = price * (decimal)(changePercent / 100);
                decimal previousClose = price - changeValue;

                // Day high/low: current price +/- 1% to 5%
                decimal highPercent = 1 + (decimal)(random.NextDouble() * 4) / 100; // 1% to 5%
                decimal lowPercent = 1 - (decimal)(random.NextDouble() * 4) / 100;   // 1% to 5% below
                decimal dayHigh = price * highPercent;
                decimal dayLow = price * lowPercent;
                if (dayLow > price) dayLow = price * 0.99m; // ensure low < price
                if (dayHigh < price) dayHigh = price * 1.01m; // ensure high > price

                var stock = new StockData
                {
                    Symbol = symbol,
                    CompanyName = company,
                    CurrentPrice = price,
                    ChangeValue = changeValue,
                    ChangePercent = (decimal)changePercent,
                    DayHigh = Math.Round(dayHigh, 2),
                    DayLow = Math.Round(dayLow, 2),
                    PriceHistory = GeneratePriceHistory(price)
                };
                stocks.Add(stock);
            }
        }

        // Helper to create a stock with random change (no longer needed separately)
        // But we keep the GeneratePriceHistory method as before.

        // Generates 20 hourly price points from the base price
        private List<PricePoint> GeneratePriceHistory(decimal basePrice)
        {
            var history = new List<PricePoint>();
            var random = new System.Random();
            var time = DateTime.Now.AddHours(-20);
            decimal price = basePrice;

            for (int i = 0; i < 20; i++)
            {
                price += (decimal)(random.NextDouble() * 10 - 5);
                history.Add(new PricePoint
                {
                    Time = time,
                    Price = Math.Round(price, 2)
                });
                time = time.AddHours(1);
            }
            return history;
        }

        // IMarketDataProvider implementation
        public List<StockData> GetMarketStocks() => stocks;

        public StockData GetStockBySymbol(string symbol) => stocks.Find(s => s.Symbol == symbol);

        // Called from quests / events to simulate market movement
        public void TriggerMarketEvent(string notificationMessage)
        {
            // 1. Shuffle indices to randomly pick 5 up and 5 down
            List<int> indices = new List<int>();
            for (int i = 0; i < stocks.Count; i++) indices.Add(i);
            for (int i = 0; i < indices.Count; i++)
            {
                int j = random.Next(i, indices.Count);
                (indices[i], indices[j]) = (indices[j], indices[i]);
            }

            // 2. First 5 get increase, last 5 get decrease
            for (int i = 0; i < indices.Count; i++)
            {
                int idx = indices[i];
                StockData stock = stocks[idx];

                // Remember old price for change calculation
                decimal oldPrice = stock.CurrentPrice;

                // Determine change percent (5–20%)
                int percentChange;
                if (i < 5)   // first half go up
                    percentChange = random.Next(5, 21);
                else         // second half go down
                    percentChange = -random.Next(5, 21);

                decimal newPrice = oldPrice * (1 + percentChange / 100m);
                stock.CurrentPrice = Math.Round(newPrice, 2);

                // Update change values
                stock.ChangeValue = stock.CurrentPrice - oldPrice;
                stock.ChangePercent = (stock.ChangeValue / oldPrice) * 100m;

                // Add new price point to history (for charts)
                stock.PriceHistory.Add(new PricePoint
                {
                    Time = DateTime.Now,
                    Price = stock.CurrentPrice
                });
                // Keep only last 20 points (optional)
                if (stock.PriceHistory.Count > 20)
                    stock.PriceHistory.RemoveAt(0);

                // Update day high/low (simple example)
                if (stock.CurrentPrice > stock.DayHigh) stock.DayHigh = stock.CurrentPrice;
                if (stock.CurrentPrice < stock.DayLow) stock.DayLow = stock.CurrentPrice;
            }

            // 3. Notify all subscribers (MarketScreen, PortfolioScreen, etc.)
            OnStocksUpdated?.Invoke();

            // 4. Show a UI notification (you can expand this)
            Debug.Log($"Market Event: {notificationMessage}");
            // Optionally call a popup manager here
            // UINotificationManager.Show(notificationMessage);
        }
    }
}