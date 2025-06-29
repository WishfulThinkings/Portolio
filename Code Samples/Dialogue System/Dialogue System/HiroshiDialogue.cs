using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Database;

public class HiroshiDialogue : MonoBehaviour, InterfaceInteractable
{
    private DatabaseReference dbRef;

    [Header("Dialogue Script")]
    public DialogueScriptable[] questionAnswers;
    public UserInformationSO userInfo;

    public GameObject dialogueUI;

    [Header("Branching Dialogue Game Objects")]
    public GameObject[] branchingDialogues;
    private int branchingDialoguePart;


    [Header("Dialogue UI")]
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private TextMeshProUGUI dialogueName;
    [SerializeField] private Image dialogueImage;
    public float charDelay;
    private float charDelayData;

    [SerializeField] private int dialogueSelector = 1;
    [SerializeField] private int dialogueCounter = 1;


    [Header("Dialogue Options Button")]
    private int questionsAsked;
    public int requiredDialogue;


    [SerializeField]
    private string prompt;
    public string interactionPrompt => prompt;
    [SerializeField] private bool isSkippable = true;

    public bool isCoroutineRunning = false;
    [SerializeField] private bool isIntroductionDone = false;

    private AudioSource audioSource;

    [Header("UI Components")]
    public GameObject emojisUI;
    public GameObject chatUI;
    public GameObject BroadcastChatUI;
    public GameObject HostSettingsUI;
    public GameObject settingsUI;
    public GameObject settingsUI2;
    public GameObject ChangeChannelUI;

    [Header("Camera and UI")]
    public GameObject pressKeyGO;
    public GameObject[] cameras;
    public Vector3 dialogueCamTransform;



    private void Start()
    {
        if (userInfo.hiroshiBool == true)
        {
            gameObject.layer = 0;
        }

    }

    private void Awake()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        charDelayData = charDelay;
        InsertPlayerName();
    }

    private void Update()
    {
        SkipFunction();

        if (chatUI == null) // If null find the component
        {
            chatUI = FindObjectOfType<ChatBoxUIContainer>().gameObject;
        }

        if (BroadcastChatUI == null)
        {
            BroadcastChatUI = FindObjectOfType<BroadcastChatControl>().gameObject;
        }

        if (emojisUI == null)
        {
            emojisUI = FindAnyObjectByType<EmojiUIContainer>().gameObject;
        }

        if (HostSettingsUI == null)
        {
            HostSettingsUI = FindObjectOfType<HostSettings>().gameObject;
        }

        if (audioSource == null)
        {
            audioSource = AudioHandler.Instance._voiceSource;
        }
    }

    public bool Interact(Interactor interactor)
    {
        if (userInfo.hiroshiBool == false)
        {
            SwitchPlayerLayer(StarterAssets.ThirdPersonController.Instance.gameObject, 10);
            if (questionAnswers[dialogueSelector].dialogueType == DialogueScriptable.DialogueType.Story)
            {
                isSkippable = !isSkippable;

            }
            else { isSkippable = false; }

            if (isCoroutineRunning == false && isIntroductionDone == false)
            {
                HiroshiIntroductionDialogueReplacement();
                StartCoroutine(DialogueCoroutine(questionAnswers[0].dialogues, questionAnswers[0].dialogues.Count, branchingDialogues[branchingDialoguePart], questionAnswers[0].dialogues.Count));
            }
            else if (isCoroutineRunning == false && isIntroductionDone == true)
            {
                StartCoroutine(DialogueCoroutine(questionAnswers[dialogueSelector].dialogues, questionAnswers[dialogueSelector].dialogues.Count, branchingDialogues[branchingDialoguePart], questionAnswers[dialogueSelector].dialogues.Count));
            }
            return true;
        }

        else { return false; }
    }

    private void SkipFunction()
    {
        if (isSkippable == false && isCoroutineRunning == true)
        {
            charDelay = charDelayData;
        }
        else if (isSkippable == true)
        {

            charDelay = 0;
        }
    }

    private void InsertPlayerName()
    {
        for (int i = 1; i < questionAnswers.Length; i++)
        {
            questionAnswers[i].dialogues[0].name = userInfo.playerName;
            Debug.Log("Player Names are: " + questionAnswers[i].dialogues[0].name);
        }
    }

    private void TurnOffUI(bool state)
    {
        emojisUI.SetActive(state);
        chatUI.SetActive(state);
        settingsUI.SetActive(state);
        settingsUI2.SetActive(state);
        BroadcastChatUI.SetActive(state);
        HostSettingsUI.SetActive(state);
        ChangeChannelUI.SetActive(state);
    }

    public void DialogueOptions(int number)
    {
        isSkippable = false;
        SkipFunction();
        dialogueCounter = 0;
        isIntroductionDone = true;
        dialogueSelector = number;
        questionsAsked++;
        StartCoroutine(DialogueCoroutine(questionAnswers[dialogueSelector].dialogues, questionAnswers[dialogueSelector].dialogues.Count, branchingDialogues[branchingDialoguePart], questionAnswers[dialogueSelector].dialogues.Count));
    }

    public void DoneTalkingToHiroshi()
    {
        this.gameObject.layer = 0;
        dbRef.Child("Free To Play").Child(userInfo.playerID).Child("User Information").Child("hiroshiBool").SetValueAsync(true);
        cameras[1].transform.rotation = Quaternion.Euler(0, 14.994f, 0);
        userInfo.hiroshiBool = true;
        isCoroutineRunning = false;
        StarterAssets.ThirdPersonController.Instance._controller.enabled = true;
        SwitchPlayerLayer(StarterAssets.ThirdPersonController.Instance.gameObject, 7);
        cameras[1].SetActive(false);
        cameras[0].SetActive(true);
        StarterAssets.StarterAssetsInputs.Instance.StartLook();
        StarterAssets.StarterAssetsInputs.Instance.StartMovement();
        StarterAssets.StarterAssetsInputs.Instance.StartJump();
        TurnOffUI(true);
        branchingDialogues[1].SetActive(false);
        dialogueUI.SetActive(false);
    }

    private IEnumerator DialogueCoroutine(List<Dialogue> dialogues, int requiredDialogueBeforeQuestions, GameObject branchingDialogueUI, int dialogueNumber)
    {
        requiredDialogue = requiredDialogueBeforeQuestions;
        pressKeyGO.SetActive(false);
        TurnOffUI(false);

        cameras[0].SetActive(false);
        cameras[1].transform.position = new Vector3(dialogueCamTransform.x, dialogueCamTransform.y, dialogueCamTransform.z);
        cameras[1].transform.rotation = Quaternion.Euler(0, 28.877f, 0);
        cameras[1].SetActive(true);

        isSkippable = false;
        Debug.Log("Dialogue Coroutine is running");
        branchingDialogueUI.SetActive(false);
        StarterAssets.ThirdPersonController.Instance._controller.enabled = false;
        StarterAssets.StarterAssetsInputs.Instance.StopLook();
        StarterAssets.StarterAssetsInputs.Instance.StopMovement();
        StarterAssets.StarterAssetsInputs.Instance.StopJump();
        isCoroutineRunning = true;
        if (dialogueCounter < requiredDialogueBeforeQuestions)
        {
            dialogueImage.sprite = dialogues[dialogueCounter].image;
            audioSource.clip = dialogues[dialogueCounter].voice;
            audioSource.Play();
            dialogueText.text = "";
            dialogueName.text = dialogues[dialogueCounter].name;
            dialogueUI.SetActive(true);

            foreach (char letter in dialogues[dialogueCounter].dialogue.ToCharArray())
            {
                dialogueText.text += letter;
                yield return new WaitForSeconds(charDelay);
            }
            dialogueCounter++;
            isCoroutineRunning = false;
            isSkippable = false;
            pressKeyGO.SetActive(true);
        }

        else
        {
            if (dialogueSelector == 6)
            {
                DoneTalkingToHiroshi();
            }
            else
            {
                switch (questionsAsked)
                {
                    case 3:
                        yield return new WaitForSeconds(0.5f);
                        branchingDialogueUI.SetActive(false);
                        branchingDialoguePart++;
                        branchingDialogues[branchingDialoguePart].SetActive(true);
                        yield break;
                    default:
                        yield return new WaitForSeconds(0.5f);
                        isCoroutineRunning = false;
                        branchingDialogueUI.SetActive(true);
                        isSkippable = false;
                        break;
                }
            }
        }
    }

    private void HiroshiIntroductionDialogueReplacement()
    {
        questionAnswers[0].dialogues[0].dialogue = "Hello there " + userInfo.playerName.ToString() + "! Welcome again to the \"Start Land Convention.\"";
    }

    private void SwitchPlayerLayer(GameObject player, int layerNumber)
    {
        player.layer = layerNumber;
        foreach (Transform child in player.transform)
        {
            child.gameObject.layer = layerNumber;
            Transform _HasChildren = child.GetComponentInChildren<Transform>();
            if (_HasChildren != null)
                SwitchPlayerLayer(child.gameObject, layerNumber);
        }
    }
}