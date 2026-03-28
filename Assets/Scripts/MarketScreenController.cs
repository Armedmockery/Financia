using UnityEngine;
using System.Collections;

namespace Game.StockMarket
{
    public class MarketScreenController : MonoBehaviour
    {
        public Transform listParent;
        public StockListItemController stockListItemPrefab;

        private IMarketDataProvider marketDataProvider;
        private MobileAppNavigator navigator;
        
        // public BackendMarketProvider apiProvider;



        // Called once by navigator
        public void Initialize(
            IMarketDataProvider provider,
            MobileAppNavigator appNavigator)
        {
            Debug.Log("MarketScreenController.Initialize called");

            marketDataProvider = provider;
            navigator = appNavigator;

            PopulateMarket();
            //StartCoroutine(UpdateMarketPrices());

            // Subscribe to price updates so the list refreshes automatically
            MarketDataManager.Instance.OnStocksUpdated +=OnMarketDataUpdated;
        }

        private void OnMarketDataUpdated()
    {
        PopulateMarket();
    }

        public void PopulateMarket()
{
    if (marketDataProvider == null)
    {
        Debug.LogError("marketDataProvider is NULL");
        return;
    }

    var stocks = marketDataProvider.GetMarketStocks();

    if (stocks == null)
    {
        Debug.LogError("GetMarketStocks() returned NULL");
        return;
    }

    Debug.Log("Stocks count: " + stocks.Count);

    foreach (Transform child in listParent)
        Destroy(child.gameObject);

    foreach (var stock in stocks)
    {
        var item = Instantiate(stockListItemPrefab, listParent);
        item.Setup(stock, navigator);
    }
}

// Update market prices every 60 seconds
// private IEnumerator UpdateMarketPrices()
// {
//     while (true)
//     {
//         yield return apiProvider.FetchMarket(newStocks =>
//         {
//             var backendProvider =
//                 marketDataProvider as BackendMarketDataProvider;

//             backendProvider.SetStocks(newStocks);
//         });

//         PopulateMarket();
//         yield return new WaitForSeconds(30f);
//     }
// }




    }
    

}
