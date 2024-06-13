using FinanceModule;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StockGraphManager : MonoBehaviour
{
    public GameObject stockGraphPrefab;

    // Start is called before the first frame update
    void Start()
    {
        Stonks.Load();
        CreateStockGraph("AMC", Vector3.zero);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateStockGraph(string symbol, Vector3 position)
    {
        Stonk? s = Stonks.Get(symbol);
        if (s == null)
        {
            Debug.LogError("Unable to get stock from Yahoo");
            return;
        }
        Stonk stonk = s.Value;

        Debug.Log("Downloaded data for " + stonk.CompanyName);

        // create canvas
        GameObject stockGraph = new GameObject(string.Format("Stock Graph for {0}", symbol), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = stockGraph.GetComponent<Canvas>();
        canvas.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 5.0f);
        canvas.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 5.0f);
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        canvas.referencePixelsPerUnit = 1;
        canvas.sortingOrder = -100;
        stockGraph.transform.position = position;
        CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
        scaler.referencePixelsPerUnit = 1;

        // create graph element
        GameObject graphElement = Instantiate(stockGraphPrefab, stockGraph.transform);
        graphElement.transform.Find("Controls/StockSymbol").GetComponent<TMPro.TMP_InputField>().text = stonk.Symbol;
        graphElement.transform.Find("StockInfo/Title/StockName").GetComponent<TextMeshProUGUI>().text = stonk.CompanyName;

        // download historical data
        GridLineController grid = graphElement.transform.Find("StockInfo/Graph/GridLines").GetComponent<GridLineController>();
        stonk.HistoricalData = Stonks.Get(stonk, System.DateTime.Now.AddDays(-(grid.MaxValue.x - grid.MinValue.x)), System.DateTime.Now);
        Debug.Log(string.Format("Downloading data from {0} to {1}", System.DateTime.Now.AddDays(-(grid.MaxValue.x - grid.MinValue.x)), System.DateTime.Now));
        List<float>[] boxAndWhiskerData = new List<float>[4]
        {
            new List<float>(),
            new List<float>(),
            new List<float>(),
            new List<float>()
        };
        List<float> lineData = new List<float>();
        for (int i = 0; i < stonk.HistoricalData.Count; i++)
        {
            Stonket stonket = stonk.HistoricalData[i];
            boxAndWhiskerData[0].Add((float)stonket.OpeningPrice);
            boxAndWhiskerData[1].Add((float)stonket.LowPrice);
            boxAndWhiskerData[2].Add((float)stonket.HighPrice);
            boxAndWhiskerData[3].Add((float)stonket.ClosingPrice);
            lineData.Add((float)stonket.ClosingPrice);
        }

        graphElement.transform.Find("StockInfo/Graph/BoxAndWhiskerPlot").GetComponent<BoxAndWhiskerLineController>().SetData(boxAndWhiskerData);
        graphElement.transform.Find("StockInfo/Graph/Line").GetComponent<GraphLineController>().SetData(lineData);
    }
}
