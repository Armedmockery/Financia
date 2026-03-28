using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Game.StockMarket
{
    public class StockChartRenderer : MonoBehaviour
    {
        public RectTransform chartArea;
        public GameObject pointPrefab;

        [Header("Tick UI")]
        public TextMeshProUGUI yTickPrefab;
        public TextMeshProUGUI xTickPrefab;
        public Transform yTickContainer;
        public Transform xTickContainer;

        [Header("Padding")]
        public float paddingLeft = 50f;
        public float paddingRight = 20f;
        public float paddingTop = 20f;
        public float paddingBottom = 40f;

        public void DrawChart(List<PricePoint> history)
        {
            foreach (Transform child in chartArea)
                Destroy(child.gameObject);

            foreach (Transform child in yTickContainer)
                Destroy(child.gameObject);

            foreach (Transform child in xTickContainer)
                Destroy(child.gameObject);

            if (history == null || history.Count == 0)
                return;

            float minPrice = float.MaxValue;
            float maxPrice = float.MinValue;

            foreach (var p in history)
            {
                float price = (float)p.Price;
                if (price < minPrice) minPrice = price;
                if (price > maxPrice) maxPrice = price;
            }

            float width = chartArea.rect.width - paddingLeft - paddingRight;
            float height = chartArea.rect.height - paddingTop - paddingBottom;

            for (int i = 0; i < history.Count; i++)
            {
                float normalizedX = i / (float)(history.Count - 1);
                float range = maxPrice - minPrice;

                float normalizedY;

                if (range == 0)
                    normalizedY = 0.5f;   // draw flat line in middle
                else
                    normalizedY =
                        ((float)history[i].Price - minPrice) / range;


                var point = Instantiate(pointPrefab, chartArea);

                RectTransform rt = point.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 0);
                rt.pivot = new Vector2(0.5f, 0.5f);

                rt.anchoredPosition =
                    new Vector2(
                        paddingLeft + normalizedX * width,
                        paddingBottom + normalizedY * height
                    );
            }

            CreateYTicks(minPrice, maxPrice, height);
            CreateXTicks(history, width);
            Debug.Log("Drawing points: " + history.Count);

        }

        void CreateYTicks(float minPrice, float maxPrice, float height)
        {
            for (int i = 0; i < 3; i++)
            {
                float t = i / 2f;
                float price = Mathf.Lerp(minPrice, maxPrice, t);

                var label = Instantiate(yTickPrefab, yTickContainer);
                label.text = price.ToString("0");

                RectTransform rt = label.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 0);

                rt.anchoredPosition =
                    new Vector2(
                        paddingLeft - 10,
                        paddingBottom + t * height
                    );
            }
        }

        void CreateXTicks(List<PricePoint> history, float width)
        {
            int[] indices = { 0, history.Count / 2, history.Count - 1 };

            foreach (int index in indices)
            {
                var label = Instantiate(xTickPrefab, xTickContainer);
                label.text = history[index].Time.ToString("HH:mm");

                RectTransform rt = label.GetComponent<RectTransform>();
                rt.anchorMin = new Vector2(0, 0);
                rt.anchorMax = new Vector2(0, 0);

                float normalizedX = index / (float)(history.Count - 1);

                rt.anchoredPosition =
                    new Vector2(
                        paddingLeft + normalizedX * width,
                        paddingBottom - 20
                    );
            }
        }
    }
}
