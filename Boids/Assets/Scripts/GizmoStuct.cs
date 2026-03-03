using UnityEngine;
using System;

[Serializable]
public struct GizmoStruct {
    public Color permiterColor;
    public Color cellColor;
    public Color headingColor;
    public Color neighborColor;

    public bool showBoidHeading;
    public bool showGrid;
    public bool showNeighbors;
}