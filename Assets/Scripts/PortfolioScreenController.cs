using UnityEngine;
using TMPro;
using Game.StockMarket;

namespace Game.StockMarket
{
    public class PortfolioScreenController : MonoBehaviour
    {
        [Header("Summary")]
        public TextMeshProUGUI balanceText;
        public TextMeshProUGUI totalInvestedText;
        public TextMeshProUGUI netWorthText;

        [Header("Holdings List")]
        public Transform holdingsContent;
        public HoldingListItemController holdingItemPrefab;

        private PortfolioManager portfolioManager;
        private IMarketDataProvider marketDataProvider;
        private MobileAppNavigator navigator;

        [Header("Performance Chart")]
        public StockChartRenderer chartRenderer;


        public void Initialize(
            PortfolioManager portfolio,
            IMarketDataProvider marketProvider,
            MobileAppNavigator appNavigator)
        {
            portfolioManager = portfolio;
            marketDataProvider = marketProvider;
            navigator = appNavigator;

            RefreshUI();
            MarketDataManager.Instance.OnStocksUpdated += OnMarketUpdated;
        }
        private void OnMarketUpdated()
        {
            RefreshUI();
        }

        public void RefreshUI()
        {
            

            UpdateSummary();
            PopulateHoldings();
            if (portfolioManager.PerformanceHistory.Count > 1)
    chartRenderer.DrawChart(portfolioManager.PerformanceHistory);
 
            
        }

        private void UpdateSummary()
        {
            balanceText.text = $"₹{portfolioManager.CashBalance:N2}";

            decimal invested = portfolioManager.GetTotalInvestedAmount();
            totalInvestedText.text = $"₹{invested:N2}";

            decimal netWorth = portfolioManager.GetNetWorth(
                symbol => marketDataProvider.GetStockBySymbol(symbol).CurrentPrice
            );
            netWorthText.text = $"₹{netWorth:N2}";
            if (portfolioManager == null)
    return;

            

        }

        private void PopulateHoldings()
        {
            foreach (Transform child in holdingsContent)
                Destroy(child.gameObject);

            var holdings = portfolioManager.GetAllHoldings();

            foreach (var holding in holdings)
            {
                var stock = marketDataProvider.GetStockBySymbol(holding.Symbol);

                var item = Instantiate(holdingItemPrefab, holdingsContent);
                item.Setup(holding, stock, navigator);
            }
        }
    }
}
