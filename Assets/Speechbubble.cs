using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Speechbubble : MonoBehaviour
{
    public float delay = 0.1f;
    private string currentText = "";
    public TMP_Text Textbox;

    public void InputNewString(string i)
    {
        StopAllCoroutines();
        StartCoroutine(ShowText(i));
    }
    public IEnumerator ShowText(string fullText)
    {
        for (int i = 0; i <= fullText.Length; i++)
        {
            currentText = fullText.Substring(0, i);
            Textbox.text = currentText;
            yield return new WaitForSeconds(delay);
        }
    }
}
