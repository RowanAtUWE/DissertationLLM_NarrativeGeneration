using System;
using System.IO;
using System.Text;
using UnityEngine;

public class GameLogCreator : MonoBehaviour
{
    private string logFilePath;
    public bool loggingEnabled = true;

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