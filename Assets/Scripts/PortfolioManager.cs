using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Game.StockMarket
{
    public class PortfolioManager
    {
        public decimal CashBalance { get; private set; }

        // Key = Stock Symbol (INFY, TCS, etc.)
        private Dictionary<string, HoldingData> _holdings;

        //chart data
        public List<PricePoint> PerformanceHistory { get; private set; }


        public PortfolioManager(decimal startingCash)
        {
            CashBalance = startingCash;
            _holdings = new Dictionary<string, HoldingData>();
            PerformanceHistory = new List<PricePoint>();
            PerformanceHistory.Add(new PricePoint
            {
                Time = DateTime.Now,
                Price = startingCash
            });

        }
        public PortfolioManager(PortfolioSaveData data)
        {
            CashBalance = (decimal)data.cashBalance;
            _holdings = new Dictionary<string, HoldingData>();

            foreach (var holding in data.holdings)
            {
                _holdings[holding.Symbol] = holding;
            }

            PerformanceHistory =
                data.performanceHistory ?? new List<PricePoint>();

            if (PerformanceHistory.Count == 0)
            {
                PerformanceHistory.Add(new PricePoint
                {
                    Time = DateTime.Now,
                    Price = CashBalance
                });
            }
        }



       



        //Generate Performance History
        public void RecordPerformance(Func<string, decimal> currentPriceProvider)
        {
            decimal netWorth = GetNetWorth(currentPriceProvider);

            PerformanceHistory.Add(new PricePoint
            {
                Time = DateTime.Now,
                Price = netWorth
            });
        }


        // -------------------------
        // READ OPERATIONS
        // -------------------------

        public List<HoldingData> GetAllHoldings()
        {
            return _holdings.Values.ToList();
        }

        public HoldingData GetHolding(string symbol)
        {
            _holdings.TryGetValue(symbol, out var holding);
            return holding;
        }

        public bool OwnsStock(string symbol)
        {
            return _holdings.ContainsKey(symbol);
        }

        // -------------------------
        // BUY LOGIC
        // -------------------------

        public bool CanBuy(decimal price, int quantity)
        {
            return CashBalance >= price * quantity;
        }

        public bool BuyStock(StockData stock, int quantity)
        {
            decimal totalCost = stock.CurrentPrice * quantity;

            if (!CanBuy(stock.CurrentPrice, quantity))
                return false;

            CashBalance -= totalCost;

            if (_holdings.ContainsKey(stock.Symbol))
            {
                // Recalculate average buy price
                var holding = _holdings[stock.Symbol];

                decimal existingValue =(decimal)holding.AverageBuyPrice * holding.Quantity;

                decimal newValue = stock.CurrentPrice * quantity;
                int newQuantity = holding.Quantity + quantity;

                holding.AverageBuyPrice =
                    (float)((existingValue + newValue) / newQuantity);

                holding.Quantity = newQuantity;
            }
            else
            {
                _holdings[stock.Symbol] = new HoldingData
                {
                    Symbol = stock.Symbol,
                    CompanyName = stock.CompanyName,
                    Quantity = quantity,
                    AverageBuyPrice = (float)stock.CurrentPrice
                };
            }
Debug.Log(SaveController.Instance);
Debug.Log(this);

            
            SaveController.AutoSave();
            MobileAppNavigator.Instance?.RefreshPortfolioUI();

            return true;

            
        }

        // -------------------------
        // SELL LOGIC
        // -------------------------

        public bool CanSell(string symbol, int quantity)
        {
            return _holdings.ContainsKey(symbol) &&
                   _holdings[symbol].Quantity >= quantity;
        }

        public bool SellStock(StockData stock, int quantity)
        {
            if (!CanSell(stock.Symbol, quantity))
                return false;

            var holding = _holdings[stock.Symbol];

            decimal saleValue = stock.CurrentPrice * quantity;
            CashBalance += saleValue;

            holding.Quantity -= quantity;

            if (holding.Quantity == 0)
            {
                _holdings.Remove(stock.Symbol);
            }

            SaveController.AutoSave();
MobileAppNavigator.Instance?.RefreshPortfolioUI();


            return true;

            
        }

        // -------------------------
        // PORTFOLIO METRICS
        // -------------------------

        public decimal GetTotalInvestedAmount()
{
    return _holdings.Values.Sum(h =>
        (decimal)h.AverageBuyPrice * h.Quantity);
}


        public decimal GetNetWorth(Func<string, decimal> currentPriceProvider)
        {
            decimal holdingsValue = _holdings.Values.Sum(h =>
                currentPriceProvider(h.Symbol) * h.Quantity);

            return CashBalance + holdingsValue;
        }
        public PortfolioSaveData ToSaveData()
        {
            return new PortfolioSaveData(
                CashBalance,
                GetAllHoldings(),
                PerformanceHistory
            );
        }

    }
}
