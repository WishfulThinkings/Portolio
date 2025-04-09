using MixedReality.Toolkit;
using ModooMarbleGameClientServerData.Src.Dtos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using ModooMarbleGameClientServerData.Src.Entities.GameModels;
using ModooMarbleGameClientServerData.Src.Types;
public class PlayerMovement : MonoBehaviour
{

    [Header("Board Components")]
    [SerializeField]private Transform[] tiles;
    public Transform[] animators;
    private readonly Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();
    private IEnumerator runningCoroutine;

    [Header("Player Components")]

    [SerializeField] private float playerMovementSpeed = 20;
    [Header("------------------------------------")]

    [SerializeField] private bool isCoroutineRunning;


    [Header("Players")]
    public List<GameObject> players = new();
    public GameObject noOlympicTileFound;
    private bool hasLandmark;

    [Header("NPC Components")]

    [Space]
    [SerializeField] private float pauseDuration = 0.3f;
    [SerializeField] private float rotationTime = 0.1f;
    [SerializeField] private string idleAnimationName = "Idle";
    [SerializeField] private string runningAnimationName = "Run";

    public static PlayerMovement Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);

        }
    }
    private void Start()
    {
        MatchStartAnimation();
        StartCoroutine(CoroutineCoordinator());
    }

    public void MoveTile(UInt64 number, UInt64 playerNumber)
    {
        Debug.Log("MOVING TO " + number);
        Debug.Log("PLAYER NUMBER IS " + playerNumber);
        if (runningCoroutine == null)
        {
            runningCoroutine = MovePlayerCoroutine(number, playerNumber);
            StartCoroutine(runningCoroutine);
        }
        else
        {
            coroutineQueue.Enqueue(MovePlayerCoroutine(number, playerNumber));
        }
    }

    public void LandOnTile(UInt64 number, UInt64 playerNumber)
    {
        if (runningCoroutine == null)
        {
            runningCoroutine = PlayerLandOnTileCoroutine(number, playerNumber);
            StartCoroutine(runningCoroutine);
        }
        else
        {
            coroutineQueue.Enqueue(PlayerLandOnTileCoroutine(number, playerNumber));
        }
    }

    IEnumerator CoroutineCoordinator()
    {
        while (true)
        {
            while (coroutineQueue.Count > 0)
                yield return StartCoroutine(coroutineQueue.Dequeue());
            yield return null;
        }
    }


    public IEnumerator MovePlayerCoroutine(UInt64 tileNumber, UInt64 playerNumber)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].GetComponent<CharacterController>().playerId == playerNumber)
            {

                players[i].GetComponent<Animator>().Play(runningAnimationName);
                Vector3 startPosition = players[i].transform.position;
                Vector3 targetPosition = tiles[tileNumber].transform.position;

                AudioManager.Instance.PlaySFX(UnityEngine.Random.Range(7, 11));

                Transform playerTransform = players[i].transform;


                Vector3 directionToTarget = targetPosition - startPosition;
                directionToTarget.y = 0; 
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

                float turnDuration = rotationTime;
                playerTransform.DORotateQuaternion(targetRotation, turnDuration).SetEase (Ease.Linear);




                float journeyLength = Vector3.Distance(startPosition, targetPosition);
                float moveDuration = journeyLength / playerMovementSpeed;
                playerTransform.DOMove (targetPosition, moveDuration).SetEase(Ease.Linear);
                yield return new WaitForSeconds(moveDuration);



                if (animators[tileNumber].GetComponent<TileMovement>() != null) animators[tileNumber].GetComponent<TileMovement>().TileHitAnim();
            }
        }
    }

    public IEnumerator PlayerLandOnTileCoroutine(UInt64 tileNumber, UInt64 playerNumber)
    {
        Debug.Log("PLAYER LAND ON TILE IS LANDING IN " +  tileNumber);
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].GetComponent<CharacterController>().playerId == playerNumber)
            {
                players[i].GetComponent<Animator>().Play(runningAnimationName);
                Vector3 startPosition = players[i].transform.position;
                Vector3 targetPosition = tiles[tileNumber].transform.position;

                Quaternion startRotation = players[i].transform.rotation;
                Vector3 directionToTarget = targetPosition - startPosition;
                directionToTarget.y = 0;
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                float rotationElapsed = 0f;

                while (rotationElapsed < rotationTime)
                {
                    players[i].transform.rotation = Quaternion.Slerp(startRotation, targetRotation, rotationElapsed / rotationTime);
                    rotationElapsed += Time.deltaTime;
                    yield return null;
                }
                players[i].transform.rotation = targetRotation; 

                // Move towards the target position
                float moveElapsed = 0f;
                float journeyLength = Vector3.Distance(startPosition, targetPosition);
                float moveDuration = journeyLength / playerMovementSpeed;

                AudioManager.Instance.PlaySFX(UnityEngine.Random.Range(7, 11));

                while (moveElapsed < moveDuration)
                {
                    players[i].transform.position = Vector3.Lerp(startPosition, targetPosition, moveElapsed / moveDuration);
                    moveElapsed += Time.deltaTime;
                    yield return null;
                }
                players[i].transform.position = targetPosition;

                yield return new WaitForSeconds(0.1f);
                players[i].GetComponent<Animator>().Play(idleAnimationName);
            }
        }
        for (int j = 0; j < players.Count; j++)
        {
            players[j].transform.GetPositionAndRotation(out Vector3 worldPosition, out Quaternion worldRotation);
            if (players[j].GetComponent<CharacterController>().playerId == playerNumber)
            {
                players[j].transform.SetParent(tiles[tileNumber].transform, false);
                players[j].transform.position = worldPosition;
                players[j].transform.rotation = worldRotation;
                
                players[j].transform.localEulerAngles = Vector3.zero;
            }
        }

        if (tileNumber == 17 && playerNumber == WebsocketHandler.Instance.selfPlayerId)
        {
            yield return CheckLandmark(playerNumber);
        }
    }

    public IEnumerator MovePlayerInstantly(UInt64 tileNumber, UInt64 playerNumber)
    {


        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].GetComponent<CharacterController>().playerId == playerNumber)
            {

                Vector3 startPosition = players[i].transform.position;
                Vector3 targetPosition = tiles[tileNumber].transform.position;


                Vector3 diveDownPosition = targetPosition;
                diveDownPosition.y = -2f; 


                float diveDuration = 0.3f; 
                float elapsedTime = 0f;

                while (elapsedTime < diveDuration)
                {
                    players[i].transform.position = Vector3.Lerp(startPosition, diveDownPosition, elapsedTime / diveDuration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                players[i].transform.position = diveDownPosition; 


                yield return new WaitForSeconds(0.1f);


                Vector3 originalPosition = targetPosition;
                originalPosition.y = startPosition.y;

                elapsedTime = 0f;
                while (elapsedTime < diveDuration)
                {
                    players[i].transform.position = Vector3.Lerp(diveDownPosition, originalPosition, elapsedTime / diveDuration);
                    elapsedTime += Time.deltaTime;
                    yield return null;
                }
                players[i].transform.position = originalPosition; 

 
                players[i].transform.rotation = Quaternion.identity;


                players[i].GetComponent<Animator>().Play(idleAnimationName);


                players[i].transform.SetParent(tiles[tileNumber].transform, false);
            }
        }

     

    }

    private IEnumerator CheckLandmark(UInt64 playerId)
    {
        var tileList = WebsocketHandler.Instance.matchClient.GameMap.GetTilesOfType<PropertyTileGameModel>(TileTypeEnum.PROPERTY);
        foreach (var tile in tileList)
        {
            if (tile.OwnerPlayerId == playerId)
            {
                if (tile.CurrentUpgradeLevel == PropertyUpgradeLevelEnum.LANDMARK)
                {
                    hasLandmark = true;
                    break;
                }
                else
                {
                    hasLandmark = false;
                }
            }
        }
        if (hasLandmark == true)
        {
            noOlympicTileFound.SetActive(false);
        }
        else
        {
            noOlympicTileFound.SetActive(true);
            yield return new WaitForSeconds(1f);
            noOlympicTileFound.SetActive(false);
        }
    }

    private void MatchStartAnimation()
    {
        foreach (var player in players)
        {
            player.GetComponent<Animator>().Play("Greet Match Start");
        }
    }

    public void AcceleratorMovement()
    {
        playerMovementSpeed = 50f;
        pauseDuration = 0f;
        rotationTime = 0f;
        Invoke(nameof(ResetMovement), 3f);
    }

    public void ResetMovement()
    {
        playerMovementSpeed = 10f;
        pauseDuration = 0.1f;
        rotationTime = 0.1f;
    }
    private void OnEnable()
    {
        WebsocketEventHandler.MoveTile += MoveTile;
        WebsocketEventHandler.LandOnTile += LandOnTile;
    }

    private void OnDisable()
    {
        WebsocketEventHandler.MoveTile -= MoveTile;
        WebsocketEventHandler.LandOnTile -= LandOnTile;
    }

}



