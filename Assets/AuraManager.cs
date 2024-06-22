using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuggingFace.API;
using UnityEngine.UI;
using TMPro;
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
        if (s.Contains("HI"))
        {

        }
        else
        {
            SendStringToModel(s);
        }
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
        string history = "Chat has no history.";
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
    }
}
