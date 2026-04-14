using UnityEngine;
using System.Collections.Generic;
using System.Text;


public class MemoryManager : MonoBehaviour
{

    public List<string> narrativeMemories = new List<string>();

    private int maxMemories = 10;


    public string BuildMemoryPrompt()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Important:");
        foreach (var memory in narrativeMemories)
        {
            sb.AppendLine("- " + memory);
        }
        string memoryPrompt = sb.ToString();

        Debug.Log("current memory prompt: " + memoryPrompt);

        return memoryPrompt;
    }

    public void AddMemory(string memory)
    {
        if (string.IsNullOrWhiteSpace(memory) || memory.Length > 50)
        {
            Debug.Log("Invalid memory. Must be non-empty and less than 100 characters.");
            return;
        }
         
        
        if (!narrativeMemories.Contains(memory))
        {
            narrativeMemories.Add(memory);

            if (narrativeMemories.Count > maxMemories)
            {
                narrativeMemories.RemoveAt(0);
            }
        }
    }

    public void RemoveMemory(string memory)
    {
        narrativeMemories.Remove(memory);
    }
}
