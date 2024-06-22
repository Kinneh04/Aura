using CodeMonkey.Utils;
using HuggingFace.API;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CatScript : MonoBehaviour
{

    public string Name;
    public bool isDragging = false;
    private Rigidbody2D rb;
    public Vector3 Offset;
    private Button_Sprite CMBS;
    public RectTransform rectTransform;
    public Animator CatAnimator;

    public AnimationClip CatStandIdleAnimClip;
    public AnimationClip[] CatIdleAnimClips;

    float timer = 0;
    public float minIdleAnim, maxIdleAnim;

    float sleepTimer = 0;
    public bool isSleeping = false;
    public float sleepChance = 0.05f;
    public float idleTimer = 0.0f;

    [Header("Popup")]
    public float lastsForTime;
    public GameObject PopupBox;
    public Speechbubble SB;
    public float speechtimer;
    public float minText, maxText;
    [Header("Texts")]
    public List<string> RandomTexts = new List<string>();
    public List<string> RandomTextsOnAwake = new List<string>();
    [Header("Keywords")]
    public GameObject ProcessingSpeech;
    

    [Header("DrawingBoard")]
    public Texture2D GeneratedImage;
    public GameObject TextureOutputImage;
    public Transform Ccanvas;

    [TextArea(5,5)]
    public string Catcontext;
    bool CheckKeywords(string inputString, string[] keywords)
    {
        return keywords.Any(keyword => inputString.Contains(keyword.ToLower()));
    }

    public string chooseRandomFrom(List<string> choices)
    {
        return (choices[Random.Range(0, choices.Count)]);
    }

    public void GenerateImage()
    {
        GameObject GO = Instantiate(TextureOutputImage);
        TextureOutput TO = GO.GetComponent<TextureOutput>();
        TO.texture = GeneratedImage;
        TO.image.texture = TO.texture;
        GO.transform.SetParent(Ccanvas, true);
        GO.transform.position = new Vector3(0, 0, 0);
    }

    public void GoToSleep()
    {
        isSleeping = true;
        CatAnimator.SetBool("isSleeping", true);
        sleepTimer = 999999;
        idleTimer = 0.0f;
    }

    public void WakeUp()
    {
        if (isSleeping)
        {
            PopUpSpeech(RandomTextsOnAwake[Random.Range(0, RandomTextsOnAwake.Count)]);
        }
        isSleeping = false;
        CatAnimator.SetBool("isSleeping", false);
    }

    public void CatProcessSpeech(string spoken)
    {
        ProcessingSpeech.SetActive(true);
        
        spoken = spoken.ToLower();
        if (isSleeping && spoken.Contains(Name.ToLower()) || isSleeping && spoken.Contains("wake"))
        {
            WakeUp();
        }
        else if (!isSleeping && spoken.Contains("sleep"))
        {
            GoToSleep();
        }
        else if (spoken.Contains("image") && spoken.Contains("generate") || spoken.Contains("image") || spoken.Contains("picture"))
        {
            HuggingFaceAPI.TextToImage(spoken, response =>
            {
                GeneratedImage = response;
                GenerateImage();
                ProcessingSpeech.SetActive(false);
                PopUpSpeech("Here is your image meow!");
            }, error =>
            {
                PopUpSpeech("Sorry I was being silly. Can you repeat that please?");
                ProcessingSpeech.SetActive(false);
            });
        }
        else if (!isSleeping)
        {
            HuggingFace.API.Conversation conversation = new Conversation();
            conversation.AddUserInput(Catcontext);
            conversation.AddGeneratedResponse("Ok Meow. My name is muffin, and I am a cat, and you are my new owner");
            HuggingFaceAPI.Conversation(spoken, response =>
            {
                PopUpSpeech(response);
                ProcessingSpeech.SetActive(false);
            }, error =>
            {
                ProcessingSpeech.SetActive(false);
            }, context: conversation);
        }
        
    }




    public void PopUpSpeech(string speech, float duration = 4.0f)
    {
        PopupBox.SetActive(false);
        PopupBox.SetActive(true);
        lastsForTime = duration;
        SB.InputNewString(speech);
    }

    private void Start()
    {
        timer = maxIdleAnim;
        rb = GetComponent<Rigidbody2D>();
        CMBS = GetComponent<Button_Sprite>();
        CMBS.MouseDownOnceFunc = () =>
        {
            StartDrag();
        };
        CMBS.MouseUpOnceFunc = () =>
        {
            EndDrag();
        };

       // PopUpSpeech("Meow");
        speechtimer = maxText;

        GoToSleep();
    }
    private void StartDrag()
    {
        WakeUp();
        isDragging = true;
        rb.gravityScale = 0f; // Disable gravity while dragging
    }

    private void EndDrag()
    {
        isDragging = false;
        rb.gravityScale = 1f; // Enable gravity after releasing the sprite
    }

    private void Update()
    {

        if (speechtimer > 0 && !PopupBox.activeSelf)
        {
            speechtimer -= Time.deltaTime;
            if (speechtimer <= 0)
            {
                speechtimer = Random.Range(minText, maxText);
                if(!isSleeping)
                    PopUpSpeech(RandomTexts[Random.Range(0, RandomTexts.Count)]);
            }
        }

        if (PopupBox.activeSelf)
        {
            lastsForTime -= Time.deltaTime;
            if (lastsForTime <= 0) PopupBox.SetActive(false);
        }

        if (isDragging)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,
  Input.mousePosition.y, 10.0f));
            //Debug.Log(mousePos);
            transform.position = new Vector2(mousePos.x, mousePos.y);

        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position + Offset, Vector2.down, 0.1f);

        if (hit.collider == null)
        {
            //  Debug.Log("Sprite is in the air!");
            CatAnimator.SetBool("isFalling", true);
        }
        else
        {
            //Debug.Log(hit.collider.name);
            CatAnimator.SetBool("isFalling", false);

            if (!isDragging && !isSleeping) idleTimer += Time.deltaTime;
            else idleTimer = 0;
        }

        if (timer > 0)
        {
            timer -= Time.deltaTime;
            if (timer <= 0 && !isDragging && !isSleeping)
            {
                CatAnimator.Play(CatIdleAnimClips[Random.Range(0, CatIdleAnimClips.Length)].name);
                timer = Random.Range(minIdleAnim, maxIdleAnim);
            }
        }

        if (idleTimer > 25f && !isSleeping)
        {
            GoToSleep();
        }

        //if (Input.GetMouseButtonDown(1))
        //{
        //    // Create a ray from the mouse position
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit hit2;

        //    // Perform the raycast
        //    if (Physics.Raycast(ray, out hit2))
        //    {
        //        // Check if the object hit is tagged as "cat"
        //        if (hit2.collider.CompareTag("cat") && !isDragging)
        //        {
        //            StartDrag();
        //        }
        //    }
        //}

        //if (Input.GetMouseButtonUp(1))
        //{
        //    if (isDragging)
        //    {
        //        EndDrag();
        //    }
        //}

        // else Debug.Log(hit.collider.name);
    }
}
