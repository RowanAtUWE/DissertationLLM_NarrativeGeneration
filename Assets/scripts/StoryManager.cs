using UnityEngine;
using System.Collections.Generic;


public class StoryManager : MonoBehaviour
{
    public List<PlotPoint> allMinorPlotPoints;
    public List<PlotPoint> allMajorPlotPoints;

    public List<PlotPoint> validMinorPlotPoints = new List<PlotPoint>();
    public List<PlotPoint> validMajorPlotPoints = new List<PlotPoint>();
    public List<PlotPoint> chosenMinorPlotPoints = new List<PlotPoint>();
    public List<PlotPoint> chosenMajorPlotPoints = new List<PlotPoint>();


    public Location currentLocation = Location.Village;
    public Phase currentPhase = Phase.Beginning;

    public bool isInNewLocation = false;
    public int messageCount = 0;

    private int minorAmount = 2;
    private int majorAmount = 1;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PickMajorPlotPoints();
        PickMinorPlotPoints();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Awake()
    {
        UpdateValidMinorPlotPoints();
        UpdateValidMajorPlotPoints();
        
    }

    public void UpdateValidMinorPlotPoints()
    {
        validMinorPlotPoints.Clear();
        
        foreach (var plotpoint in allMinorPlotPoints)
        {
            bool correctLocation = plotpoint.location.Contains(currentLocation);
            bool correctPhase = plotpoint.phase.Contains(currentPhase);

            if (correctLocation && correctPhase)
            {
                validMinorPlotPoints.Add(plotpoint);
            }
        }
    }

    public void PickMinorPlotPoints()
    {
        chosenMinorPlotPoints.Clear();
        if (chosenMinorPlotPoints.Count <= minorAmount)
        {
            for (int i = 0; i < minorAmount; i++)
            {
                int index = Random.Range(0, validMinorPlotPoints.Count);
                
                chosenMinorPlotPoints.Add(validMinorPlotPoints[index]);
                
                 

            }
            
        }
        
    }

    public void UpdateValidMajorPlotPoints()
    {
        validMajorPlotPoints.Clear();

        foreach (var plotpoint in allMajorPlotPoints)
        {
            bool correctLocation = plotpoint.location.Contains(currentLocation);
            bool correctPhase = plotpoint.phase.Contains(currentPhase);

            if (correctLocation && correctPhase)
            {
                validMajorPlotPoints.Add(plotpoint);
            }
        }
    }

    public void PickMajorPlotPoints()
    {
        chosenMajorPlotPoints.Clear();
        if (chosenMajorPlotPoints.Count <= majorAmount)
        {
            for (int i = 0; i < majorAmount; i++)
            {
                int index2 = Random.Range(0, validMajorPlotPoints.Count);
                
                chosenMajorPlotPoints.Add(validMajorPlotPoints[index2]);
                
                

            }

        }

    }

    public void SetLocation(Location newLocation)
    {
        if (newLocation != currentLocation)
        {
            currentLocation = newLocation;
            Debug.Log($"[STORY] Location changed to {currentLocation}");

            messageCount = 0;
            isInNewLocation = true;
            currentPhase = Phase.Beginning;

            UpdateValidMinorPlotPoints();
            UpdateValidMajorPlotPoints();
            PickMajorPlotPoints();
            PickMinorPlotPoints();
        }
    }

    public void SetStoryPhase(Phase newPhase)
    {
        if (newPhase != currentPhase)
        {
            currentPhase = newPhase;
            Debug.Log($"[STORY] Phase changed to {currentPhase}");
            
        }
    }
}
