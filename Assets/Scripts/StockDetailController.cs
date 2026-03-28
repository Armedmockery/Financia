using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Game.StockMarket;

namespace Game.StockMarket
{
    public class StockDetailController : MonoBehaviour
    {
        [Header("Text")]
        public TextMeshProUGUI symbolText;
        public TextMeshProUGUI companyNameText;
        public TextMeshProUGUI priceText;
        public TextMeshProUGUI changeText;
        public TextMeshProUGUI dayHighText;
        public TextMeshProUGUI dayLowText;
        public TextMeshProUGUI ownedQtyText;

        [Header("Quantity Selector")]
        public TextMeshProUGUI quantityText;
        public Button plusButton;
        public Button minusButton;

        [Header("Actions")]
        public Button buyButton;
        public Button sellButton;

        private StockData currentStock;
        private PortfolioManager portfolioManager;

        private int selectedQuantity = 1;
        [Header("Chart")]
public StockChartRenderer chartRenderer;


        // -------------------------
        // INITIALIZATION
        // -------------------------

        public void Show(StockData stock, PortfolioManager portfolio)
        {
            currentStock = stock;
            portfolioManager = portfolio;

            selectedQuantity = 1;
            Debug.Log(portfolio);
Debug.Log(stock);

            RefreshUI();
        }

        // -------------------------
        // UI REFRESH
        // -------------------------

        private void RefreshUI()
        {
            Debug.Log("RefreshUI called");
            Debug.Log(symbolText);
            Debug.Log(companyNameText);
            Debug.Log(priceText);
            Debug.Log(changeText);
            Debug.Log(dayHighText);
            Debug.Log(dayLowText);
            Debug.Log(ownedQtyText);
            Debug.Log(quantityText);
            Debug.Log(buyButton);
            Debug.Log(sellButton);
            Debug.Log(minusButton);
            Debug.Log(chartRenderer);
            symbolText.text = currentStock.Symbol;
            companyNameText.text = currentStock.CompanyName;

            priceText.text = $"₹{currentStock.CurrentPrice:N2}";
            changeText.text =
                $"{currentStock.ChangeValue:+0.##;-0.##} ({currentStock.ChangePercent:+0.##;-0.##}%)";

            if (currentStock.ChangeValue > 0)
            {
                changeText.color = Color.green;
            }
            else if (currentStock.ChangeValue < 0)
            {
                changeText.color = Color.red;
            }
            else
            {
                changeText.color = Color.white;
            }


            dayHighText.text = $"Day High: ₹{currentStock.DayHigh:N2}";
            dayLowText.text = $"Day Low: ₹{currentStock.DayLow:N2}";

            quantityText.text = selectedQuantity.ToString();

            var holding = portfolioManager.GetHolding(currentStock.Symbol);
            int ownedQty = holding != null ? holding.Quantity : 0;

            ownedQtyText.text = $"Owned: {ownedQty}";

            // Button states
            buyButton.interactable =
                portfolioManager.CanBuy(currentStock.CurrentPrice, selectedQuantity);

            sellButton.interactable =
                portfolioManager.CanSell(currentStock.Symbol, selectedQuantity);

            minusButton.interactable = selectedQuantity > 1;
            
            if (chartRenderer != null && currentStock.PriceHistory != null)
{
    chartRenderer.DrawChart(currentStock.PriceHistory);
}


        }

        // -------------------------
        // BUTTON HANDLERS
        // -------------------------

        public void OnIncreaseQuantity()
        {
            selectedQuantity++;
            RefreshUI();
        }

        public void OnDecreaseQuantity()
        {
            if (selectedQuantity > 1)
                selectedQuantity--;

            RefreshUI();
        }

        public void OnBuyClicked()
        {
            Debug.Log("BUY CLICKED");
            bool success = portfolioManager.BuyStock(currentStock, selectedQuantity);
            Debug.Log("BUY SUCCESS: " + success);
            QuestController.Instance?.UpdateCustomObjective("stock");
            if (success)
            {
                portfolioManager.RecordPerformance(
                    symbol =>
                    {
                        if (symbol == currentStock.Symbol)
                            return (decimal)currentStock.CurrentPrice;

                        var holding = portfolioManager.GetHolding(symbol);
                        return holding != null ? (decimal)holding.AverageBuyPrice : 0m;
                    }
                );

                RefreshUI();
            }
        }

        public void OnSellClicked()
        {
            bool success = portfolioManager.SellStock(currentStock, selectedQuantity);
            if (success)
            {
                portfolioManager.RecordPerformance(
                    symbol =>
                    {
                        if (symbol == currentStock.Symbol)
                            return (decimal)currentStock.CurrentPrice;

                        var holding = portfolioManager.GetHolding(symbol);
                        return holding != null ? (decimal)holding.AverageBuyPrice : 0m;
                    }
                );
                RefreshUI();
            }
        }
    }
}
