using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Firebase.Database;

public class BranchingDialogue : MonoBehaviour, InterfaceInteractable
{
    [Header("Firebase")]
    public DatabaseReference dbRef;

    [Header("Dialogue Script")]
    public DialogueScriptable[] questionAnswers;
    public UserInformationSO user;
    public GameObject dialogueUI;
    public OneTimeInterActionScriptable sakuraDialogue;

    [Header("Branching Dialogue Game Objects")]
    public GameObject[] branchingDialogues;
    private int _branchingDialoguePart;
    public GameObject[] cameras;
    public Vector3 dialogueCamTransform;



    [Header("Dialogue UI")]
    [SerializeField] private TextMeshProUGUI _dialogueText;
    [SerializeField] private TextMeshProUGUI _dialogueName;
    [SerializeField] private Image _dialogueImage;
    public GameObject pressKeyGO;
    public float charDelay;
    private float charDelayData;

    [SerializeField] private int _dialogueSelector = 1;
    [SerializeField] private int _dialogueCounter = 1;


    [Header("Dialogue Options Button")]
    private int _questionsAsked;
    public int requiredDialogue;


    [SerializeField]
    private string _prompt;
    public string interactionPrompt => _prompt;
    [SerializeField] private bool _isSkippable = true;

    [HideInInspector] public bool isCoroutineRunning = false;
    [SerializeField] private bool _isIntroductionDone = false;

    [SerializeField] private AudioSource _audioSource;

    [Header("UI Components")]
    public GameObject emojisUI;
    public GameObject chatUI;
    public GameObject BroadcastChatUI;
    public GameObject HostSettingsUI;
    public GameObject settingsUI;
    public GameObject settingsUI2;
    public GameObject ChangeChannelUI;


    private void Awake()
    {
        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        charDelayData = charDelay;
    }

    private void Update()
    {
        if (chatUI == null) // If null find the component
        {
            chatUI = FindObjectOfType<ChatBoxUIContainer>().gameObject;
        }

        if (BroadcastChatUI == null)
        {
            BroadcastChatUI = FindObjectOfType<BroadcastChatControl>().gameObject;
        }

        if (HostSettingsUI == null)
        {
            HostSettingsUI = FindObjectOfType<HostSettings>().gameObject;
        }

        if (emojisUI == null)
        {
            emojisUI = FindAnyObjectByType<EmojiUIContainer>().gameObject;
        }

        SkipFunction();


        if (_audioSource == null)
        {
            _audioSource = AudioHandler.Instance._voiceSource;
        }
    }

    public bool Interact(Interactor interactor)
    {
        if (user.sakuraBool == false)
        {
            SwitchPlayerLayer(StarterAssets.ThirdPersonController.Instance.gameObject, 10);
            if (questionAnswers[_dialogueSelector].dialogueType == DialogueScriptable.DialogueType.Story)
            {
                _isSkippable = !_isSkippable;

            }
            else { _isSkippable = false; }

            if (isCoroutineRunning == false && _isIntroductionDone == false)
            {
                StartCoroutine(DialogueCoroutine(questionAnswers[0].dialogues, questionAnswers[0].dialogues.Count, branchingDialogues[_branchingDialoguePart], questionAnswers[0].dialogues.Count));
            }
            else if (isCoroutineRunning == false && _isIntroductionDone == true)
            {
                StartCoroutine(DialogueCoroutine(questionAnswers[_dialogueSelector].dialogues, questionAnswers[_dialogueSelector].dialogues.Count, branchingDialogues[_branchingDialoguePart], questionAnswers[_dialogueSelector].dialogues.Count));
            }
            return true;
        }

        else { gameObject.layer = 0; }
        return true;
    }

    public void StartDialogue()
    {
        if (user.sakuraBool == false)
        {
            StartCoroutine(DialogueCoroutine(questionAnswers[0].dialogues, questionAnswers[0].dialogues.Count, branchingDialogues[_branchingDialoguePart], questionAnswers[0].dialogues.Count));
        }

        else { gameObject.layer = 0; }
    }
    private void SkipFunction()
    {
        if (_isSkippable == false && isCoroutineRunning == true)
        {
            charDelay = charDelayData;
        }
        else if (_isSkippable == true)
        {
            charDelay = 0;
        }
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

    private void InsertPlayerName()
    {
        for (int i = 1; i < questionAnswers.Length - 1; i++)
        {
            questionAnswers[i].dialogues[0].name = user.playerName;
        }
    }

    private void TurnOffUI(bool state)
    {
        pressKeyGO.SetActive(false);
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
        _isSkippable = false;
        SkipFunction();
        _dialogueCounter = 0;
        _isIntroductionDone = true;
        _dialogueSelector = number;
        _questionsAsked++;
        StartCoroutine(DialogueCoroutine(questionAnswers[_dialogueSelector].dialogues, questionAnswers[_dialogueSelector].dialogues.Count, branchingDialogues[_branchingDialoguePart], questionAnswers[_dialogueSelector].dialogues.Count));
    }

    public void DoneTalkingToSakura()
    {
        dbRef.Child("Free To Play").Child(user.playerID).Child("User Information").Child("sakuraBool").SetValueAsync(true);
        this.gameObject.layer = 0;
        user.sakuraBool = true;
        isCoroutineRunning = false;
        SwitchPlayerLayer(StarterAssets.ThirdPersonController.Instance.gameObject, 7);
        cameras[1].SetActive(false);
        cameras[0].SetActive(true);
        StarterAssets.ThirdPersonController.Instance._controller.enabled = true;
        StarterAssets.StarterAssetsInputs.Instance.StartLook();
        StarterAssets.StarterAssetsInputs.Instance.StartMovement();
        StarterAssets.StarterAssetsInputs.Instance.StartJump();
        TurnOffUI(true);
        sakuraDialogue.interactionFinished = true;
        branchingDialogues[2].SetActive(false);
        dialogueUI.SetActive(false);
    }


    private IEnumerator DialogueCoroutine(List<Dialogue> dialogues, int requiredDialogueBeforeQuestions, GameObject branchingDialogueUI, int dialogueNumber)
    {
        pressKeyGO.SetActive(false);
        InsertPlayerName();
        TurnOffUI(false);

        cameras[0].SetActive(false);
        cameras[1].transform.position = new Vector3(dialogueCamTransform.x, dialogueCamTransform.y, dialogueCamTransform.z);
        cameras[1].SetActive(true);

        _isSkippable = false;
        branchingDialogueUI.SetActive(false);
        StarterAssets.ThirdPersonController.Instance._controller.enabled = false;
        StarterAssets.StarterAssetsInputs.Instance.StopLook();
        StarterAssets.StarterAssetsInputs.Instance.StopMovement();
        StarterAssets.StarterAssetsInputs.Instance.StopJump();
        isCoroutineRunning = true;
        if (_dialogueCounter < requiredDialogueBeforeQuestions - 1)
        {
            _dialogueImage.sprite = dialogues[_dialogueCounter].image;
            _audioSource.clip = dialogues[_dialogueCounter].voice;
            _audioSource.Play();
            _dialogueText.text = "";
            _dialogueName.text = dialogues[_dialogueCounter].name;
            dialogueUI.SetActive(true);

            foreach (char letter in dialogues[_dialogueCounter].dialogue.ToCharArray())
            {
                _dialogueText.text += letter;
                yield return new WaitForSeconds(charDelay);
            }
            _dialogueCounter++;
            isCoroutineRunning = false;
            _isSkippable = false;
            pressKeyGO.SetActive(true);
        }

        else
        {

            switch (_questionsAsked)
            {
                case 3:
                    yield return new WaitForSeconds(0.5f);
                    branchingDialogueUI.SetActive(false);
                    _branchingDialoguePart++;
                    branchingDialogues[_branchingDialoguePart].SetActive(true);
                    yield break;
                case 6:
                    branchingDialogueUI.SetActive(false);
                    yield return new WaitForSeconds(0.5f);
                    _branchingDialoguePart++;
                    branchingDialogues[_branchingDialoguePart].SetActive(true);
                    yield break;
                default:
                    yield return new WaitForSeconds(0.5f);
                    isCoroutineRunning = false;
                    branchingDialogueUI.SetActive(true);
                    _isSkippable = false;
                    break;
            }
        }
    }
}
