using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.StockMarket
{
    public class StockListItemController : MonoBehaviour
    {
        [Header("UI")]
        public TextMeshProUGUI symbolText;
        public TextMeshProUGUI companyNameText;
        public TextMeshProUGUI priceText;
        public TextMeshProUGUI changeText;

        private StockData stockData;
        private MobileAppNavigator navigator;

        // Called by MarketScreenController
        public void Setup(StockData data, MobileAppNavigator appNavigator)
        {
            stockData = data;
            navigator = appNavigator;

            symbolText.text = data.Symbol;
            companyNameText.text = data.CompanyName;
            priceText.text = $"₹{data.CurrentPrice:N2}";
            changeText.text = $"{data.ChangePercent:+0.##;-0.##}%";
        }

        // Button OnClick
        public void OnItemClicked()
        {
            navigator.ShowStockDetail(stockData);
        }
    }
}
