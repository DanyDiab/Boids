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
    [SerializeField] LineChart lineChart;
    
    string resultsPath;
    string fileDir;
    List<ExperimentRecord> experimentRecords;
    
    void Start() {
        fileDir = Application.dataPath + "/Scripts/Results";
        resultsPath = $"{fileDir}/ExperimentResults.csv";
        experimentRecords = getCsvData();
        
        // generatePerformanceBar(barChart, "Optimal Averages", "Averages across the optimal settings for various map sizes");
        // generatePerformanceScatter(scatterChart, "Scatter Chart", "TEST");
        generateOptimalParameterLine(lineChart, "Optimal Parameter Sizes", "Total MS vs Cell Size / Leaf Capacity");
    }

    List<ExperimentRecord> getCsvData() {
        List<ExperimentRecord> records;
        using (StreamReader reader = new StreamReader(resultsPath))
        using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture)){
             records = csv.GetRecords<ExperimentRecord>().ToList();
        }
        return records;
    }

    void addTitleToChart(BaseChart chart, string title, string subTitle){
        Title titleComponent = chart.EnsureChartComponent<Title>();
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
        addTitleToChart(barChart, title, subTitle);
        
        Dictionary<(SearchAlgos, float), float> minTimes = new Dictionary<(SearchAlgos, float), float>();
        
        foreach(ExperimentRecord run in experimentRecords) {
            (SearchAlgos, float) key = (run.searchAlgo, run.mapSize);
            if(!minTimes.ContainsKey(key)) {
                minTimes[key] = run.averageTotalMS;
            } else if (run.averageTotalMS < minTimes[key]) {
                minTimes[key] = run.averageTotalMS;
            }
        }
        
        Dictionary<SearchAlgos, List<float>> algoOptimalTimes = new Dictionary<SearchAlgos, List<float>>();
        
        foreach(KeyValuePair<(SearchAlgos, float), float> kvp in minTimes) {
            SearchAlgos algo = kvp.Key.Item1;
            if(!algoOptimalTimes.ContainsKey(algo)) {
                algoOptimalTimes[algo] = new List<float>();
            }
            algoOptimalTimes[algo].Add(kvp.Value);
        }
        
        foreach(KeyValuePair<SearchAlgos, List<float>> kvp in algoOptimalTimes) {
            float sum = 0f;
            foreach(float time in kvp.Value) {
                sum += time;
            }
            float averageOptimalMS = sum / kvp.Value.Count;
            
            barChart.AddXAxisData("" + kvp.Key);
            barChart.AddData(0, averageOptimalMS);
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

    void generateOptimalParameterLine(LineChart chart, string title, string subTitle) {
        addTitleToChart(chart, title, subTitle);
        enableLegend(chart);

        chart.RemoveData();
        Serie uniformGrid = chart.AddSerie<Line>("Uniform Grid");
        Serie quadTree = chart.AddSerie<Line>("Quadtree");

        // Set X-Axis to Log scale
        XAxis xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Log; 
        xAxis.axisName.show = true;
        xAxis.axisName.name = "Cell Size / Leaf Capacity";
        xAxis.data.Clear(); 

        YAxis yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        yAxis.axisName.show = true;
        yAxis.axisName.name = "Average Total MS";

        Tooltip tooltip = chart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;
        tooltip.itemFormatter = "{a} <br/> Parameter Size: {c0} <br/> Total MS: {c1:f2}";

        Dictionary<(SearchAlgos, float), List<float>> groupedTimes = new Dictionary<(SearchAlgos, float), List<float>>();

        foreach(ExperimentRecord record in experimentRecords) {
            (SearchAlgos, float) key = (record.searchAlgo, record.leafCapacityOrCellSize);
            if(!groupedTimes.ContainsKey(key)) {
                groupedTimes[key] = new List<float>();
            }
            groupedTimes[key].Add(record.averageTotalMS);
        }

        List<(SearchAlgos, float, float)> averagedData = new List<(SearchAlgos, float, float)>();

        foreach(KeyValuePair<(SearchAlgos, float), List<float>> kvp in groupedTimes) {
            float sum = 0f;
            foreach(float time in kvp.Value) {
                sum += time;
            }
            float average = sum / kvp.Value.Count;
            averagedData.Add((kvp.Key.Item1, kvp.Key.Item2, average));
        }

        averagedData = averagedData.OrderBy(item => item.Item2).ToList();

        foreach((SearchAlgos algo, float paramSize, float avgTime) in averagedData) {
            int seriesIndex = algo == SearchAlgos.UNIFORMGRID ? 0 : 1;
            chart.AddData(seriesIndex, paramSize, avgTime); 
        }
    }
}