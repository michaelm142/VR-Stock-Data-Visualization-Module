using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class GraphLineController : MonoBehaviour
{
    public string filename;

    public AnimationCurve dataCurve;

    public GridLineController grid;

    private LineRenderer line;

    // Start is called before the first frame update
    void Start()
    {
        line = GetComponent<LineRenderer>();

        List<float> fileData = new List<float>();
        using (StreamReader file = new StreamReader(new FileStream(filename, FileMode.Open)))
        {
            while (!file.EndOfStream)
            {
                string line = file.ReadLine();
                line = line.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries)[0];
                float value = Convert.ToSingle(line);
                fileData.Add(value);
            }
        }

        for (int i = 0; i < fileData.Count; i++)
        {
            dataCurve.AddKey(i, fileData[i]);
        }

        grid.Validating.AddListener(Validate);

        Validate();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Validate()
    {
        Rect rect = GetComponent<RectTransform>().rect;
        List<Vector3> positions = new List<Vector3>();
        for (float x = 0, i = grid.MinValue.x; x < rect.width; x+=grid.WidthStep, i++)
        {
            float value = (Mathf.Clamp(dataCurve.Evaluate(i), grid.MinValue.y, grid.MaxValue.y) - grid.MinValue.y) / grid.HeightDifference * rect.height;
            positions.Add(Vector3.up * value + Vector3.right * x);
        }
        line.positionCount = positions.Count;
        line.SetPositions(positions.ToArray()); 
    }
}
