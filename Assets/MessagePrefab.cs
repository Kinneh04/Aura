using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MessagePrefab : MonoBehaviour
{
    public TMP_Text txt;
    public float typingSpeed = 0.02f;

    public void LoadText(string t)
    {
        StartCoroutine(LoadTextCoroutine(t));
    }

    IEnumerator LoadTextCoroutine(string t)
    {
        txt.text = "";
        foreach (char c in t)
        {
            txt.text += c.ToString();
            yield return new WaitForSeconds(typingSpeed);
        }
    }
}
