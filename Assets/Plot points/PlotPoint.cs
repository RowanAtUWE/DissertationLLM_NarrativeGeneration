using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PlotPoint", menuName = "Scriptable Objects/PlotPoint")]
public class PlotPoint : ScriptableObject
{
    

    public string plotPointName;
    [TextArea(3, 10)]
    public string description;
    public List<Location> location;
    public List<Phase> phase;
    public bool isMajorPlotPoint;
    public bool isUsed;
}


   