using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class DialogueInteractionScript : MonoBehaviour, InterfaceInteractable
{


    [Header("Dialogue Script")]
    public DialogueScriptable dialogue;
    public GameObject dialogueUI;

    [Header("Dialogue UI")]
    [SerializeField]private TextMeshProUGUI _dialogueText;
    [SerializeField]private TextMeshProUGUI _dialogueName;
    public float charDelay;
    private float _charDelayData;
    private int _dialogueCounter = 1;

    [SerializeField]
    private string _prompt;
    public string interactionPrompt => _prompt;

    private bool _isSkippable = true;
    public bool isCoroutineRunning = false;
    private bool _canInteract = true;
    private AudioSource _audioSource;
    private void Awake()
    {
        _charDelayData = charDelay;
        _audioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        _dialogueText.text = dialogue.dialogues[0].dialogue;
    }

    private void Update()
    {
        SkipFunction();
        if (dialogue.dialogueType == DialogueScriptable.DialogueType.NPC && _canInteract == true)
        {
            if(InteractionPromptUI.Instance != null) { dialogueUI.SetActive(InteractionPromptUI.Instance.isDisplayed); } 
        }
    }
    private void SkipFunction()
    {
        if (_isSkippable == false && isCoroutineRunning == true)
        {
            charDelay = _charDelayData;
        }
        else if (_isSkippable == true)
        {

            charDelay = 0;
        }
    }

    public bool Interact(Interactor interactor)
    {
        if (dialogue.dialogueType == DialogueScriptable.DialogueType.NPC)
        {
            _isSkippable = !_isSkippable;
        
        }
        else { _isSkippable = false; }


        if (isCoroutineRunning == false && _canInteract == true)
        {
            StartCoroutine(DialogueCoroutine(dialogue.dialogues));
        }
        return true;
    }
    public IEnumerator DialogueCoroutine(List<Dialogue> dialogues)
    {
        _isSkippable = false;
        StarterAssets.ThirdPersonController.Instance._controller.enabled = false;
        StarterAssets.StarterAssetsInputs.Instance.StopLook();
        StarterAssets.StarterAssetsInputs.Instance.StopMovement();
        StarterAssets.StarterAssetsInputs.Instance.StopJump();
        isCoroutineRunning = true;   
        if(_dialogueCounter < dialogues.Count)
        {
            _dialogueText.text = "";
            if (dialogue.dialogueType == DialogueScriptable.DialogueType.Story) { _dialogueName.text = dialogues[_dialogueCounter].name; }
            dialogueUI.SetActive(true);
            _audioSource.clip = dialogues[_dialogueCounter].voice;
            _audioSource.Play();
                foreach (char letter in dialogues[_dialogueCounter].dialogue.ToCharArray())
                {
                    _dialogueText.text += letter;
                    yield return new WaitForSeconds(charDelay);
                }

            _dialogueCounter++;
            isCoroutineRunning = false;
            _isSkippable = false;
           
        }

        else 
        {
            _dialogueText.text = dialogue.dialogues[0].dialogue;
            _canInteract = false;
            dialogueUI.SetActive(false); 
            _dialogueCounter = 1; 
            isCoroutineRunning = false; 
            StarterAssets.ThirdPersonController.Instance._controller.enabled = true; 
            StarterAssets.StarterAssetsInputs.Instance.StartLook(); 
            StarterAssets.StarterAssetsInputs.Instance.StartMovement();
            StarterAssets.StarterAssetsInputs.Instance.StartJump();
            yield return new WaitForSeconds(0.5f);
            _canInteract = true;
        }     
    }

    
}
