using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.StockMarket
{
    public class HoldingListItemController : MonoBehaviour
    {
        [Header("UI")]
        public TextMeshProUGUI symbolText;
        //public TextMeshProUGUI companyNameText;
        public TextMeshProUGUI quantityText;
        public TextMeshProUGUI avgBuyText;
        public TextMeshProUGUI currentPriceText;
        public TextMeshProUGUI profitLossText;
        public Color profitColor = Color.green;
        public Color lossColor = Color.red;
        private StockData stockData;
        private MobileAppNavigator navigator;

        public void Setup(
            HoldingData holding,
            StockData stock,
            MobileAppNavigator appNavigator)
        {
            stockData = stock;
            navigator = appNavigator;

            symbolText.text = holding.Symbol;
            //companyNameText.text = holding.CompanyName;
            quantityText.text = $"Qty: {holding.Quantity}";
            avgBuyText.text = $"Avg: ₹{holding.AverageBuyPrice:N2}";
            currentPriceText.text = $"₹{stock.CurrentPrice:N2}";

            decimal pnl =
    (stock.CurrentPrice - (decimal)holding.AverageBuyPrice)
    * holding.Quantity;


            profitLossText.text = $"{(pnl >= 0 ? "+" : "")}₹{pnl:N2}";
            profitLossText.color = pnl >= 0 ? profitColor : lossColor;
        }

        public void OnItemClicked()
        {
            navigator.ShowStockDetail(stockData);
        }
    }
}
