using Unity.VisualScripting;
using UnityEngine;
using XCharts.Runtime;
using CsvHelper;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;

public class Graph : MonoBehaviour{
    [Header("Charts")]
    [SerializeField] BarChart barChart;
    [SerializeField] ScatterChart scatterChart;
    string resultsPath;
    string fileDir;
    List<ExperimentRecord> experimentRecords;
    
    void Start() {
        fileDir = Application.dataPath + "/Scripts/Results";
        resultsPath = $"{fileDir}/ExperimentResults.csv";
        experimentRecords = getCsvData();
        generatePerformanceBar(barChart, "Optimal Averages", "Averages across the optimal settings for various map sizes");
        generatePerformanceScatter(scatterChart, "Scatter Chart", "TEST");
    }

    List<ExperimentRecord> getCsvData() {
        List<ExperimentRecord> records;
        using (var reader = new StreamReader(resultsPath))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)){
             records = csv.GetRecords<ExperimentRecord>().ToList();
        }
        return records;
    }


    void addTitleToChart(BaseChart chart, string title, string subTitle){
        var titleComponent = chart.EnsureChartComponent<Title>();
        titleComponent.show = true;
        titleComponent.text = title;
        titleComponent.subText = subTitle;
    }

    void enableLegend(BaseChart chart) {
        Legend legend = chart.EnsureChartComponent<Legend>();
        legend.show = true;
        legend.location.align = Location.Align.TopRight;
    }
    void generatePerformanceBar(BarChart barChart, string title, string subTitle) {
        barChart.ClearData();
        addTitleToChart(barChart,title,subTitle);
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



    void generatePerformanceScatter(ScatterChart chart, string title, string subTitle) {
        addTitleToChart(chart, title, subTitle);
        enableLegend(chart);
        
        Serie uniformGrid = chart.GetSerie(0);
        uniformGrid.serieName = "Uniform Grid";
        
        Serie quadTree = chart.GetSerie(1);
        quadTree.serieName = "Quadtree";

        XAxis xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Value;
        xAxis.axisName.show = true;
        xAxis.axisName.name = "Average Density";

        YAxis yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        yAxis.axisName.show = true;
        yAxis.axisName.name = "Average Total MS";
        
        Tooltip tooltip = chart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;
        
        tooltip.itemFormatter = "{a} <br/> Parameter Size: {c2} <br/> Density: {c0:f2} <br/> Total MS: {c1:f2}";
        
        chart.ClearData();
        
        foreach(ExperimentRecord record in experimentRecords) {
            int seriesIndex = record.searchAlgo == SearchAlgos.UNIFORMGRID ? 0 : 1;
            
            chart.AddData(seriesIndex, record.averageDensity, record.averageTotalMS, record.leafCapacityOrCellSize);
        }
    }
}
