using UnityEngine;

public class DisablePlayerVisibilityHiroshi : MonoBehaviour
{
    public LayerMask playerLayer;
    public float detectionRadius = 5.0f;
    [SerializeField] private bool playerInZone = false;
    private GameObject playerGO;

    public HiroshiDialogue hiroshiDialogue;


    private void Update()
    {
        CheckForPlayer();
    }

    private void CheckForPlayer()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, playerLayer);

        bool playerCurrentlyInZone = false;

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Player"))
            {
                playerGO = collider.gameObject;
                playerCurrentlyInZone = true;
                break;
            }
        }

        if (playerCurrentlyInZone && !playerInZone)
        {
            SwitchPlayerLayer(playerGO, 10);
        }

        else if (!playerCurrentlyInZone && playerInZone && hiroshiDialogue.isCoroutineRunning == true)
        {
            SwitchPlayerLayer(playerGO, 10);
        }

        else if (!playerCurrentlyInZone && playerInZone)
        {
            SwitchPlayerLayer(playerGO, 7);
        }

        playerInZone = playerCurrentlyInZone;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }

    private void SwitchPlayerLayer(GameObject player, int layerNumber)
    {
        player.layer = layerNumber;
        foreach (Transform child in player.transform)
        {
            child.gameObject.layer = layerNumber;
            Transform hasChildren = child.GetComponentInChildren<Transform>();
            if (hasChildren != null)
                SwitchPlayerLayer(child.gameObject, layerNumber);
        }
    }
}
