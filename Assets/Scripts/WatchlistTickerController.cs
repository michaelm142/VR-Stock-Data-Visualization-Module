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
    private float removeTimer = 1.0f;

    private string tickerLayout = "Open\t\tHigh\t\tLow\t\tClose\n${0}\t{1}${2}\t{3}${4}\t{5}${6}";

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
        Stonk? stonk = Stonks.Get(symbol.ToUpper());
        if (stonk == null)
            return;

        Stonket stonket = Stonks.Get(stonk.Value, System.DateTime.Now, System.DateTime.Now)[0];
        string openPrice = System.Math.Round(stonket.OpeningPrice, 2).ToString();
        string highPrice = System.Math.Round(stonket.HighPrice, 2).ToString();
        string lowPrice = System.Math.Round(stonket.LowPrice, 2).ToString();
        string closePrice = System.Math.Round(stonket.ClosingPrice, 2).ToString();
        ticker.transform.Find("Ticker/Text").GetComponent<TextMeshProUGUI>().text =
            string.Format(tickerLayout,
            openPrice, openPrice.Length < 5 ? "\t" : "",
            highPrice, highPrice.Length < 5 ? "\t" : "",
            lowPrice, lowPrice.Length < 5 ? "\t" : "",
            closePrice, closePrice.Length < 5 ? "\t" : "");
    }
}
