using UnityEngine;
using System.Collections.Generic;
using System.Text;


public class MemoryManager : MonoBehaviour
{

    /// <summary>
    /// Manages a limited set of narrative memories used to provide additional persistent context to the LLM.
    ///
    /// This system acts as a lightweight memory layer, storing key story elements
    /// (e.g. ongoing events, important facts) and embedding them into prompts each turn.
    /// </summary>

    public List<string> narrativeMemories = new List<string>();

    // Maximum number of memories retained at once.
    // Oldest memories are removed first to keep context focused.
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

    // Adds a new memory entry if it passes validation checks.

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

    //Unused for now, but could be used to remove specific memories based on story context.

    public void RemoveMemory(string memory)
    {
        narrativeMemories.Remove(memory);
    }
}
