using Unity.VisualScripting;
using UnityEngine;
using XCharts.Runtime;
using CsvHelper;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;

public class Graph : MonoBehaviour{
    [SerializeField] BarChart barChart;
    string resultsPath;
    string fileDir;
    List<ExperimentRecord> experimentRecords;
    
    void Start() {
        fileDir = Application.dataPath + "/Scripts/Results";
        resultsPath = $"{fileDir}/ExperimentResults.csv";
        experimentRecords = getCsvData();
        generatePerformanceChart(barChart, "Optimal Averages", "Averages across the optimal settings for various map sizes");
    }

    List<ExperimentRecord> getCsvData() {
        List<ExperimentRecord> records;
        using (var reader = new StreamReader(resultsPath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)){
             records = csv.GetRecords<ExperimentRecord>().ToList();
        }
        return records;
    }
    void generatePerformanceChart(BarChart barChart, string title, string subTitle) {
        var titleComponent = barChart.EnsureChartComponent<Title>();
        titleComponent.show = true;
        titleComponent.text = title;
        titleComponent.subText = subTitle;
        barChart.ClearData();
        var optimalResults = experimentRecords
            .GroupBy(run => new { run.searchAlgo, run.mapSize })
            .Select(group => new {
                Algorithm = group.Key.searchAlgo,
                MapSize = group.Key.mapSize,
                // Find the fastest time for this specific map size
                BestMS = group.Min(run => run.averageTotalMS) 
            })
            .GroupBy(bestRun => bestRun.Algorithm)
            .Select(group => new {
                Algorithm = group.Key,
                // Average only the optimally tuned runs
                AverageOptimalMS = group.Average(run => run.BestMS) 
            })
            .ToList();
        foreach(var avg in optimalResults) {
            barChart.AddXAxisData("" + avg.Algorithm);
            barChart.AddData(0, avg.AverageOptimalMS);
        }
    }
}
