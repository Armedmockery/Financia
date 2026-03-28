using System.Collections.Generic;

namespace Game.StockMarket
{
    public interface IMarketDataProvider
    {
        List<StockData> GetMarketStocks();
        StockData GetStockBySymbol(string symbol);
    }
}
