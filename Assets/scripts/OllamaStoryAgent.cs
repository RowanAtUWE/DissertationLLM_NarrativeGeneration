using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using OllamaSharp;
using System.Linq;

public class OllamaStoryAgentChat : MonoBehaviour
{
    // --- UI ---
    public TMP_InputField playerInputField;
    public TextMeshProUGUI responseText;
    public Button sendButton;

    
    public StoryManager storyManager;
    public MemoryManager memoryManager;
    public GameLogCreator logCreator;
    private int messageCount = 0;

    // --- Ollama settings ---
    [Tooltip("Ollama base address, e.g. http://localhost:11434")]
    public string ollamaBaseUrl = "http://localhost:11434";

    [Tooltip("Model name (set to whatever model you have)")]
    public string modelName = "gemma3:4b";

    private OllamaApiClient client;
    private Chat chat; // persistent chat object that tracks messages / context



    // Streaming / state
    private bool isProcessing = false;
    private StringBuilder streamingBuffer = new StringBuilder();

    // System prompt instructing JSON output
    private readonly string systemPrompt =
        "You are an in-game storyteller agent. Always respond with a single JSON object (no extra text) with these fields: " +
        "{\"answer\":\"text the player reads\"location\":\"one of: Village,Forest,Cave,Castle\", \"addMemory\":\"}. " +
        "If location should not change, set location to the current location name or Unknown. " +
        "Do not output anything outside the JSON object.Do not use quotation marks inside the answer text." + "\r\n\r\n" +
        "\r\n Narrative Rules: " +
        "\r\n-You will be presented with minor and major narrative threads, build towards these gradually. Minor narrative threads should always be resolved before the major narrative thread is introduced." +
        "\r\n- Never resolve any narrative thread immediately upon introduction." +
        "\r\n- Narrative threads must take multiple turns to develop (minimum 3 turns)." +
        "\r\n- Focus on slow pacing, tension, and immersion." +
        "\r\n Story Phase Rules: " +
        "\r\n- The first response should establish the setting and introduce the player to their current location." +
        "\r\n- Do not introduce minor or major narrative threads in the beginning phase. Focus on world-building and immersion." +
         "\r\n- In the middle phase, start introducing minor narrative threads gradually, building tension and intrigue, but do not force narrative threads to resolve quickly." +
         "\r\n- In the climax phase, start resolving minor narrative threads and introduce the major narrative thread. The major narrative thread should be the climax of the story and should be resolved slowly to create a satisfactory ending." +
        "\r\n- In the end phase resolve any remaining narrative threads before urging the player to explore a new location." +
        "\r\n Memory Rules: "  +
        "\r\n- Always use 'addMemory' to return memory additions" +
        //"\r\n- Memories must be less than 50 characters long and concise." +
        "\r\n- You may only store 10 memories at a time, ensure memories fall under set categories:" +
        "\r\n - Important persistent facts about the world" +
        "\r\n - Ongoing situations / narrative threads" +
        "\r\n - Return memory additions as a list of short strings. If nothing should be added, return an empty list.";

    // "\r\n- Do not introduce minor or major narrative threads in the beginning phase. Focus on world-building and immersion once the player is engaged and exploring the world transition to the Middle phase." +
    // "\r\n- In the middle phase, start introducing minor narrative threads gradually, building tension and intrigue, but do not force narrative threads to resolve quickly. Once the narrative threads are introduced progress to the End phase." +
    // "\r\n- In the end phase, you can start resolving minor narrative threads and introduce the major narrative thread. The major narrative thread should be the climax of the story and should be resolved slowly to create a satisfactory ending." +
    // "\r\n- After the narrative arc has ended reset the phase to beginning and influence the player to travel to a new location one of:(Village, Forest, Cave, Castle)." +
    [Serializable]
    private class StoryResponse
    {
        public string answer;
        public string location;
        public List<string> addMemory;
    }

    async void Start()
    {
        Debug.Log($"System prompt: " + systemPrompt);
        if (playerInputField == null) Debug.LogWarning("playerInputField not assigned.");
        if (responseText == null) Debug.LogWarning("responseText not assigned.");
        if (sendButton == null) Debug.LogWarning("sendButton not assigned.");

        try
        {
            client = new OllamaApiClient(new Uri(ollamaBaseUrl));
            client.SelectedModel = modelName;

            chat = new Chat(client);

         
            await AddSystemMessageIfNeeded();

            if (responseText != null) responseText.text = "Storyteller ready.";
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to initialize Ollama client: {ex.Message}");
            if (responseText != null) responseText.text = $"ERROR: Could not connect to Ollama.\n{ex.Message}";
            if (sendButton != null) sendButton.interactable = false;
            if (playerInputField != null) playerInputField.interactable = false;
            return;
        }

        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendClicked);
        }

        logCreator?.InitialiseLog(modelName, ollamaBaseUrl, systemPrompt);
    }

    
    private async Task AddSystemMessageIfNeeded()
    {
        
        try
        {
            await foreach (var _ in chat.SendAsync(systemPrompt))
            {
                // ignore tokens for seeding
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Warning: seeding system prompt failed: {e.Message}");
        }
    }

    public async void OnSendClicked()
    {
        if (isProcessing) return;

        string user = playerInputField != null ? playerInputField.text : "";

        if (string.IsNullOrWhiteSpace(user))
        {
            if (responseText != null) responseText.text = "Please enter something.";
            return;
        }

        if (playerInputField != null) playerInputField.text = "";
        if (responseText != null) responseText.text = "...";

        if (sendButton != null) sendButton.interactable = false;
        isProcessing = true;

        streamingBuffer.Clear();

        logCreator?.LogPlayerMessage(user);

        await SendUserMessage(user);



        isProcessing = false;
        if (sendButton != null) sendButton.interactable = true;
    }

    private async Task SendUserMessage(string userMessage)
    {
        try
        {
            storyManager.messageCount++;

            int memoryCount = memoryManager.narrativeMemories.Count;

            if (memoryCount >= 5 && storyManager.currentPhase == Phase.Beginning && storyManager.messageCount >= 7)
            {
                storyManager.SetStoryPhase(Phase.Middle);
            }
            if (memoryCount >= 10 && storyManager.currentPhase == Phase.Middle && storyManager.messageCount >= 15)
            {
                storyManager.SetStoryPhase(Phase.Climax);
            }
            if (memoryCount >= 10 && storyManager.currentPhase == Phase.Climax && storyManager.messageCount >= 20)
            {
                storyManager.SetStoryPhase(Phase.End);
            }

            string locationHint = $"CurrentLocation: {storyManager.currentLocation}";

            PlotPoint firstMajor = null;
            if (storyManager.chosenMajorPlotPoints != null && storyManager.chosenMajorPlotPoints.Count > 0)
            {
                firstMajor = storyManager.chosenMajorPlotPoints.FirstOrDefault();
            }
            string majorName = firstMajor != null ? firstMajor.plotPointName : "No name";
            string majorDesc = firstMajor != null ? firstMajor.description : "No major plot points chosen.";



            List<string> minorPlotNames = new List<string>();
            List<string> minorPlotDesc = new List<string>();

            if (storyManager.chosenMinorPlotPoints != null)
            {
                foreach (var point in storyManager.chosenMinorPlotPoints)
                {
                    minorPlotNames.Add(point.plotPointName);
                    minorPlotDesc.Add(point.description);
                }
            }


            string majorPlotPrompts = "";
            string minorPlotPrompts = "";

            switch (storyManager.currentPhase)
            {
                case Phase.Beginning:
                    majorPlotPrompts = "No major narrative thread has been introduced yet.\n";
                    minorPlotPrompts = "No minor narrative threads have been introduced yet.\n";
                    break;

                case Phase.Middle:
                    majorPlotPrompts = "No major narrative thread has been introduced yet.\n";
                    minorPlotPrompts = $"Current Minor narrative thread names: {string.Join(", ", minorPlotNames)}\n" + $"the descriptions of the minor threads are {string.Join(", ", minorPlotDesc)}";
                    break;

                case Phase.Climax:
                    majorPlotPrompts = "Current Major narrative thread (forshadowing):\n" + $" Name: \"{majorName}\"\n" + $" Description: \"{majorDesc}\"";
                    minorPlotPrompts = $"Current Minor narrative thread names: {string.Join(", ", minorPlotNames)}\n" + $"the descriptions of the minor threads are {string.Join(", ", minorPlotDesc)}";
                    break;

                case Phase.End:
                    majorPlotPrompts = $"Resolve current major narrative thread {majorName}.\n";
                    minorPlotPrompts = $"Resolve current minor narrative threads {string.Join(", ", minorPlotNames)}.\n";
                    break;
            }

            /*if (storyManager.isInNewLocation)
            {
                majorPlotPrompts = "Current Major narrative thread (forshadowing):\n" + $" Name: \"{majorName}\"\n" + $" Description: \"{majorDesc}\"";
                minorPlotPrompts = $"Current Minor narrative thread names: {string.Join(", ", minorPlotNames)}\n" + $"the descriptions of the minor threads are {string.Join(", ", minorPlotDesc)}";
            }
            else if (!storyManager.isInNewLocation)
            {
                majorPlotPrompts = "Current Major narrative thread (forshadowing):\n" + $" Name: \"{majorName}\"\n";
                minorPlotPrompts = $"Current Minor narrative thread names: {string.Join(", ", minorPlotNames)}";
            }*/

            string storyphase = $"Current story phase: {storyManager.currentPhase}";

            //Debug.Log($"Major prompt: {majorPlotPrompts}\nMinor names: {string.Join(", ", minorPlotNames)}\nMinor descriptions: {string.Join(" | ", minorPlotDesc)}");

            // Combine the player's message with a short instruction to respond with JSON:
            string userPayload = $"{memoryManager.BuildMemoryPrompt()}\n{locationHint}\n{storyphase}\n{majorPlotPrompts}\n{minorPlotPrompts}\nPlayer: {userMessage}\nRespond now with the JSON object only.";

            Debug.Log($"Sending user message to model:\n{userPayload}");

            storyManager.isInNewLocation = false;

            await foreach (var token in chat.SendAsync(userPayload))
            {
                streamingBuffer.Append(token);

                if (responseText != null)
                {
                    responseText.text = streamingBuffer.ToString();
                }
            }

            string fullResponse = streamingBuffer.ToString();
            HandleAssistantFullResponse(fullResponse);
            logCreator?.LogLLMResponse(fullResponse);
        }
        catch (Exception ex)
        {
            Debug.LogError($"SendUserMessage failed: {ex.Message}\n{ex.StackTrace}");
            if (responseText != null) responseText.text = $"Error: {ex.Message}";
        }
    }

    private void HandleAssistantFullResponse(string fullText)
    {
        Debug.Log($"RAW assistant response:\n{fullText}");

        string json = ExtractFirstJsonObject(fullText);
        if (string.IsNullOrEmpty(json))
        {
            Debug.LogWarning("Could not extract JSON from model response. Showing raw text.");
            if (responseText != null) responseText.text = fullText;
            return;
        }

        StoryResponse resp = null;

        try
        {
            resp = JsonUtility.FromJson<StoryResponse>(json);
        }
        catch (Exception ex)
        {
            Debug.LogError($"JSON parse failed: {ex.Message}\nExtracted JSON: {json}");
            if (responseText != null) responseText.text = $"Malformed JSON: {ex.Message}";
            return;
        }

        if (responseText != null) responseText.text = resp.answer ?? "";

        if (!string.IsNullOrWhiteSpace(resp.location))
        {
            if (EnumTryParseIgnoreCase(resp.location.Trim(), out Location newLoc))
            {
                if (newLoc != storyManager.currentLocation)
                {
                    //storyManager.currentLocation = newLoc;
                    storyManager.SetLocation(newLoc);
                }
            }
            else
            {
                Debug.LogWarning($"Unknown location string from model: '{resp.location}'");
            }
        }

        /*if (!string.IsNullOrWhiteSpace(resp.storyPhase))
        {
            if (EnumTryParseIgnoreCase(resp.storyPhase.Trim(), out Phase phase))
            {
                storyManager.SetStoryPhase(phase);
            }
            else
            {
                Debug.LogWarning($"Unknown story phase string from model: '{resp.storyPhase}'");
            }
        }*/

        if (resp.addMemory != null)
        {
            if (resp.addMemory.Count <= 3)
            {
                foreach (var memory in resp.addMemory)
                {
                    Debug.Log($"Adding memory to narrativeMemories: {memory}");
                    memoryManager.AddMemory(memory);
                }
            }
               

        }
    }
    private bool EnumTryParseIgnoreCase<T>(string value, out T result) where T : struct
    {
        return Enum.TryParse<T>(value, true, out result);
    }

    private static string ExtractFirstJsonObject(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return null;

        int depth = 0;
        int start = -1;
        for (int i = 0; i < s.Length; i++)
        {
            if (s[i] == '{')
            {
                if (depth == 0) start = i;
                depth++;
            }
            else if (s[i] == '}')
            {
                depth--;
                if (depth == 0 && start != -1)
                {
                    return s.Substring(start, i - start + 1);
                }
            }
        }

        int first = s.IndexOf('{');
        int last = s.LastIndexOf('}');
        if (first >= 0 && last > first) return s.Substring(first, last - first + 1);
        return null;
    }

 
}