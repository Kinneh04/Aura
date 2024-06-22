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
using System;

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

    public bool hasCreatedVEnv;
    public string VEnvPath;
    public TMP_InputField VenvPathInputfield;

    [TextArea(15,15)]
    public List<string> SampleCodes;
    public Toggle CanDelete;

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
            return ExtractStringAfterSeparator(s.Substring(separatorIndex + Seperator.Length));
          //  return s.Substring(separatorIndex + Seperator.Length);
        }
        else
        {
            // Separator not found, return an empty string or the original string,
            // depending on your needs. Here, returning an empty string as an example.
            return string.Empty;
        }
    }
    public void InstallLibrary(string projectPath, string libraryName)
    {
        try
        {
            string activatePath = Path.Combine(projectPath, "venv", "Scripts", "activate");
            string command = $"\"{activatePath}\" && pip install {libraryName}";
            ProcessStartInfo startInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();
               SpawnAuraMessage($"{libraryName} installed successfully.");
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.Log($"Error installing {libraryName}: " + e.Message);
        }
    }
    public void InstallLibraryOntoVenv()
    {
        if (!hasCreatedVEnv)
        {
            SpawnAuraMessage("It seems we have not created a virtual environment this session. Please specify where you would like me to create a new one");
            return;
        }

        string libName = ExtractQuotedstring();
        InstallLibrary(VEnvPath, libName);
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
        string ContextedText = "";
        if (string.IsNullOrEmpty(ExtraContext))
        {
             ContextedText = AuraContext + ExtraContext + AuraContext2 + history + InputHeader + s + InputCloser + Seperator;
        }
        else  ContextedText = ExtraContext + AuraContext2 + history + InputHeader + s + InputCloser + Seperator;
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
            string vscodePath = @"C:\Users\kohli\AppData\Local\Programs\Microsoft VS Code\Code.exe";

            // Start Visual Studio Code with the project folder
            Process.Start(vscodePath, projectPath);
            UnityEngine.Debug.Log("Visual Studio Code opened successfully.");
        }
        catch (System.Exception e)
        {
            SpawnAuraMessage("Im sorry. I could not open VSCode as requested: " + e.Message);
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
                VEnvPath = projectPath;
                VenvPathInputfield.text = projectPath;
                hasCreatedVEnv = true;
            }
        }
        catch (System.Exception e)
        {
            SpawnAuraMessage("Im sorry. I could not create the virtual environment as requested: " + e.Message);
        }
    }

    public void OnClearVenvPath()
    {
        VenvPathInputfield.text = "";
        OnEditVenvPath();

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
        string filename = ExtractQuotedstring();
        if (string.IsNullOrEmpty(ExtractQuotedstring()))
        {
            SpawnAuraMessage("No valid filename found.");
        }

        // Get the desktop path
        string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

        // Combine the desktop path with the filename
        string filePath = Path.Combine(desktopPath, filename);

        // Create the file
        try
        {
            using (FileStream fs = File.Create(filePath))
            {
                // Optionally write some content to the file
                byte[] info = new System.Text.UTF8Encoding(true).GetBytes("This is a sample file.");
                fs.Write(info, 0, info.Length);
            }
            SpawnAuraMessage("Your File has been created successfully and saved to the desktop: " + filePath);
        }
        catch (Exception e)
        {
            SpawnAuraMessage("Error creating file: " + e.Message);
        }
    }

    public void OnGenerateImageSuccess(Texture2D image)
    {

    }
    public void GenerateImage()
    {
        HuggingFaceAPI.TextToImage(currentChatString, OnGenerateImageSuccess, OnAuraConversationFailure);
    }

    public void OnEditVenvPath()
    {
        hasCreatedVEnv = !string.IsNullOrEmpty(VenvPathInputfield.text);
        VEnvPath = VenvPathInputfield.text;
    }

    public IEnumerator WriteDelayed(string s, string pythonScriptPath)
    {
        SpawnAuraMessage("Writing your sample python code in main.py!");
        using (StreamWriter writer = new StreamWriter(pythonScriptPath))
        {
            foreach (char c  in s)
            {
                writer.Write(c);
            }
            yield return new WaitForSeconds(0.04f);
        }
        SpawnAuraMessage("Sample Python project created successfully.");
    }

    public void OnCreateSampleProject()
    {
        CreateSampleProject(VEnvPath);
    }

    public void CreateSampleProject(string projectPath)
    {
        try
        {
            // Create a new Python script file
            string pythonScriptPath = Path.Combine(projectPath, "main.py");

            StartCoroutine(WriteDelayed(SampleCodes[UnityEngine.Random.Range(0, SampleCodes.Count)], pythonScriptPath));

          
        }
        catch (System.Exception e)
        {
            SpawnAuraMessage("Error creating sample Python project: " + e.Message);
        }
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
    public void onPackageProject()
    {
        PackageProject(VEnvPath);
    }
    public void PackageProject(string projectPath)
    {
        if (!hasCreatedVEnv)
        {
            SpawnAuraMessage("You have not created a virtual environment yet");
            return;
        }
        try
        {
            string activatePath = Path.Combine(projectPath, "venv", "Scripts");
            string installPyInstallerCommand = $"\"{activatePath}\" && pip install pyinstaller";
            string pyInstallerCommand = $"CD {activatePath} && pyinstaller --onefile {projectPath}/main.py"; // Assuming main.py is the entry point
            UnityEngine.Debug.Log(pyInstallerCommand);
            // Install pyinstaller
            ProcessStartInfo installStartInfo = new ProcessStartInfo("cmd.exe", "/c " + installPyInstallerCommand)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process installProcess = Process.Start(installStartInfo))
            {
                installProcess.WaitForExit();
                UnityEngine.Debug.Log("pyinstaller installed successfully.");
            }

            // Package the project
            ProcessStartInfo packageStartInfo = new ProcessStartInfo("cmd.exe", "/c " + pyInstallerCommand)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            using (Process packageProcess = Process.Start(packageStartInfo))
            {
                packageProcess.WaitForExit();
                SpawnAuraMessage("Project packaged successfully. Package is in " + activatePath);
            }
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.Log("Error packaging project: " + e.Message);
        }
    }
    public int returnIndexThatMeetsRequirements()
    {
        int highestIndex = -1; float highest = 0;
        for (int i = 0; i < MatchingIndex.Length; i++)
        {
            if (MatchingIndex[i] > 0.42f && MatchingIndex[i] > highest)
            {
                highest = MatchingIndex[i];
                highestIndex = i;
            }
        }
        return highestIndex;
    }

    public void OnDeleteVirtualEnv()
    {
        if(CanDelete)

        DeleteVirtualEnvironment(VEnvPath);
        
        else
        {
            SpawnAuraMessage("Aura is not allowed to delete virtual environments! Please enable it in the settings tab");
        }
    }
    public void DeleteVirtualEnvironment(string projectPath)
    {
        try
        {
            string venvPath = Path.Combine(projectPath, "venv");

            if (Directory.Exists(venvPath))
            {
                // Deactivate the virtual environment
                string deactivateCommand = Path.Combine(venvPath, "Scripts", "deactivate");
                Process.Start(deactivateCommand);

                // Delete the virtual environment directory
                Directory.Delete(venvPath, true);

                SpawnAuraMessage("Virtual environment deleted successfully. Venv path has been cleared in settings!");
                OnClearVenvPath();
            }
            else
            {
                SpawnAuraMessage("Virtual environment does not exist.");
            }
        }
        catch (System.Exception e)
        {
            SpawnAuraMessage("Error deleting virtual environment: " + e.Message);
        }
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
