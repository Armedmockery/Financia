// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.Networking;

// namespace Game.StockMarket
// {
//     public class BackendMarketProvider : MonoBehaviour
//     {
//          private const string API_URL = "https://stockmarket-backend-m5z2.onrender.com/stocks";
//         // private const string API_URL = "http://192.168.1.29:5001/stocks";

//         public IEnumerator FetchMarket(System.Action<List<StockData>> onSuccess)
//         {
//             using (UnityWebRequest req = UnityWebRequest.Get(API_URL))
//             {
//                 yield return req.SendWebRequest();

//                 if (req.result != UnityWebRequest.Result.Success)
//                 {
//                     Debug.LogError(req.error);
//                 }
//                 else
//                 {
//                     string json = req.downloadHandler.text;
//                     Debug.Log("Backend response: " + json);

//                     BackendStock[] backendStocks =
//                         JsonHelper.FromJson<BackendStock>(json);

//                     var newStocks = new List<StockData>();

//                     foreach (var backend in backendStocks)
//                     {
//                         newStocks.Add(new StockData
//                         {
//                             Symbol = backend.symbol,
//                             CompanyName = backend.companyName,
//                             CurrentPrice = (decimal)backend.price,
//                             ChangeValue = (decimal)backend.change,
//                             ChangePercent = (decimal)backend.percent,
//                             DayHigh = (decimal)backend.high,
//                             DayLow = (decimal)backend.low,
//                             PriceHistory = GeneratePriceHistory((decimal)backend.price)

//                         });
//                     }


//                     onSuccess?.Invoke(newStocks);
//                 }
//             }
//         }
//         private List<PricePoint> GeneratePriceHistory(decimal basePrice)
// {
//     var history = new List<PricePoint>();
//     var random = new System.Random();
//     var time = System.DateTime.Now.AddHours(-20);

//     decimal price = basePrice;

//     for (int i = 0; i < 20; i++)
//     {
//         price += (decimal)(random.NextDouble() * 10 - 5);

//         history.Add(new PricePoint
//         {
//             Time = time,
//             Price = System.Math.Round(price, 2)
//         });

//         time = time.AddHours(1);
//     }

//     return history;
// }

//     }

//     [System.Serializable]
//     public class BackendStock
//     {
//         public string symbol;
//         public string companyName;
//         public float price;
//         public float change;
//         public float percent;
//         public float high;
//         public float low;
//     }
    
// }
