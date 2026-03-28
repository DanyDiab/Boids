using UnityEngine;
using System.Collections.Generic;
using VectorShapes;
using QuadTree;
using UnityEngine.InputSystem;

[RequireComponent(typeof(ShapeRenderer))]
public class GridVisualizer : MonoBehaviour {
    [Header("Simulation Reference")]
    [SerializeField] SimulationParameters simParams;
    [SerializeField] BoidManager boidManager;
    [SerializeField] BoidInfo boidInfo;
    
    [Header("Visualization Settings")]
    [SerializeField] int currentDepth = 0;
    public float strokeWidth = 2f;
    public float lineStrokeWidth = 4f;
    public Color strokeColor = Color.green;
    public StrokeRenderType strokeRenderType = StrokeRenderType.ScreenSpacePixels;
    public float yOffset = 0.5f;

    // Cached values from simParams
    float cachedSimBoundRadius;
    int cachedCellSize;
    SearchAlgos cachedSearchAlgo;
    GizmoStruct cachedGizmoStruct;

    // Object pool for performance (so we don't recreate shapes every frame)
    List<GameObject> shapePool = new List<GameObject>();
    int activeShapeCount = 0;

    // Separate pool for lines to show distance checks in Game View
    List<GameObject> linePool = new List<GameObject>();
    int activeLineCount = 0;

    // Lerp state for visualization
    float currentLerpFactor = 0f;
    const float minTimeScale = 0.05f;
    Camera mainCam;
    float defaultOrthographicSize;
    Vector3 defaultCameraPosition;

    void Start() {
        mainCam = Camera.main;
        if (mainCam != null) {
            defaultOrthographicSize = mainCam.orthographicSize;
            defaultCameraPosition = mainCam.transform.position;
        }
    }

    void OnEnable() {
        BoidManager.OnBoidSpawn += RefreshParameters;
        // Initial refresh
        if (simParams != null) RefreshParameters();
    }

    void OnDisable() {
        BoidManager.OnBoidSpawn -= RefreshParameters;
        Time.timeScale = 1f; // Reset time scale on disable
    }

    void RefreshParameters() {
        if (simParams == null) return;
        cachedSimBoundRadius = simParams.SimBoundRadius;
        cachedCellSize = simParams.CellSize;
        cachedSearchAlgo = simParams.CurrSearchAlgo;
        cachedGizmoStruct = simParams.GizmoStruct;

        if (simParams.TargetBoidID < 0 || (boidManager != null && boidManager.boids != null && simParams.TargetBoidID >= boidManager.boids.Length)) {
            simParams.TargetBoidID = 0;
        }

        if (mainCam != null) {
            defaultOrthographicSize = (cachedSimBoundRadius / 2) + 20;
            // Also reset position if it was zoomed
            if (!simParams.IsVisualizingSearch) {
                 mainCam.transform.position = defaultCameraPosition;
                 mainCam.orthographicSize = defaultOrthographicSize;
                 currentLerpFactor = 0;
            }
        }
    }

    void Update() {
        if (Keyboard.current == null) return;

        // Tab: toggles search visualization mode
        if (Keyboard.current.tabKey.wasPressedThisFrame) {
            simParams.IsVisualizingSearch = !simParams.IsVisualizingSearch;
        }

        // V key handling
        if (Keyboard.current.vKey.wasPressedThisFrame) {
            if (simParams.IsVisualizingSearch) {
                // Cycle boids when in search visualization
                simParams.TargetBoidID = (simParams.TargetBoidID + 1) % simParams.NumBoids;
            } else {
                // Cycle QuadTree depth normally
                currentDepth++;
            }
        }
        
        // R: Reset visualization to root node
        if (Keyboard.current.rKey.wasPressedThisFrame) {
            currentDepth = 0;
            if (simParams.IsVisualizingSearch) simParams.TargetBoidID = 0;
        }

        HandleVisualizationLerps();
    }

    void HandleVisualizationLerps() {
        if (simParams == null || mainCam == null) return;

        float targetFactor = simParams.IsVisualizingSearch ? 1f : 0f;
        currentLerpFactor = Mathf.MoveTowards(currentLerpFactor, targetFactor, Time.unscaledDeltaTime * simParams.SearchVisLerpSpeed);

        // Lerp TimeScale
        Time.timeScale = Mathf.Lerp(1f, minTimeScale, currentLerpFactor);

        // Lerp Camera
        if (currentLerpFactor > 0 && cachedSearchAlgo != SearchAlgos.BF) {
            Boid targetBoid = GetTargetBoid();
            if (targetBoid != null) {
                Vector3 targetPos = targetBoid.transform.position;
                Vector3 cameraTargetPos = new Vector3(targetPos.x, defaultCameraPosition.y, targetPos.z);
                
                mainCam.transform.position = Vector3.Lerp(defaultCameraPosition, cameraTargetPos, currentLerpFactor);
                mainCam.orthographicSize = Mathf.Lerp(defaultOrthographicSize, simParams.ZoomOrthographicSize, currentLerpFactor);
            }
        } else if (currentLerpFactor == 0 && !simParams.IsVisualizingSearch || (currentLerpFactor > 0 && cachedSearchAlgo == SearchAlgos.BF)) {
            // Only snap back when fully returned to 0 or stay at default if BF
            if (cachedSearchAlgo == SearchAlgos.BF) {
                 mainCam.transform.position = defaultCameraPosition;
                 mainCam.orthographicSize = defaultOrthographicSize;
            } else if (currentLerpFactor == 0) {
                 mainCam.transform.position = defaultCameraPosition;
                 mainCam.orthographicSize = defaultOrthographicSize;
            }
        }
    }

    Boid GetTargetBoid() {
        if (boidManager != null && boidManager.boids != null && 
            simParams.TargetBoidID >= 0 && simParams.TargetBoidID < boidManager.boids.Length) {
            return boidManager.boids[simParams.TargetBoidID];
        }
        return null;
    }

    void LateUpdate() {
        // Draw the visualization continuously so it matches the real-time simulation
        DrawVisualization();
    }

    public void DrawVisualization() {
        activeShapeCount = 0;
        activeLineCount = 0;

        if (simParams == null) return;

        if (cachedSearchAlgo == SearchAlgos.QUADTREE) {
            if (simParams.Nodes != null && simParams.Nodes.Count > 0) {
                DrawNode(0, new Vector2(cachedSimBoundRadius, cachedSimBoundRadius), new Vector2(-cachedSimBoundRadius / 2f, -cachedSimBoundRadius / 2f), 0);
            }
        } else if (cachedSearchAlgo == SearchAlgos.UNIFORMGRID) {
            int[] highlightIDs = null;
            if (simParams.IsVisualizingSearch) {
                highlightIDs = GetHighlightCellIDs();
            }
            
            DrawUniformGrid(highlightIDs);

            if (simParams.IsVisualizingSearch && highlightIDs != null) {
                DrawSearchLines(highlightIDs);
            }
        } else if (cachedSearchAlgo == SearchAlgos.BF) {
            if (simParams.IsVisualizingSearch) {
                DrawBFSearchLines();
            }
        }

        // Deactivate unused shapes in the pools
        for (int i = activeShapeCount; i < shapePool.Count; i++) {
            if (shapePool[i].activeSelf) shapePool[i].SetActive(false);
        }
        for (int i = activeLineCount; i < linePool.Count; i++) {
            if (linePool[i].activeSelf) linePool[i].SetActive(false);
        }
    }

    int[] GetHighlightCellIDs() {
        if (boidManager == null || boidManager.search == null) return null;
        UniformGridSearch gridSearch = boidManager.search as UniformGridSearch;
        if (gridSearch == null) return null;

        Boid targetBoid = GetTargetBoid();
        if (targetBoid == null) return null;

        return gridSearch.GetNeighboringCellIDs(targetBoid.transform.position);
    }

    void DrawSearchLines(int[] neighborCellIDs) {
        UniformGridSearch gridSearch = boidManager.search as UniformGridSearch;
        Boid targetBoid = GetTargetBoid();
        if (gridSearch == null || targetBoid == null || boidInfo == null) return;

        Vector3 targetPos = targetBoid.transform.position;

        foreach (int cellID in neighborCellIDs) {
            if (cellID < 0 || cellID >= gridSearch.NumCellsPerRow * gridSearch.NumCellsPerRow) continue;
            
            List<int> candidateIDs = gridSearch.GetBoidIDsInCell(cellID);
            foreach (int candID in candidateIDs) {
                if (candID == simParams.TargetBoidID) continue;
                Boid candBoid = boidManager.boids[candID];
                if (candBoid != null) {
                    CreateOrUpdateLine(targetPos, candBoid.transform.position, simParams.DistanceCheckLineColor);
                }
            }
        }
    }

    void DrawUniformGrid(int[] highlightIDs = null) {
        if (cachedCellSize <= 0) return;

        int numCellsRow = Mathf.Max(1, (int)Mathf.Ceil(cachedSimBoundRadius / cachedCellSize));
        float sizePerCell = cachedSimBoundRadius / numCellsRow;

        for (int i = 0; i < numCellsRow * numCellsRow; i++) {
            int rowNumber = i / numCellsRow; // This is the Z index
            int colNumber = i % numCellsRow; // This is the X index
            float xPos = (-cachedSimBoundRadius / 2f) + (colNumber * sizePerCell) + (sizePerCell / 2f);
            float zPos = (-cachedSimBoundRadius / 2f) + (rowNumber * sizePerCell) + (sizePerCell / 2f);
            Vector3 finalPosition = new Vector3(xPos, yOffset, zPos);
            
            Color? color = null;
            if (highlightIDs != null) {
                for (int j = 0; j < highlightIDs.Length; j++) {
                    if (highlightIDs[j] == i) {
                        color = simParams.HighlightCellColor;
                        break;
                    }
                }
            }

            CreateOrUpdateRectangle(finalPosition, sizePerCell, sizePerCell, color);
        }
    }

    void DrawNode(int index, Vector2 boxDims, Vector2 offset, int depth) {
        if (depth > currentDepth) return;
        
        List<Node> nodes = simParams.Nodes;
        if (index >= nodes.Count || index < 0) return;

        // VectorShapes Rectangles are centered on their transform by default.
        // We calculate the center of the quadrant to align it with the world.
        Vector3 centerPosition = new Vector3(offset.x + (boxDims.x / 2f), yOffset, offset.y + (boxDims.y / 2f));
        
        CreateOrUpdateRectangle(centerPosition, boxDims.x, boxDims.y);

        Node currNode = nodes[index];
        int firstChild = currNode.FirstChild;
        
        if (firstChild != -1 && depth < currentDepth) {
           Vector2 newDims = new Vector2(boxDims.x / 2f, boxDims.y / 2f);
           
           // Calculate the 4 quadrants following SimManager/QuadTreeSearch logic
           Vector2 tl = offset;
           Vector2 tr = new Vector2(offset.x + newDims.x, offset.y);
           Vector2 bl = new Vector2(offset.x, offset.y + newDims.y);
           Vector2 br = new Vector2(offset.x + newDims.x, offset.y + newDims.y);

           DrawNode(firstChild,     newDims, tl, depth + 1);
           DrawNode(firstChild + 1, newDims, tr, depth + 1); 
           DrawNode(firstChild + 2, newDims, bl, depth + 1); 
           DrawNode(firstChild + 3, newDims, br, depth + 1); 
        }
    }

    void CreateOrUpdateRectangle(Vector3 position, float width, float height, Color? overrideColor = null) {
        GameObject obj;
        Shape shape;

        // Fetch from pool or instantiate new
        if (activeShapeCount < shapePool.Count) {
            obj = shapePool[activeShapeCount];
            if (!obj.activeSelf) obj.SetActive(true);
            shape = obj.GetComponent<Shape>();
        } else {
            obj = new GameObject("GridCell_" + activeShapeCount);
            obj.transform.parent = transform;
            // Rotate -90 on X so the local XY plane of the shape lies on the global XZ plane facing UP.
            obj.transform.localRotation = Quaternion.Euler(-90, 0, 0);
            
            shape = obj.AddComponent<Shape>();
            shape.ShapeData.ShapeType = ShapeType.Rectangle;
            shape.ShapeData.IsFillEnabled = false;
            shape.ShapeData.IsStrokeEnabled = true;
            shape.ShapeData.StrokeRenderType = strokeRenderType;
            shape.ShapeData.ShapeOffset = Vector3.zero;
            
            shapePool.Add(obj);
        }

        // Update position and size
        obj.transform.localPosition = position;
        
        // Use override color, then cachedGizmoStruct, then local strokeColor
        Color activeColor = overrideColor ?? ((cachedGizmoStruct.cellColor != Color.clear) ? cachedGizmoStruct.cellColor : strokeColor);

        // Only update ShapeData if values changed to avoid unnecessary mesh rebuilds
        bool needsUpdate = false;
        if (shape.ShapeData.ShapeSize.x != width || shape.ShapeData.ShapeSize.y != height) {
            shape.ShapeData.ShapeSize = new Vector2(width, height);
            needsUpdate = true;
        }
        if (shape.ShapeData.GetStrokeColor() != activeColor) {
            shape.ShapeData.SetStrokeColor(activeColor);
            needsUpdate = true;
        }
        if (shape.ShapeData.GetStrokeWidth() != strokeWidth) {
            shape.ShapeData.SetStrokeWidth(strokeWidth);
            needsUpdate = true;
        }

        if (needsUpdate) {
            shape.ShapeData = shape.ShapeData;
        }

        activeShapeCount++;
    }

    void CreateOrUpdateLine(Vector3 start, Vector3 end, Color color) {
        GameObject obj;
        Shape shape;

        if (activeLineCount < linePool.Count) {
            obj = linePool[activeLineCount];
            if (!obj.activeSelf) obj.SetActive(true);
            shape = obj.GetComponent<Shape>();
        } else {
            obj = new GameObject("DistanceLine_" + activeLineCount);
            obj.transform.parent = transform;
            shape = obj.AddComponent<Shape>();
            shape.ShapeData.ShapeType = ShapeType.Polygon;
            shape.ShapeData.IsPolygonStrokeClosed = false;
            shape.ShapeData.IsFillEnabled = false;
            shape.ShapeData.IsStrokeEnabled = true;
            shape.ShapeData.StrokeRenderType = strokeRenderType;
            linePool.Add(obj);
        }

        // Use world positions (assuming Shape object at zero)
        Vector3 s = new Vector3(start.x, yOffset + 0.1f, start.z);
        Vector3 e = new Vector3(end.x, yOffset + 0.1f, end.z);
        
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;

        // Clear and add the two points
        shape.ShapeData.ClearPolyPoints();
        shape.ShapeData.AddPolyPoint(s);
        shape.ShapeData.AddPolyPoint(e);

        shape.ShapeData.SetStrokeColor(color);
        shape.ShapeData.SetStrokeWidth(lineStrokeWidth);

        // Notify VectorShapes that data changed
        shape.ShapeData = shape.ShapeData;
        
        activeLineCount++;
    }

    void DrawBFSearchLines() {
        IBoidSearch search = boidManager.search;
        Boid targetBoid = GetTargetBoid();
        if (search == null || targetBoid == null || boidManager.boids == null || boidInfo == null) return;

        Vector3 targetPos = targetBoid.transform.position;
        int numBoids = simParams.NumBoids;
        
        for (int i = 0; i < numBoids; i++) {
            if (i == simParams.TargetBoidID) continue;
            Boid candBoid = boidManager.boids[i];
            if (candBoid != null) {
                CreateOrUpdateLine(targetPos, candBoid.transform.position, simParams.DistanceCheckLineColor);
            }
        }
    }
}
