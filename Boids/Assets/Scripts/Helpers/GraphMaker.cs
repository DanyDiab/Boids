using Unity.VisualScripting;
using UnityEngine;
using XCharts.Runtime;
using CsvHelper;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.IO;

public class Graph : MonoBehaviour {
    [Header("Charts")]
    [SerializeField] BarChart barChart;
    [SerializeField] ScatterChart scatterChart;
    [SerializeField] LineChart performanceComparisonChart; 
    [SerializeField] LineChart gridOptimalParameterChart;
    [SerializeField] LineChart quadtreeOptimalParameterChart;
    [SerializeField] LineChart gridDistanceChecksChart;
    [SerializeField] LineChart quadtreeDistanceChecksChart;
    
    [Header("Parameter Impact Charts")]
    [SerializeField] LineChart gridParameterImpactChart;
// this is a test comment
    [SerializeField] LineChart quadtreeParameterImpactChart;

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
        
        // Generate charts if their references are set
        TryGenerate(barChart, generatePerformanceBar);
        TryGenerate(scatterChart, generatePerformanceScatter);
        TryGenerate(performanceComparisonChart, generatePerformanceComparisonLine);
        
        TryGenerate(gridOptimalParameterChart, chart => generateOptimalParameterChart(
            chart, "Uniform Grid: Optimal Cell Size", "Number of Boids vs Optimal Cell Size", SearchAlgos.UNIFORMGRID, "Optimal Cell Size"));
            
        TryGenerate(quadtreeOptimalParameterChart, chart => generateOptimalParameterChart(
            chart, "Quadtree: Optimal Leaf Capacity", "Number of Boids vs Optimal Leaf Capacity", SearchAlgos.QUADTREE, "Optimal Leaf Capacity"));

        TryGenerate(gridDistanceChecksChart, chart => generateDistanceChecksChart(
            chart, "Uniform Grid: Average Checks Per Boid", "Number of Boids vs Average Checks", SearchAlgos.UNIFORMGRID));
            
        TryGenerate(quadtreeDistanceChecksChart, chart => generateDistanceChecksChart(
            chart, "Quadtree: Average Checks Per Boid", "Number of Boids vs Average Checks", SearchAlgos.QUADTREE));

        TryGenerate(gridParameterImpactChart, chart => generateParameterImpactChart(
            chart, "Uniform Grid: Parameter Impact", "Cell Size vs Execution Time", SearchAlgos.UNIFORMGRID, "Cell Size"));

        TryGenerate(quadtreeParameterImpactChart, chart => generateParameterImpactChart(
            chart, "Quadtree: Parameter Impact", "Leaf Capacity vs Execution Time", SearchAlgos.QUADTREE, "Leaf Capacity"));
    }

    List<ExperimentRecord> getCsvData() {
        List<ExperimentRecord> records;
        using (StreamReader reader = new StreamReader(resultsPath))
        using (CsvReader csv = new CsvReader(reader, CultureInfo.InvariantCulture)) {
             records = csv.GetRecords<ExperimentRecord>().ToList();
        }
        return records;
    }

    // --- Helpers ---

    void TryGenerate<T>(T chart, System.Action<T> generateAction) where T : BaseChart {
        if (chart == null) return;
        
        chart.gameObject.SetActive(true);
        chart.AnimationEnable(false);
        generateAction(chart);
    }

    Dictionary<int, ExperimentRecord> GetOptimalRecords(SearchAlgos algo) {
        Dictionary<int, ExperimentRecord> optimalRecords = new Dictionary<int, ExperimentRecord>();
        
        foreach (ExperimentRecord record in experimentRecords) {
            if (record.searchAlgo != algo) continue;

            int boids = record.numBoids;
            if (!optimalRecords.ContainsKey(boids) || record.averageTotalMS < optimalRecords[boids].averageTotalMS) {
                optimalRecords[boids] = record;
            }
        }
        return optimalRecords;
    }

    void SetupChartBasic(BaseChart chart, string title, string subTitle) {
        chart.RemoveData(); // Completely removes all existing series and data from the prefab
        Title titleComponent = chart.EnsureChartComponent<Title>();
        titleComponent.show = true;
        titleComponent.text = title;
        titleComponent.subText = subTitle;
    }

    void SetupLineChart(LineChart chart, string title, string subTitle, string xAxisName, string yAxisName, string tooltipFormatter) {
        SetupChartBasic(chart, title, subTitle);
        
        Legend legend = chart.EnsureChartComponent<Legend>();
        legend.show = true;
        legend.location.align = Location.Align.TopRight;

        XAxis xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Value;
        xAxis.axisName.show = true;
        xAxis.axisName.name = xAxisName;

        YAxis yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        yAxis.axisName.show = true;
        yAxis.axisName.name = yAxisName;

        Tooltip tooltip = chart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;
        tooltip.itemFormatter = tooltipFormatter;
    }

    // --- Generators ---

    void generatePerformanceBar(BarChart chart) {
        SetupChartBasic(chart, "Optimal Averages", "Averages across the optimal settings for various map sizes");
        
        chart.AddSerie<Bar>("Optimal Averages"); // Explicitly add the series after wiping
        
        // Loop over the specific search algorithms to find the optimal averages
        foreach (SearchAlgos algo in System.Enum.GetValues(typeof(SearchAlgos))) {
            Dictionary<int, ExperimentRecord> optimalRecords = GetOptimalRecords(algo);
            if (optimalRecords.Count == 0) continue;

            float sum = 0f;
            foreach (ExperimentRecord record in optimalRecords.Values) {
                sum += record.averageTotalMS;
            }
            float averageOptimalMS = sum / optimalRecords.Count;
            
            chart.AddXAxisData(algo.ToString());
            chart.AddData(0, averageOptimalMS);
        }
    }

    void generatePerformanceScatter(ScatterChart chart) {
        SetupChartBasic(chart, "Scatter Chart", "TEST");
        
        Legend legend = chart.EnsureChartComponent<Legend>();
        legend.show = true;
        legend.location.align = Location.Align.TopRight;
        
        chart.AddSerie<Scatter>("Uniform Grid"); // Explicitly add instead of using GetSerie
        chart.AddSerie<Scatter>("Quadtree");

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
        
        foreach(ExperimentRecord record in experimentRecords) {
            int seriesIndex = record.searchAlgo == SearchAlgos.UNIFORMGRID ? 0 : 1;
            chart.AddData(seriesIndex, record.averageDensity, record.averageTotalMS, record.leafCapacityOrCellSize);
        }
    }

    void generatePerformanceComparisonLine(LineChart chart) {
        SetupLineChart(chart, "Algorithm Performance", "Number of Boids vs Average Total MS", "Number of Boids", "Average Total MS", "{a} <br/> Boids: {c0} <br/> Time: {c1:f2} ms");

        chart.AddSerie<Line>("Uniform Grid");
        chart.AddSerie<Line>("Quadtree");
        chart.AddSerie<Line>("Brute Force");

        Dictionary<int, ExperimentRecord> gridBest = GetOptimalRecords(SearchAlgos.UNIFORMGRID);
        Dictionary<int, ExperimentRecord> qtBest = GetOptimalRecords(SearchAlgos.QUADTREE);
        Dictionary<int, ExperimentRecord> bfBest = GetOptimalRecords(SearchAlgos.BF);

        HashSet<int> allBoids = new HashSet<int>(gridBest.Keys);
        allBoids.UnionWith(qtBest.Keys);
        allBoids.UnionWith(bfBest.Keys);

        List<int> sortedBoids = allBoids.ToList();
        sortedBoids.Sort();

        foreach(int boids in sortedBoids) {
            chart.AddData(0, boids, gridBest.ContainsKey(boids) ? gridBest[boids].averageTotalMS : 0); 
            chart.AddData(1, boids, qtBest.ContainsKey(boids) ? qtBest[boids].averageTotalMS : 0); 
            chart.AddData(2, boids, bfBest.ContainsKey(boids) ? bfBest[boids].averageTotalMS : 0); 
        }
    }

    void generateOptimalParameterChart(LineChart chart, string title, string subTitle, SearchAlgos algo, string yAxisName) {
        string paramType = algo == SearchAlgos.UNIFORMGRID ? "Size" : "Capacity";
        SetupLineChart(chart, title, subTitle, "Number of Boids", yAxisName, "{a} <br/> Boids: {c0} <br/> Optimal " + paramType + ": {c1}");
        
        string serieName = algo == SearchAlgos.UNIFORMGRID ? "Uniform Grid" : "Quadtree";
        chart.AddSerie<Line>(serieName);

        Dictionary<int, ExperimentRecord> optimalRecords = GetOptimalRecords(algo);
        List<int> sortedBoids = optimalRecords.Keys.ToList();
        sortedBoids.Sort();

        foreach(int boids in sortedBoids) {
            chart.AddData(0, boids, optimalRecords[boids].leafCapacityOrCellSize); 
        }
    }

    void generateDistanceChecksChart(LineChart chart, string title, string subTitle, SearchAlgos algo) {
        SetupLineChart(chart, title, subTitle, "Number of Boids", "Average Checks Per Boid", "{a} <br/> Boids: {c0} <br/> Checks: {c1:f2}");
        
        string serieName = algo == SearchAlgos.UNIFORMGRID ? "Uniform Grid" : "Quadtree";
        chart.AddSerie<Line>(serieName);

        Dictionary<int, ExperimentRecord> optimalRecords = GetOptimalRecords(algo);
        List<int> sortedBoids = optimalRecords.Keys.ToList();
        sortedBoids.Sort();

        foreach(int boids in sortedBoids) {
            chart.AddData(0, boids, optimalRecords[boids].averageChecksPerBoid); 
        }
    }

    void generateParameterImpactChart(LineChart chart, string title, string subTitle, SearchAlgos algo, string xAxisName) {
        int maxBoids = experimentRecords.Max(r => r.numBoids);
        
        SetupLineChart(chart, title, subTitle + $" (High-Density Stress Test: {maxBoids} Boids)", xAxisName, "Average Total MS", "{a} <br/> " + xAxisName + ": {c0} <br/> Time: {c1:f2} ms");

        var highestDensityRecords = experimentRecords
            .Where(r => r.searchAlgo == algo && r.numBoids == maxBoids)
            .OrderBy(r => r.leafCapacityOrCellSize)
            .ToList();

        if (highestDensityRecords.Count == 0) return;

        string serieName = $"{maxBoids} Boids";
        chart.AddSerie<Line>(serieName);

        foreach (var record in highestDensityRecords) {
            chart.AddData(0, record.leafCapacityOrCellSize, record.averageTotalMS);
        }x
    }
}