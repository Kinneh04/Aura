using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextureOutput : MonoBehaviour
{
    public Texture2D texture;

    public RawImage image;

    private void Start()
    {
        image.texture = texture;
    }
    public void CloseWindow()
    {
        Destroy(gameObject);
    }
}
