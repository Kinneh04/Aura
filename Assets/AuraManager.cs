using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuggingFace;
using UnityEngine.UI;
using TMPro;
public class AuraManager : MonoBehaviour
{

    public Button SendButton;
    public GameObject AuraIsThinkingGO;
    public GameObject YourMessagePrefab, AuraMessagePrefab;

    public Transform MessagePrefabParent;

    public TMP_InputField YourInputfield;
    
    public void SendYourMessage()
    {
        GameObject GO = Instantiate(YourMessagePrefab);
        GO.transform.SetParent(MessagePrefabParent, false);
        GO.GetComponent<MessagePrefab>().LoadText(YourInputfield.text);
        YourInputfield.text = "";
        SendButton.interactable = false;
        AuraIsThinkingGO.SetActive(true);
    }

    public void SpawnAuraMessage()
    {

    }
}
