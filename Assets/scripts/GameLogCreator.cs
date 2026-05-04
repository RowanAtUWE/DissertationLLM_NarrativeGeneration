using System;
using System.IO;
using System.Text;
using UnityEngine;

public class GameLogCreator : MonoBehaviour
{

    /// <summary>
    /// Handles logging of player inputs and LLM responses to a text file.
    /// 
    /// This is used for debugging, evaluation, and analysis through ablation.
    /// Documenting how prompts are constructed and how the model responds over time.
    /// 
    /// Each play session generates a timestamped log containing:
    /// - System configuration (model, prompt)
    /// - Player inputs
    /// - LLM responses
    /// </summary>

    private string logFilePath;
    public bool loggingEnabled = true;

    // Creates a new log file for each play session.

    public void InitialiseLog(string modelName, string ollamaBaseUrl, string systemPrompt)
    {
        if (!loggingEnabled) return;

        string folder = Path.Combine(Application.dataPath, "GameLogs");
        Directory.CreateDirectory(folder);

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"StoryRun_{timestamp}.txt";

        logFilePath = Path.Combine(folder, $"StoryRun_{timestamp}.txt");

        StringBuilder header = new StringBuilder();
        header.AppendLine("STORY RUN LOG");
        header.AppendLine($"Created: {DateTime.Now}");
        header.AppendLine($"Model: {modelName}");
        header.AppendLine($"Ollama URL: {ollamaBaseUrl}");
        header.AppendLine($"System Prompt: {systemPrompt}");
        header.AppendLine();
        header.AppendLine("--------------------------------");
        header.AppendLine();

        File.WriteAllText(logFilePath, header.ToString());

        Debug.Log($"[LOG] Run log created at:\n{logFilePath}");
    }

    // Appends player input to the log file.

    public void LogPlayerMessage(string message)
    {
        if (!loggingEnabled || string.IsNullOrEmpty(logFilePath)) return;

        StringBuilder entry = new StringBuilder();
        entry.AppendLine("PLAYER:");
        entry.AppendLine($"Time: {DateTime.Now}");
        entry.AppendLine(message);
        entry.AppendLine();
        entry.AppendLine("--------------------------------");
        entry.AppendLine();

        File.AppendAllText(logFilePath, entry.ToString());
    }

    // Appends full LLM response to the log file.

    public void LogLLMResponse(string response)
    {
        if (!loggingEnabled || string.IsNullOrEmpty(logFilePath)) return;

        StringBuilder entry = new StringBuilder();
        entry.AppendLine("LLM RESPONSE:");
        entry.AppendLine($"Time: {DateTime.Now}");
        entry.AppendLine(response);
        entry.AppendLine();
        entry.AppendLine("--------------------------------");
        entry.AppendLine();

        File.AppendAllText(logFilePath, entry.ToString());
    }

}