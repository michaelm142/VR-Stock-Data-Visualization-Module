using System;
using FinanceModule;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WatchlistTickerController : MonoBehaviour
{
    public List<GameObject> tickers = new List<GameObject>();

    public Transform Addbutton;

    public GameObject tickerPrefab;

    public float TickerSpacing = 10.0f;

    private string tickerLayout = "Last Price\tChange\t% Change\t\n${0}\t{1}<color=\"{2}\">${3}{4}\t<color=\"{5}\">{6}%";

    // Start is called before the first frame update
    void Start()
    {
        Stonks.Load();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void AddTicker()
    {
        Rect rect = GetComponent<RectTransform>().rect;
        Rect prefabRect = tickerPrefab.GetComponent<RectTransform>().rect;
        GameObject ticker = Instantiate(tickerPrefab, transform);
        ticker.name = "Ticker #" + tickers.Count.ToString();
        ticker.transform.localPosition = Vector3.down * (prefabRect.height + TickerSpacing) * tickers.Count;

        tickers.Add(ticker);
        //tickers.ForEach(t => t.transform.SetParent(null));
        //Addbutton.transform.SetParent(null);
        GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 0.0f, rect.height + (prefabRect.height + TickerSpacing));
        //tickers.ForEach(t => t.transform.SetParent(transform));
        //Addbutton.transform.SetParent(transform);

        //Addbutton.transform.localPosition = Vector3.zero;

        ticker.transform.Find("Ticker/CloseButton").GetComponent<Button>().onClick.AddListener(delegate () { RemoveTicker(ticker); });
        ticker.transform.Find("StockSymbol").GetComponent<TMP_InputField>().onEndEdit.AddListener(delegate (string text) { UpdateTicker(text, ticker); });
        transform.localPosition = Vector3.zero;
    }

    public void RemoveTicker(GameObject ticker)
    {
        Vector3 startPoint = ticker.transform.localPosition;

        int index = tickers.IndexOf(ticker);
        Destroy(ticker);
        tickers.RemoveAt(index);
        for (int i = index; i < tickers.Count; i++)
        {
            tickers[i].transform.localPosition = startPoint + Vector3.down * (tickerPrefab.GetComponent<RectTransform>().rect.height + TickerSpacing) * (i - index);
        }
        Rect rect = GetComponent<RectTransform>().rect;
        Rect prefabRect = tickerPrefab.GetComponent<RectTransform>().rect;
        GetComponent<RectTransform>().SetInsetAndSizeFromParentEdge(RectTransform.Edge.Bottom, 50.0f, rect.height - (prefabRect.height + TickerSpacing));
    }

    void UpdateTicker(string symbol, GameObject ticker)
    {
        Stonk? s = Stonks.Get(symbol.ToUpper());
        if (s == null)
            return;
        Stonk stonk = s.Value;
        stonk.HistoricalData = Stonks.Get(stonk, System.DateTime.Now.AddDays(-1), System.DateTime.Now);
        Stonket yesterdayStonket = stonk.HistoricalData[0];
        Stonket todayStonket = stonk.HistoricalData[1];

        decimal closePrice = todayStonket.AdjustedClosingPrice;
        decimal priceChange = todayStonket.AdjustedClosingPrice - yesterdayStonket.AdjustedClosingPrice;
        decimal percentChange = System.Math.Round(priceChange / yesterdayStonket.AdjustedClosingPrice * 100, 2);
        closePrice = Math.Round(closePrice, 2);
        priceChange = Math.Round(priceChange, 2);

        ticker.transform.Find("Ticker/Text").GetComponent<TextMeshProUGUI>().text =
            string.Format(tickerLayout, closePrice, closePrice.ToString().Length < 5 ? "\t" : "", priceChange > 0 ? "green" : "red", Math.Abs(priceChange), Math.Abs(priceChange).ToString().Length < 5 ? "\t" : "", percentChange > 0 ? "green" : "red", percentChange);
    }
}
