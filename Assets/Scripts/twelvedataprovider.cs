using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

namespace Game.StockMarket
{
    public class TwelveDataMarketProvider : MonoBehaviour
    {
        private const string API_KEY = "0bfd16d131cc4bcd826b76e3f6579378";

        public IEnumerator FetchPrice(string symbol, System.Action<decimal> onSuccess)
        {
            string url =
                $"https://api.twelvedata.com/price?symbol={symbol}&apikey={API_KEY}";

            using (UnityWebRequest req = UnityWebRequest.Get(url))
            {
                yield return req.SendWebRequest();

                if (req.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(req.error);
                }
                else
                {
                    var json = req.downloadHandler.text;

                PriceResponse response =
                    JsonUtility.FromJson<PriceResponse>(json);

                if (response == null || string.IsNullOrEmpty(response.price))
                {
                    Debug.LogWarning("Invalid API response: " + json);
                    yield break;
                }
                Debug.Log("API Response: " + json);


                decimal price =
                    decimal.Parse(response.price, CultureInfo.InvariantCulture);

                onSuccess?.Invoke(price);

                }
            }
        }

        [System.Serializable]
        private class PriceResponse
        {
            public string price;
        }
    }
}
