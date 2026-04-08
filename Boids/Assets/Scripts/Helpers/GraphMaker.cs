using Unity.VisualScripting;
using UnityEngine;
using XCharts.Runtime;
using CsvHelper;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;
using System.Collections; 

public class Graph : MonoBehaviour {
    [Header("Charts")]
    [SerializeField] BarChart barChart;
    [SerializeField] ScatterChart scatterChart;
    [SerializeField] LineChart performanceComparisonChart; 
    [SerializeField] LineChart gridOptimalParameterChart;
    [SerializeField] LineChart quadtreeOptimalParameterChart;
    
    [Header("Chart Toggles")]
    [SerializeField] bool showBarChart = true;
    [SerializeField] bool showScatterChart = true;
    [SerializeField] bool showPerformanceComparison = true;
    [SerializeField] bool showGridOptimal = true;
    [SerializeField] bool showQuadtreeOptimal = true;

    [Header("Save Settings")]
    [SerializeField] string saveDirectory = "Assets/Scripts/Results/Graphs";

    string resultsPath;
    string fileDir;
    List<ExperimentRecord> experimentRecords;
    
    void Start() {

        fileDir = Application.dataPath + "/Scripts/Results";
        resultsPath = $"{fileDir}/ExperimentResults.csv";
        experimentRecords = getCsvData();

        if (!Directory.Exists(saveDirectory)) {
            Directory.CreateDirectory(saveDirectory);
        }
        
        // 1. Generate the charts
        if (showBarChart && barChart != null) {
            barChart.AnimationEnable(false);
            barChart.gameObject.SetActive(true);
            generatePerformanceBar(barChart, "Optimal Averages", "Averages across the optimal settings for various map sizes");
        } else if (barChart != null) {
            barChart.gameObject.SetActive(false);
        }

        if (showScatterChart && scatterChart != null) {
            scatterChart.gameObject.SetActive(true);
            scatterChart.AnimationEnable(false);
            generatePerformanceScatter(scatterChart, "Scatter Chart", "TEST");
        } else if (scatterChart != null) {
            scatterChart.gameObject.SetActive(false);
        }

        if (showPerformanceComparison && performanceComparisonChart != null) {
            performanceComparisonChart.gameObject.SetActive(true);
            performanceComparisonChart.AnimationEnable(false);
            generatePerformanceComparisonLine(performanceComparisonChart, "Algorithm Performance", "Number of Boids vs Average Total MS");
        } else if (performanceComparisonChart != null) {
            performanceComparisonChart.gameObject.SetActive(false);
        }

        if (showGridOptimal && gridOptimalParameterChart != null) {
            gridOptimalParameterChart.gameObject.SetActive(true);
            gridOptimalParameterChart.AnimationEnable(false);

            generateGridOptimalParameterByBoidsLine(gridOptimalParameterChart, "Uniform Grid: Optimal Cell Size", "Number of Boids vs Optimal Cell Size");
        } else if (gridOptimalParameterChart != null) {
            gridOptimalParameterChart.gameObject.SetActive(false);
        }

        if (showQuadtreeOptimal && quadtreeOptimalParameterChart != null) {
            quadtreeOptimalParameterChart.gameObject.SetActive(true);
            quadtreeOptimalParameterChart.AnimationEnable(false);
            generateQuadtreeOptimalParameterByBoidsLine(quadtreeOptimalParameterChart, "Quadtree: Optimal Leaf Capacity", "Number of Boids vs Optimal Leaf Capacity");
        } else if (quadtreeOptimalParameterChart != null) {
            quadtreeOptimalParameterChart.gameObject.SetActive(false);
        }
    }

    List<ExperimentRecord> getCsvData() {
        List<ExperimentRecord> records;
        using (StreamReader reader = new StreamReader(resultsPath))
        using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture)) {
             records = csv.GetRecords<ExperimentRecord>().ToList();
        }
        return records;
    }

    void addTitleToChart(BaseChart chart, string title, string subTitle) {
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
            (SearchAlgos, float) key = (run.searchAlgo, run.numBoids);
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

    void generatePerformanceComparisonLine(LineChart chart, string title, string subTitle) {
        addTitleToChart(chart, title, subTitle);
        enableLegend(chart);

        chart.RemoveData();
        chart.AddSerie<Line>("Uniform Grid");
        chart.AddSerie<Line>("Quadtree");
        chart.AddSerie<Line>("Brute Force");

        XAxis xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Value; 
        xAxis.axisName.show = true;
        xAxis.axisName.name = "Number of Boids";

        YAxis yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        yAxis.axisName.show = true;
        yAxis.axisName.name = "Average Total MS";

        Tooltip tooltip = chart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;
        tooltip.itemFormatter = "{a} <br/> Boids: {c0} <br/> Time: {c1:f2} ms";

        Dictionary<int, float> gridBest = new Dictionary<int, float>();
        Dictionary<int, float> qtBest = new Dictionary<int, float>();
        Dictionary<int, float> bfTimes = new Dictionary<int, float>();

        foreach(ExperimentRecord record in experimentRecords) {
            int boids = record.numBoids;
            
            if (record.searchAlgo == SearchAlgos.UNIFORMGRID) {
                if (!gridBest.ContainsKey(boids) || record.averageTotalMS < gridBest[boids])
                    gridBest[boids] = record.averageTotalMS;
            } 
            else if (record.searchAlgo == SearchAlgos.QUADTREE) {
                if (!qtBest.ContainsKey(boids) || record.averageTotalMS < qtBest[boids])
                    qtBest[boids] = record.averageTotalMS;
            } 
            else if (record.searchAlgo == SearchAlgos.BF) {
                bfTimes[boids] = record.averageTotalMS;
            }
        }

        List<int> sortedBoids = bfTimes.Keys.ToList();
        sortedBoids.Sort();

        foreach(int boids in sortedBoids) {
            chart.AddData(0, boids, gridBest.ContainsKey(boids) ? gridBest[boids] : 0); 
            chart.AddData(1, boids, qtBest.ContainsKey(boids) ? qtBest[boids] : 0); 
            chart.AddData(2, boids, bfTimes.ContainsKey(boids) ? bfTimes[boids] : 0); 
        }
    }

    void generateGridOptimalParameterByBoidsLine(LineChart chart, string title, string subTitle) {
        addTitleToChart(chart, title, subTitle);
        enableLegend(chart);

        chart.RemoveData();
        chart.AddSerie<Line>("Uniform Grid");

        XAxis xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Value; 
        xAxis.axisName.show = true;
        xAxis.axisName.name = "Number of Boids";

        YAxis yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        yAxis.axisName.show = true;
        yAxis.axisName.name = "Optimal Cell Size";

        Tooltip tooltip = chart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;
        tooltip.itemFormatter = "{a} <br/> Boids: {c0} <br/> Optimal Size: {c1}";

        Dictionary<int, float> optimalSizes = new Dictionary<int, float>();
        Dictionary<int, float> lowestTimes = new Dictionary<int, float>();

        foreach(ExperimentRecord record in experimentRecords) {
            if(record.searchAlgo != SearchAlgos.UNIFORMGRID) continue;

            int boids = record.numBoids;
            if(!lowestTimes.ContainsKey(boids) || record.averageTotalMS < lowestTimes[boids]) {
                lowestTimes[boids] = record.averageTotalMS;
                optimalSizes[boids] = record.leafCapacityOrCellSize;
            }
        }

        List<int> sortedBoids = optimalSizes.Keys.ToList();
        sortedBoids.Sort();

        foreach(int boids in sortedBoids) {
            chart.AddData(0, boids, optimalSizes[boids]); 
        }
    }

    void generateQuadtreeOptimalParameterByBoidsLine(LineChart chart, string title, string subTitle) {
        addTitleToChart(chart, title, subTitle);
        enableLegend(chart);

        chart.RemoveData();
        chart.AddSerie<Line>("Quadtree");

        XAxis xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Value; 
        xAxis.axisName.show = true;
        xAxis.axisName.name = "Number of Boids";

        YAxis yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        yAxis.axisName.show = true;
        yAxis.axisName.name = "Optimal Leaf Capacity";

        Tooltip tooltip = chart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;
        tooltip.itemFormatter = "{a} <br/> Boids: {c0} <br/> Optimal Capacity: {c1}";

        Dictionary<int, float> optimalCapacities = new Dictionary<int, float>();
        Dictionary<int, float> lowestTimes = new Dictionary<int, float>();

        foreach(ExperimentRecord record in experimentRecords) {
            if(record.searchAlgo != SearchAlgos.QUADTREE) continue;

            int boids = record.numBoids;
            if(!lowestTimes.ContainsKey(boids) || record.averageTotalMS < lowestTimes[boids]) {
                lowestTimes[boids] = record.averageTotalMS;
                optimalCapacities[boids] = record.leafCapacityOrCellSize;
            }
        }

        List<int> sortedBoids = optimalCapacities.Keys.ToList();
        sortedBoids.Sort();

        foreach(int boids in sortedBoids) {
            chart.AddData(0, boids, optimalCapacities[boids]); 
        }
    }
}