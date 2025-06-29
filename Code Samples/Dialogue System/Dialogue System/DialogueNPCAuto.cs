using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class DialogueNPCAuto : MonoBehaviour, InterfaceInteractable
{
    [Header("Gizmos")]
    public LayerMask playerLayer;
    public float detectionRadius = 7;

    [Header("Dialogue Script")]
    public DialogueScriptable dialogue;
    public GameObject dialogueUI;

    [Header("Dialogue UI")]
    [SerializeField] private TextMeshProUGUI _dialogueText;
    private int _dialogueCounter = 1;

    [SerializeField]
    private string _prompt;
    public string interactionPrompt => _prompt;

    private bool _isSkippable = true;
    public bool isCoroutineRunning = false;
    private bool _canInteract = true;
    [SerializeField]private bool _playSFX = false;
    private AudioSource _audioSource;
    [SerializeField]private AudioSource _mainAudioSource;
    //private Camera _cam;

    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        _dialogueText.text = dialogue.dialogues[0].dialogue;
    }

    private void Update()
    {
        _audioSource.volume = AudioHandler.Instance._voiceSource.volume;
        CheckForPlayer();
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
        _canInteract = false;
        _isSkippable = false;
        isCoroutineRunning = true;
        while(_dialogueCounter < dialogues.Count)
        {
            dialogueUI.SetActive(true);
            _dialogueText.text = "";
            yield return new WaitForSeconds(0.2f);
            _dialogueText.text = dialogue.dialogues[_dialogueCounter].dialogue; 
            _audioSource.clip = dialogues[_dialogueCounter].voice;
            _audioSource.Play();
            yield return new WaitForSeconds(_audioSource.clip.length);
            _dialogueCounter++;
            isCoroutineRunning = false;
            _isSkippable = false;
        }

            _dialogueText.text = dialogue.dialogues[0].dialogue;
            dialogueUI.SetActive(false);
            _dialogueCounter = 1;
            isCoroutineRunning = false;
            yield return new WaitForSeconds(0.5f);
            _canInteract = true;
    }

    private void CheckForPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

        bool playerCurrentlyInZone = false;

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                playerCurrentlyInZone = true;
                break;
            }
        }

        if (playerCurrentlyInZone && _canInteract == true && dialogue.dialogueType == DialogueScriptable.DialogueType.NPC)
        {
            if (InteractionPromptUI.Instance != null) 
            { 
                dialogueUI.SetActive(InteractionPromptUI.Instance.isDisplayed);
            }

        }

        if (!playerCurrentlyInZone)
        {
            StopAllCoroutines();
            _dialogueText.text = dialogue.dialogues[0].dialogue;
            dialogueUI.SetActive(false);
            _dialogueCounter = 1;
            isCoroutineRunning = false;
            _canInteract = true;
            //_audioSource.Stop();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
