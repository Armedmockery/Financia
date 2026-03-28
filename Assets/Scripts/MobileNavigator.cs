using UnityEngine;

namespace Game.StockMarket
{
    public class MobileAppNavigator : MonoBehaviour
    {
        [Header("Phone Level")]
        public GameObject homePage;
        public GameObject stockMarketApp;

        [Header("Stock App Screens")]
        public GameObject marketScreen;
        public GameObject portfolioScreen;
        public GameObject stockDetailScreen;

        [Header("Core Systems")]
        
        public IMarketDataProvider marketDataProvider;

        [SerializeField] private StockDetailController stockDetailController;
        public MarketScreenController marketScreenController;
        public PortfolioScreenController portfolioScreenController;
        
        private enum AppScreen
        {
            Market,
            Portfolio
        }

        private AppScreen lastAppScreen;
        public static MobileAppNavigator Instance;

private void Awake()
{
    Instance = this;

    stockDetailController =
        stockDetailScreen.GetComponentInChildren<StockDetailController>();

    marketScreenController =
        marketScreen.GetComponent<MarketScreenController>();

    portfolioScreenController =
        portfolioScreen.GetComponent<PortfolioScreenController>();

    marketDataProvider = MarketDataManager.Instance;
    if (marketDataProvider == null)
    {
        Debug.LogError("MarketDataManager not found!");
        return;
    }

    // Optionally subscribe to refresh the market screen when data changes
    MarketDataManager.Instance.OnStocksUpdated += RefreshMarketIfVisible;
}

private void Start()
{
    

    ShowHomePage();
}

public void RefreshPortfolioUI()
{
    if (portfolioScreen.activeSelf)
        portfolioScreenController.RefreshUI();
}



        // =============================
        // PHONE LEVEL NAVIGATION
        // =============================

        public void ShowHomePage()
        {
            homePage.SetActive(true);
            stockMarketApp.SetActive(false);
        }

        public void OpenStockMarketApp()
{
        homePage.SetActive(false);
        stockMarketApp.SetActive(true);

        ShowMarket();

        marketScreenController.Initialize(
            marketDataProvider,
            this
        );


}

        // =============================
        // APP LEVEL NAVIGATION
        // =============================

        public void ShowMarket()
        {
            HideAllAppScreens();
            marketScreen.SetActive(true);
            lastAppScreen = AppScreen.Market;
        }

        public void ShowPortfolio()
{
    HideAllAppScreens();
    portfolioScreen.SetActive(true);

    portfolioScreenController.Initialize(
    SaveController.Instance.GetPortfolioManager(),
    marketDataProvider,
    this
);

}


        public void ShowStockDetail(StockData stock)
        {
            HideAllAppScreens();
            stockDetailScreen.SetActive(true);

            stockDetailController.Show(
    stock,
    SaveController.Instance.GetPortfolioManager()
);

        }

        public void BackFromStockDetail()
        {
            if (lastAppScreen == AppScreen.Market)
                ShowMarket();
            else
                ShowPortfolio();
        }

        // =============================
        // INTERNAL
        // =============================

        private void HideAllAppScreens()
        {
            marketScreen.SetActive(false);
            portfolioScreen.SetActive(false);
            stockDetailScreen.SetActive(false);
        }

        private void OnDestroy()
        {
            if (MarketDataManager.Instance != null)
                MarketDataManager.Instance.OnStocksUpdated -= RefreshMarketIfVisible;
        }

        private void RefreshMarketIfVisible()
        {
            if (marketScreen.activeSelf)
                marketScreenController.PopulateMarket();
        }
    }
    
}
