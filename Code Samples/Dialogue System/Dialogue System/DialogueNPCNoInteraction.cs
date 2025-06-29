using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class DialogueNPCNoInteraction : MonoBehaviour
{
    [Header("Gizmos")]
    public LayerMask playerLayer;
    public float detectionRadius = 2.0f;
    public float timerDelay;

    [Header("Dialogue Script")]
    public DialogueScriptable dialogue;
    public GameObject dialogueUI;

    [Header("Dialogue UI")]
    [SerializeField] private TextMeshProUGUI _dialogueText;
    private int _dialogueCounter = 0;

    public bool isCoroutineRunning = false;
    [SerializeField]private bool _canInteract = true;
    private bool _playSFX = false;
    [SerializeField]private AudioSource _audioSource;
    //private Camera _cam;


    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    private void Update()
    {
        CheckForPlayer();
    }
    public IEnumerator DialogueCoroutine(List<Dialogue> dialogues)
    {
        Debug.Log("Coroutien is running");
        _canInteract = false;
        isCoroutineRunning = true;
        while (_dialogueCounter < dialogues.Count)
        {
            dialogueUI.SetActive(true);
            _dialogueText.text = "";
            yield return new WaitForSeconds(0.2f);
            _dialogueText.text = dialogue.dialogues[_dialogueCounter].dialogue;
            //_audioSource.clip = dialogues[_dialogueCounter].voice;
            //_audioSource.Play();
            yield return new WaitForSeconds(5f);
            _dialogueCounter++;
            isCoroutineRunning = false;
        }

        _dialogueText.text = dialogue.dialogues[0].dialogue;
        _dialogueCounter = 0;
        isCoroutineRunning = false;
        yield return new WaitForSeconds(timerDelay);
        dialogueUI.SetActive(false);
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


        if (playerCurrentlyInZone && _canInteract == true)
        {
            if (!isCoroutineRunning)
            {
                StartCoroutine(DialogueCoroutine(dialogue.dialogues));
                AudioManager.Instance.PlaySFXRecurring("Window-Open");
                if (dialogueUI.activeInHierarchy)
                {
                    _playSFX = true;
                    //AudioManager.Instance.PlaySFXRecurring("Window-Open");
                    Debug.Log("Is Running");
                }
                else if (!dialogueUI.activeInHierarchy)
                {
                    _playSFX = false;
                }
            }
           
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
