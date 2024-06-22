using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuggingFace.API;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
public class AuraManager : MonoBehaviour
{

    public Button SendButton;
    public GameObject AuraIsThinkingGO;
    public GameObject YourMessagePrefab, AuraMessagePrefab;

    public Transform MessagePrefabParent;

    public TMP_InputField YourInputfield;

    public string SetFailureApology;
    [TextArea(5, 5)]
    public string AuraContext, AuraContext2;
    [TextArea(5,5)]
    public string ExtraContext;
    [TextArea(5, 5)]
    public string AuraIntroMessage;
    [TextArea(2, 3)]
    public string InputHeader, InputCloser, Seperator;

    public Animator AuraAnimator;

    public List<string> YourPastMessages;
    public List<string> AuraPastMessages;

    public TMP_InputField ExtraContextInputfield;
    public GameObject SavedContextSuccess;

    public List<GameObject> SpawnedChatBubbles = new();

    public string[] SentenceSimilarities;
    public UnityEvent[] events;
    public float[] MatchingIndex;

    public string currentChatString;

    [Header("Events")]
    public string spotifyExtension = "https://open.spotify.com/search/";
    [TextArea(2,2)]
    public string OpenMusicResponse, OpenYoutubeResponse;

    public List<string> MusicGenres;
    public string youtubeExtension = "https://www.youtube.com/results?search_query=";

    public void OnChangeContext()
    {
        SavedContextSuccess.SetActive(false);
    }

    public void OnHitClearExtraContext()
    {
        ExtraContext = "";
        ExtraContextInputfield.text = "";
        SavedContextSuccess.SetActive(false);
    }

    public void OnHitNewChat()
    {
        foreach (GameObject GO in SpawnedChatBubbles)
        {
            Destroy(GO);
        }
        SpawnedChatBubbles.Clear();
        YourPastMessages.Clear();
        AuraPastMessages.Clear();
    }

    public void OnSaveExtraContext()
    {
        ExtraContext = ExtraContextInputfield.text;
        SavedContextSuccess.SetActive(true);
    }
    private void Start()
    {
        SpawnAuraMessage(AuraIntroMessage);
    }
    public void OnAuraConversationSuccess(string s)
    {
        SpawnAuraMessage(s);
    }

    public void OnAuraConversationFailure(string s)
    {
        SpawnAuraMessage(SetFailureApology + " : " + s);
    }

    public void OnHitAuraPop()
    {
        AuraAnimator.SetBool("PopIn", !AuraAnimator.GetBool("PopIn"));
    }

    public void ParseString(string s)
    {

            PassThroughSSModel(s);
    }
    public string ExtractStringAfterSeparator(string s)
    {
        if (!s.Contains(Seperator)) return s;
        int separatorIndex = s.IndexOf(Seperator);

        // Check if the separator was found
        if (separatorIndex != -1)
        {
            // Extract and return the substring after the separator
            // Adding the length of the separator to skip over it
            return s.Substring(separatorIndex + Seperator.Length);
        }
        else
        {
            // Separator not found, return an empty string or the original string,
            // depending on your needs. Here, returning an empty string as an example.
            return string.Empty;
        }
    }
    public void SendStringToModel(string s)
    {
        string history = "There is currently no chat history between you and user.";
        if (YourPastMessages.Count > 0)
        {
            history = "Chat History: ";
            for (int i = 0; i < YourPastMessages.Count; i++)
            {
                history += "User Query: " + YourPastMessages[i] + "\n";
                history += "Aura Response: " + AuraPastMessages[i] + "\n";
            }
        }

       string ContextedText = AuraContext + ExtraContext + AuraContext2 + history + InputHeader + s + InputCloser + Seperator;
        //Conversation context = new();
        //context.AddUserInput(AuraContext + ExtraContext + "'");
        //context.AddGeneratedResponse("Yes, I will respond accordingly.");
        HuggingFaceAPI.TextGeneration(ContextedText, OnAuraConversationSuccess, OnAuraConversationFailure);
    }
    
    public void SendYourMessage()
    {
        YourPastMessages.Add(YourInputfield.text);
        if (YourPastMessages.Count > 3) YourPastMessages.RemoveAt(0);
        GameObject GO = Instantiate(YourMessagePrefab);
        GO.transform.SetParent(MessagePrefabParent, false);
        GO.GetComponent<MessagePrefab>().LoadText(YourInputfield.text);
        ParseString(YourInputfield.text);

        YourInputfield.text = "";
        SendButton.interactable = false;
        AuraIsThinkingGO.SetActive(true);

        SpawnedChatBubbles.Add(GO);
      
    }

    public string returnMatchedGenre()
    {
     
        foreach (string s in MusicGenres)
        {
            if (currentChatString.Contains(s))
            {
                return s;
            }
        }
        return "random";
    }

    public string ExtractQuotedstring()
    {
        // Regular expression to find text within quotes
        Match match = Regex.Match(currentChatString, "'([^']*)'");
        if (match.Success)
        {
            return match.Groups[1].Value;
        }
        return null; // No quoted text found
    }
    public void OpenVisualStudioCode(string projectPath)
    {
        try
        {
            // Path to Visual Studio Code executable
            string vscodePath = @"C:\Program Files\Microsoft VS Code\Code.exe";

            // Start Visual Studio Code with the project folder
            Process.Start(vscodePath, projectPath);
            UnityEngine.Debug.Log("Visual Studio Code opened successfully.");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.Log("Error opening Visual Studio Code: " + e.Message);
        }
    }

    // Function to create a new virtual environment
    public void CreateVirtualEnvironment(string projectPath)
    {
        try
        {
            // Create the project directory if it doesn't exist
            Directory.CreateDirectory(projectPath);

            // Command to create a new virtual environment
            string command = $"python -m venv {Path.Combine(projectPath, "venv")}";

            // Start a new process to run the command
            ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
               SpawnAuraMessage("Virtual environment created successfully.");
            }
        }
        catch (System.Exception e)
        {
            SpawnAuraMessage("Error creating virtual environment: " + e.Message);
        }
    }
    public void PlayMusic()
    {
        string s = returnMatchedGenre();
        Application.OpenURL(spotifyExtension + s + "/playlists");
        SpawnAuraMessage(OpenMusicResponse + s);
    }

    public void PlayVideo()
    {
        string s = ExtractQuotedstring();
        Application.OpenURL(youtubeExtension + s);
        SpawnAuraMessage(OpenYoutubeResponse + s);
    }

    public void CreateNewDocument()
    {

    }

    public void CreateNewFolder()
    {

    }

    public void GenerateImage()
    {

    }

    public void SummarizeDocument()
    {

    }

    public void FindLinkToOpen()
    {

    }

    public void OpenVSCodeAndNewDirectory()
    {
        string projectName = ExtractQuotedstring();
        if (projectName != null)
        {
            string projectPath = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop), projectName);

            // Create the virtual environment
            CreateVirtualEnvironment(projectPath);

            // Open Visual Studio Code with the new project folder
            OpenVisualStudioCode(projectPath);
        }
        else
        {
            SpawnAuraMessage("Im sorry. I could not find the name of your desired virtual environment in your query. Please specify the name of your venv.");
        }
    }

    public void onSentenceSimilaritySuccess(float[] f)
    {
        MatchingIndex = f;
        int res = returnIndexThatMeetsRequirements();
        if (res == -1)
        {
            SendStringToModel(currentChatString);
        }
        else
        {
            events[res].Invoke();
        }
    }

    public int returnIndexThatMeetsRequirements()
    {
        int highestIndex = -1; float highest = 0;
        for (int i = 0; i < MatchingIndex.Length; i++)
        {
            if (MatchingIndex[i] > 0.55f && MatchingIndex[i] > highest)
            {
                highest = MatchingIndex[i];
                highestIndex = i;
            }
        }
        return highestIndex;
    }

    public void PassThroughSSModel(string s)
    {
       
        HuggingFaceAPI.SentenceSimilarity(s, onSentenceSimilaritySuccess, OnAuraConversationFailure, SentenceSimilarities);
        currentChatString = s.ToLower();
    }

    public void SpawnAuraMessage(string s)
    {
        AuraPastMessages.Add(s);
        if (YourPastMessages.Count > 3) YourPastMessages.RemoveAt(0);
        GameObject GO = Instantiate(AuraMessagePrefab);
        GO.transform.SetParent(MessagePrefabParent, false);
        GO.GetComponent<MessagePrefab>().LoadText(ExtractStringAfterSeparator(s));
      //  ParseString(YourInputfield.text);
        SendButton.interactable = true;
        AuraIsThinkingGO.SetActive(false);

        SpawnedChatBubbles.Add(GO);
    }
}
