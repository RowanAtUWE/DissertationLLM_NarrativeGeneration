using UnityEngine;
using System.Collections.Generic;

public class EnviromentalUpdate : MonoBehaviour
{
    [Header("Environment Objects")]
    public List<GameObject> VillageEnviroment;
    public List<GameObject> ForestEnviroment;
    public List<GameObject> CaveEnviroment;
    public List<GameObject> CastleEnviroment;

    [Header("Story Agent Reference")]
    public OllamaStoryAgentChat storyAgent;
    public StoryManager storyManager;

    private Location lastLocation;

    void Start()
    {
        if (storyManager == null)
        {
            Debug.LogError("EnviromentalUpdate: storyAgent reference missing!");
            enabled = false;
            return;
        }

        lastLocation = storyManager.currentLocation - 1; 
        UpdateEnvironment();
    }

    void Update()
    {
        if (storyManager.currentLocation != lastLocation)
        {
            UpdateEnvironment();
            lastLocation = storyManager.currentLocation;
        }
    }

    private void UpdateEnvironment()
    {
        DisableAll();

        switch (storyManager.currentLocation)
        {
            case Location.Village:
                EnableList(VillageEnviroment);
                break;

            case Location.Forest:
                EnableList(ForestEnviroment);
                break;

            case Location.Cave:
                EnableList(CaveEnviroment);
                break;

            case Location.Castle:
                EnableList(CastleEnviroment);
                break;

            default:
                Debug.LogWarning("Unknown location no environment activated.");
                break;
        }

        Debug.Log($"[EnviromentalUpdate] Updated environment for location: {storyManager.currentLocation}");
    }

    private void DisableAll()
    {
        EnableList(VillageEnviroment, false);
        EnableList(ForestEnviroment, false);
        EnableList(CaveEnviroment, false);
        EnableList(CastleEnviroment, false);
    }

    private void EnableList(List<GameObject> list, bool enable = true)
    {
        foreach (GameObject obj in list)
        {
            if (obj != null)
                obj.SetActive(enable);
        }
    }
}
