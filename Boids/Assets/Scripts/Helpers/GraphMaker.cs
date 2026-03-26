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
    [SerializeField] LineChart gridParameterPerformanceChart;
    [SerializeField] LineChart quadtreeParameterPerformanceChart;
    [SerializeField] LineChart gridParameterDensityChart;
    [SerializeField] LineChart quadtreeParameterDensityChart;
    
    [Header("Chart Toggles")]
    [SerializeField] bool showBarChart = true;
    [SerializeField] bool showScatterChart = true;
    [SerializeField] bool showGridPerformance = true;
    [SerializeField] bool showQuadtreePerformance = true;
    [SerializeField] bool showGridDensity = true;
    [SerializeField] bool showQuadtreeDensity = true;

    string resultsPath;
    string fileDir;
    List<ExperimentRecord> experimentRecords;
    
    void Start() {
        fileDir = Application.dataPath + "/Scripts/Results";
        resultsPath = $"{fileDir}/ExperimentResults.csv";
        experimentRecords = getCsvData();
        
        if (showBarChart && barChart != null) {
            barChart.gameObject.SetActive(true);
            generatePerformanceBar(barChart, "Optimal Averages", "Averages across the optimal settings for various map sizes");
        } else if (barChart != null) {
            barChart.gameObject.SetActive(false);
        }

        if (showScatterChart && scatterChart != null) {
            scatterChart.gameObject.SetActive(true);
            generatePerformanceScatter(scatterChart, "Scatter Chart", "TEST");
        } else if (scatterChart != null) {
            scatterChart.gameObject.SetActive(false);
        }

        if (showGridPerformance && gridParameterPerformanceChart != null) {
            gridParameterPerformanceChart.gameObject.SetActive(true);
            generateGridParameterPerformanceLine(gridParameterPerformanceChart, "Grid Performance", "Cell Size vs Total MS");
        } else if (gridParameterPerformanceChart != null) {
            gridParameterPerformanceChart.gameObject.SetActive(false);
        }

        if (showQuadtreePerformance && quadtreeParameterPerformanceChart != null) {
            quadtreeParameterPerformanceChart.gameObject.SetActive(true);
            generateQuadtreeParameterPerformanceLine(quadtreeParameterPerformanceChart, "Quadtree Performance", "Leaf Capacity vs Total MS");
        } else if (quadtreeParameterPerformanceChart != null) {
            quadtreeParameterPerformanceChart.gameObject.SetActive(false);
        }

        if (showGridDensity && gridParameterDensityChart != null) {
            gridParameterDensityChart.gameObject.SetActive(true);
            generateGridOptimalParameterByDensityLine(gridParameterDensityChart, "Optimal Grid Cell Size", "Density vs Optimal Cell Size");
        } else if (gridParameterDensityChart != null) {
            gridParameterDensityChart.gameObject.SetActive(false);
        }

        if (showQuadtreeDensity && quadtreeParameterDensityChart != null) {
            quadtreeParameterDensityChart.gameObject.SetActive(true);
            generateQuadtreeOptimalParameterByDensityLine(quadtreeParameterDensityChart, "Optimal Quadtree Leaf Capacity", "Density vs Optimal Capacity");
        } else if (quadtreeParameterDensityChart != null) {
            quadtreeParameterDensityChart.gameObject.SetActive(false);
        }
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

    void generateGridParameterPerformanceLine(LineChart chart, string title, string subTitle) {
        addTitleToChart(chart, title, subTitle);
        enableLegend(chart);

        chart.RemoveData();
        Serie uniformGrid = chart.AddSerie<Line>("Uniform Grid");

        XAxis xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Log; 
        xAxis.axisName.show = true;
        xAxis.axisName.name = "Cell Size";
        xAxis.data.Clear(); 

        YAxis yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        yAxis.axisName.show = true;
        yAxis.axisName.name = "Average Total MS";

        Tooltip tooltip = chart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;
        tooltip.itemFormatter = "{a} <br/> Cell Size: {c0} <br/> <br/> Total MS: {c1:f2}";

        Dictionary<float, List<float>> groupedTimes = new Dictionary<float, List<float>>();

        foreach(ExperimentRecord record in experimentRecords) {
            if(record.searchAlgo != SearchAlgos.UNIFORMGRID) continue;

            float key = record.leafCapacityOrCellSize;
            if(!groupedTimes.ContainsKey(key)) {
                groupedTimes[key] = new List<float>();
            }
            groupedTimes[key].Add(record.averageTotalMS);
        }

        List<(float, float)> averagedData = new List<(float, float)>();

        foreach(KeyValuePair<float, List<float>> kvp in groupedTimes) {
            float sum = 0f;
            foreach(float time in kvp.Value) {
                sum += time;
            }
            float average = sum / kvp.Value.Count;
            averagedData.Add((kvp.Key, average));
        }

        List<(float paramSize, float avgTime)> sortedData = averagedData.OrderBy(item => item.Item1).ToList();

        foreach((float paramSize, float avgTime) in sortedData) {
            chart.AddData(0, paramSize, avgTime); 
        }
    }

    void generateQuadtreeParameterPerformanceLine(LineChart chart, string title, string subTitle) {
        addTitleToChart(chart, title, subTitle);
        enableLegend(chart);

        chart.RemoveData();
        Serie quadTree = chart.AddSerie<Line>("Quadtree");

        XAxis xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Log; 
        xAxis.axisName.show = true;
        xAxis.axisName.name = "Leaf Capacity";
        xAxis.data.Clear(); 

        YAxis yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        yAxis.axisName.show = true;
        yAxis.axisName.name = "Average Total MS";

        Tooltip tooltip = chart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;
        tooltip.itemFormatter = "{a} <br/> Leaf Capacity: {c0} <br/> <br/> Total MS: {c1:f2}";

        Dictionary<float, List<float>> groupedTimes = new Dictionary<float, List<float>>();

        foreach(ExperimentRecord record in experimentRecords) {
            if(record.searchAlgo != SearchAlgos.QUADTREE) continue;

            float key = record.leafCapacityOrCellSize;
            if(!groupedTimes.ContainsKey(key)) {
                groupedTimes[key] = new List<float>();
            }
            groupedTimes[key].Add(record.averageTotalMS);
        }

        List<(float, float)> averagedData = new List<(float, float)>();

        foreach(KeyValuePair<float, List<float>> kvp in groupedTimes) {
            float sum = 0f;
            foreach(float time in kvp.Value) {
                sum += time;
            }
            float average = sum / kvp.Value.Count;
            averagedData.Add((kvp.Key, average));
        }

        List<(float paramSize, float avgTime)> sortedData = averagedData.OrderBy(item => item.Item1).ToList();

        foreach((float paramSize, float avgTime) in sortedData) {
            chart.AddData(0, paramSize, avgTime); 
        }
    }

    void generateGridOptimalParameterByDensityLine(LineChart chart, string title, string subTitle) {
        addTitleToChart(chart, title, subTitle);
        enableLegend(chart);

        chart.RemoveData();
        chart.AddSerie<Line>("Uniform Grid");

        XAxis xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Value; 
        xAxis.axisName.show = true;
        xAxis.axisName.name = "Average Density";

        YAxis yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        yAxis.axisName.show = true;
        yAxis.axisName.name = "Optimal Cell Size";

        Tooltip tooltip = chart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;
        tooltip.itemFormatter = "{a} <br/> Density Bin: {c0:f1} <br/> Optimal Size: {c1}";

        float densityBinSize = 5.0f; 
        
        Dictionary<float, ExperimentRecord> optimalRecords = new Dictionary<float, ExperimentRecord>();

        foreach(ExperimentRecord record in experimentRecords) {
            if(record.searchAlgo != SearchAlgos.UNIFORMGRID) continue;

            float binnedDensity = Mathf.Round(record.averageDensity / densityBinSize) * densityBinSize;
            
            if(!optimalRecords.ContainsKey(binnedDensity)) {
                optimalRecords[binnedDensity] = record;
            } else if (record.averageTotalMS < optimalRecords[binnedDensity].averageTotalMS) {
                optimalRecords[binnedDensity] = record;
            }
        }

        List<ExperimentRecord> sortedOptimalRecords = optimalRecords.Values.OrderBy(record => record.averageDensity).ToList();

        foreach(ExperimentRecord optimalRecord in sortedOptimalRecords) {
            float plotDensity = Mathf.Round(optimalRecord.averageDensity / densityBinSize) * densityBinSize;
            chart.AddData(0, plotDensity, optimalRecord.leafCapacityOrCellSize); 
        }
    }

void generateQuadtreeOptimalParameterByDensityLine(LineChart chart, string title, string subTitle) {
        addTitleToChart(chart, title, subTitle);
        enableLegend(chart);

        chart.RemoveData();
        chart.AddSerie<Line>("Quadtree");

        XAxis xAxis = chart.EnsureChartComponent<XAxis>();
        xAxis.type = Axis.AxisType.Value; 
        xAxis.axisName.show = true;
        xAxis.axisName.name = "Average Density";

        YAxis yAxis = chart.EnsureChartComponent<YAxis>();
        yAxis.type = Axis.AxisType.Value;
        yAxis.axisName.show = true;
        yAxis.axisName.name = "Optimal Leaf Capacity";

        Tooltip tooltip = chart.EnsureChartComponent<Tooltip>();
        tooltip.show = true;
        tooltip.itemFormatter = "{a} <br/> Density Bin: {c0:f1} <br/> Optimal Capacity: {c1}";

        float densityBinSize = 1.0f; 
        
        // Group all records by their density bin
        Dictionary<float, List<ExperimentRecord>> binnedRecords = new Dictionary<float, List<ExperimentRecord>>();

        foreach(ExperimentRecord record in experimentRecords) {
            if(record.searchAlgo != SearchAlgos.QUADTREE) continue;

            float binnedDensity = Mathf.Round(record.averageDensity / densityBinSize) * densityBinSize;
            
            if(!binnedRecords.ContainsKey(binnedDensity)) {
                binnedRecords[binnedDensity] = new List<ExperimentRecord>();
            }
            binnedRecords[binnedDensity].Add(record);
        }

        List<(float density, float optimalCapacity)> smoothedOptimalData = new List<(float, float)>();

        foreach(KeyValuePair<float, List<ExperimentRecord>> binKvp in binnedRecords) {
            float currentDensityBin = binKvp.Key;
            List<ExperimentRecord> recordsInBin = binKvp.Value;

            // Group the records in this specific bin by their Leaf Capacity
            Dictionary<float, List<float>> timesByCapacity = new Dictionary<float, List<float>>();
            
            foreach(ExperimentRecord r in recordsInBin) {
                if (!timesByCapacity.ContainsKey(r.leafCapacityOrCellSize)) {
                    timesByCapacity[r.leafCapacityOrCellSize] = new List<float>();
                }
                timesByCapacity[r.leafCapacityOrCellSize].Add(r.averageTotalMS);
            }

            // Find the capacity with the lowest AVERAGE time to defeat outliers
            float bestCapacity = 0f;
            float lowestAvgMS = float.MaxValue;

            foreach(KeyValuePair<float, List<float>> capacityKvp in timesByCapacity) {
                float sumMS = 0f;
                foreach(float time in capacityKvp.Value) {
                    sumMS += time;
                }
                float avgMS = sumMS / capacityKvp.Value.Count;

                if (avgMS < lowestAvgMS) {
                    lowestAvgMS = avgMS;
                    bestCapacity = capacityKvp.Key;
                }
            }

            smoothedOptimalData.Add((currentDensityBin, bestCapacity));
        }

        List<(float density, float optimalCapacity)> sortedData = smoothedOptimalData.OrderBy(item => item.density).ToList();

        foreach((float density, float optimalCapacity) in sortedData) {
            chart.AddData(0, density, optimalCapacity); 
        }
    }
}