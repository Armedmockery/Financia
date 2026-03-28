// using UnityEngine;

// namespace Game.StockMarket
// {
//     /// <summary>
//     /// Helper class for displaying holding information in UI
//     /// Combines holding data with current market data
//     /// </summary>
//     public class HoldingDisplayData
//     {
//         public HoldingData Holding { get; private set; }
//         public StockData MarketData { get; private set; }
        
//         // Calculated properties
//         public decimal CurrentValue => MarketData?.CurrentPrice * Holding.Quantity ?? 0;
//         public decimal InvestedAmount => Holding.AverageBuyPrice * Holding.Quantity;
//         public decimal ProfitLoss => CurrentValue - InvestedAmount;
//         public decimal ProfitLossPercent => InvestedAmount > 0 ? (ProfitLoss / InvestedAmount) * 100 : 0;
        
//         public HoldingDisplayData(HoldingData holding, StockData marketData)
//         {
//             Holding = holding;
//             MarketData = marketData;
//         }
//     }
// }