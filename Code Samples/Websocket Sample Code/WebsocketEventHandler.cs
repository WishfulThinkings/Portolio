using Microsoft.MixedReality.GraphicsTools;
using ModooMarbleGameClientServerData.Src.Dtos;
using ModooMarbleGameClientServerData.Src.Dtos.GameEventDtos;

using ModooMarbleGameClientServerData.Src.Dtos.GameInputDtos;
using ModooMarbleGameClientServerData.Src.Entities.GameModels;
using ModooMarbleGameClientServerData.Src.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static Microsoft.MixedReality.GraphicsTools.MeshInstancer;

public class WebsocketEventHandler : MonoBehaviour
{
    public delegate void MoveTileAction(UInt64 tile, UInt64 player);
    public static MoveTileAction MoveTile;

    public delegate void LandOnTileAction(UInt64 tile, UInt64 player);
    public static LandOnTileAction LandOnTile;

    public delegate void RollDiceDelegate(ushort result, ulong playerID);
    public static RollDiceDelegate RollDiceAction;

    public static WebsocketEventHandler Instance;

    [Header("Buy Property Attributes")]
    public static ulong tileIDPrompt;
    [SerializeField]private PropertyUpgradeLevelEnum upgradeLevel = PropertyUpgradeLevelEnum.HOUSE;

    [Header("Roll Components")]
    public RectMask2DFast thermometer;
    public GameObject thermometerGO;
    public float percentage;
    [SerializeField]private bool isRolling;
    public float fillDuration;
    public Button rollButton;

    [Header("Target Player UI")]
    public GameObject targetPlayer;
    
    [Header("Chance Card Info")]
    public UInt64 chanceCardTargetID;
    public UInt64 chanceCardID;

    [Header("Is Double UI Check")]
    public GameObject rolledDoubleUI;

    public List<CharacterController> eventPlayerLists;
    public List<TargetPlayerButton> targetPlayerButtons;
    public List<TargetPlayerButton> targetPlayerButtonsChance;

    public GameObject overlay;
    public GameObject optionScreen;
    public bool isGameStarting;
    public TextMeshProUGUI gameEventText;
    public GameObject preRollUI;
    public Animator die;

    private Queue<GameEventDto> gameEventQueue = new Queue<GameEventDto>();
    [SerializeField]private bool _isProcessingEvent = false;

    [Header("Prompt Timeout Bools")]
    [SerializeField]private bool _turnOrderTimeOut = true;
    [SerializeField] private bool _preRollTimeOut = true;
    [SerializeField] private bool _playerBuyPropertyTimeOut = true;
    [SerializeField] private bool _playerUpgradePropertyTimeOut = true;
    [SerializeField] private bool _playerBuyoutPropertyTimeOut = true;
    [SerializeField] private bool _playerBuyShopItemsTimeOut = true;
    [SerializeField] private bool _playerJailBailOutTimeOut = true;
    [SerializeField] private bool _playerCardTargetTimeOut = true;
    [SerializeField] private bool _goTileRemoteUpgradeTimeOut = true;
    [SerializeField] private bool _playerMortgageTimeOut = true;
    [SerializeField] private bool _olympicEventTimeout = true;
    [SerializeField] private bool _worldTileEventTimeout = true;
    [SerializeField] private bool _holidayTile = true;
    [SerializeField] private bool _holidayTileBuyout = true;
    [SerializeField] private bool _holidayVisit = true;
    [SerializeField] private bool _holidayVisitBuyout = true;
                     public static bool _cityDonation = true;
    [SerializeField] private bool _citySwap = true;
    private bool _playerAssigned;


    [Header("Player UI")]
    public List<AssignPlayer> playerUIScript;


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
        WebsocketHandler.Instance.playerList = eventPlayerLists;
    }

    private void OnEnable()
    {
        WebsocketHandler.ProcessGameEvent += EnqueueGameEvent;
    }

    private void OnDisable()
    {
        WebsocketHandler.ProcessGameEvent -= EnqueueGameEvent;
    }


    private void Update()
    {
      //DisablePlayerInteraction();

    }

    private void EnqueueGameEvent(GameEventDto gameEvent)
    {
        gameEventQueue.Enqueue(gameEvent);
        if (!_isProcessingEvent)
        {
            StartCoroutine(ProcessGameEventQueue());
        }
    }

    private IEnumerator ProcessGameEventQueue()
    {
        _isProcessingEvent = true;

        while (gameEventQueue.Count > 0)
        {
            GameEventDto currentEvent = gameEventQueue.Dequeue();
            yield return StartCoroutine(ProcessGameEvent(currentEvent));
        }

        _isProcessingEvent = false;
    }

    private IEnumerator ProcessGameEvent (GameEventDto gameEvent) {

        //gameEventText.text = gameEvent.GameEventType.ToString();
        if (gameEvent.GameEventType != GameEventTypeEnum.MATCH_TIME_UPDATE)
            Debug.Log (gameEvent.GameEventType.ToString ());

        MatchGameModelClient matchClient = WebsocketHandler.Instance.matchClient;

        switch (gameEvent.GameEventType) {

            //============================== Match Game Events ================================//
            case GameEventTypeEnum.MATCH_START: {
                matchClient.MatchStart ();
                AssignPlayers ();
            }
            break;

            case GameEventTypeEnum.MATCH_END: {
                matchClient.MatchEnd ();
            }
            break;

            case GameEventTypeEnum.MATCH_PLAYER_TURN_START: {
                var matchPlayerTurnStartEvent = (MatchPlayerTurnStartGameEventDto)gameEvent;

                matchClient.PlayerSetupTurnStart (matchPlayerTurnStartEvent.playerId);
                matchClient.SetCurrentTurnPlayerId (matchPlayerTurnStartEvent.playerId);

                yield return StartCoroutine (AnimationController.Instance.MachPlayerTurnStartEventCoroutine (matchPlayerTurnStartEvent.playerId, rollButton));

                DisablePlayerInteraction (matchPlayerTurnStartEvent.playerId);


            }
            break;

            case GameEventTypeEnum.MATCH_PLAYER_TURN_END: {
                var matchPlayerTurnEndEvent = (MatchPlayerTurnEndGameEventDto)gameEvent;
                //DisablePlayerInteraction(matchPlayerTurnEndEvent.playerId);
                DisableRollUI ();
                matchClient.PlayerSetupTurnEnd (matchPlayerTurnEndEvent.playerId);
                // DisablePlayerInteraction();
                //AnimationController.Instance.MatchPlayerTurnStartEvent(WebsocketHandler.Instance.matchClient.GetTurnIndexOfPlayerId(matchPlayerTurnEndEvent.playerId));
            }
            break;

            case GameEventTypeEnum.MATCH_PLAYER_TURN_REPEAT: {
                var matchPlayerTurnRepeatEvent = (MatchPlayerTurnRepeatGameEventDto)gameEvent;
                matchClient.PlayerSetupTurnRepeat (matchPlayerTurnRepeatEvent.playerId);
                AnimationController.Instance.ResetBool ();
                DisablePlayerInteraction (matchPlayerTurnRepeatEvent.playerId);
            }
            break;

            case GameEventTypeEnum.MATCH_TURN_NUMBER_UPDATE: {
                // DisablePlayerInteraction();
                var matchPlayerTurnNumberUpdate = (MatchTurnNumberUpdateGameEventDto)gameEvent;
                matchClient.SetTurnNumber (matchPlayerTurnNumberUpdate.turnNumber);
                yield return StartCoroutine (AnimationController.Instance.UpdateTurnUI (matchPlayerTurnNumberUpdate.turnNumber));
            }
            break;

            case GameEventTypeEnum.MATCH_PLAYER_DEFEAT: {
                //  DisablePlayerInteraction();
                var playerdefeat = (MatchPlayerDefeatGameEventDto)gameEvent;
                matchClient.EliminatePlayer (playerdefeat.playerId);
                yield return StartCoroutine (AnimationController.Instance.PlayerDefeat (playerdefeat.playerId, playerdefeat.defeatType));
            }
            break;

            case GameEventTypeEnum.MATCH_PLAYER_WIN: {
                // DisablePlayerInteraction();
                var playerWin = (MatchPlayerWinGameEventDto)gameEvent;
                yield return StartCoroutine (AnimationController.Instance.PlayerWin (playerWin.playerId, playerWin.winType));
            }
            break;

            // ===================================================== Player Game Events ==============================================//
            case GameEventTypeEnum.PLAYER_GAIN_CHIPS: {
                var gainChipsEvent = (PlayerGainChipsGameEventDto)gameEvent;
                matchClient.PlayerGainChips (gainChipsEvent.playerId, gainChipsEvent.chipsAmount);
                StartCoroutine (AnimationController.Instance.PlayerGainChipsEvent (gainChipsEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_LOSE_CHIPS: {
                var loseChipsEvent = (PlayerLoseChipsGameEventDto)gameEvent;
                matchClient.PlayerLoseChips (loseChipsEvent.playerId, loseChipsEvent.chipsAmount);
                StartCoroutine (AnimationController.Instance.PlayerLoseChipsEvent (loseChipsEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_GAIN_ITEM: {
                var playerGainItemEvent = (PlayerGainItemGameEventDto)gameEvent;
                matchClient.PlayerGainItem (playerGainItemEvent.playerId, playerGainItemEvent.itemId);
                yield return StartCoroutine (AnimationController.Instance.PlayerGainItemEvent (playerGainItemEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_LOSE_ITEM: {
                var playerLoseItemEvent = (PlayerLoseItemGameEventDto)gameEvent;
                matchClient.PlayerLoseItem (playerLoseItemEvent.playerId, playerLoseItemEvent.itemId);
                yield return StartCoroutine (AnimationController.Instance.PlayerLoseItemEvent (playerLoseItemEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_GAIN_STATUS_EFFECT: {
                var playerGainStatusEffectEvent = (PlayerGainStatusEffectGameEventDto)gameEvent;
                matchClient.PlayerGainStatusEffect (playerGainStatusEffectEvent.playerId, playerGainStatusEffectEvent.statusEffect);
                yield return StartCoroutine (AnimationController.Instance.PlayerGainStatusEffectEvent (playerGainStatusEffectEvent.playerId, playerGainStatusEffectEvent.statusEffect.StatusEffectType));
            }
            break;

            case GameEventTypeEnum.PLAYER_LOSE_STATUS_EFFECT: {
                var playerLoseStatusEffectEvent = (PlayerLoseStatusEffectGameEventDto)gameEvent;
                matchClient.PlayerLoseStatusEffect (playerLoseStatusEffectEvent.playerId, playerLoseStatusEffectEvent.statusEffect);
                yield return StartCoroutine (AnimationController.Instance.PlayerLoseStatusEffectEvent (playerLoseStatusEffectEvent.playerId, playerLoseStatusEffectEvent.statusEffect.StatusEffectType));
            }
            break;

            case GameEventTypeEnum.PLAYER_TRIGGER_STATUS_EFFECT: {
                var playerTriggerStatusEvent = (PlayerTriggerStatusEffectGameEventDto)gameEvent;
                yield return StartCoroutine (AnimationController.Instance.PlayerTriggerStatusEffectEvent (playerTriggerStatusEvent.playerId, playerTriggerStatusEvent.statusEffect.StatusEffectType));
            }
            break;

            case GameEventTypeEnum.PLAYER_PASS_THROUGH_TILE: {
                var playerPassThroughTileEvent = (PlayerPassThroughTileGameEventDto)gameEvent;
                yield return PlayerMovement.Instance.MovePlayerCoroutine (playerPassThroughTileEvent.tileId, playerPassThroughTileEvent.playerId);
                //MoveTile?.Invoke(playerPassThroughTileEvent.tileId, playerPassThroughTileEvent.playerId);
                // Add animation handling if needed.
            }
            break;

            case GameEventTypeEnum.PLAYER_LAND_ON_TILE: {
                var playerLandOnTileEvent = (PlayerLandOnTileGameEventDto)gameEvent;
                //LandOnTile?.Invoke(playerLandOnTileEvent.tileId, playerLandOnTileEvent.playerId);
                yield return PlayerMovement.Instance.PlayerLandOnTileCoroutine (playerLandOnTileEvent.tileId, playerLandOnTileEvent.playerId);
                matchClient.PlayerMoveToTile (playerLandOnTileEvent.playerId, playerLandOnTileEvent.tileId);
                // Add animation handling if needed
            }
            break;
            case GameEventTypeEnum.PLAYER_PRE_ROLL_PROMPT: {
                var playerPreRollPromptEvent = (PlayerPreRollPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (playerPreRollPromptEvent.playerId, PlayerPromptEnum.PRE_ROLL, 0, 0, PlayerPromptSourceTypeEnum.NONE);

                yield return StartCoroutine (AnimationController.Instance.PlayerPreRollPromptEvent (playerPreRollPromptEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_PRE_ROLL_ROLL_DICE: {
                _preRollTimeOut = true;
                var playerRollDiceEvent = (PlayerRollDiceGameEventDto)gameEvent;
                Debug.Log ("ROLLING");
                yield return AnimationController.Instance.PlayerRollDiceEvent (playerRollDiceEvent.playerId, playerRollDiceEvent.totalRollBoost);

                if (CheckIfDouble (playerRollDiceEvent.diceResults)) {
                    matchClient.PlayerTickDoubleRoll (playerRollDiceEvent.playerId);
                }

                foreach (UInt16 result in playerRollDiceEvent.diceResults) {
                    RollDiceAction?.Invoke (result, playerRollDiceEvent.playerId);
                    Debug.Log ("RESULTROLL " + result);
                }
                DisableRollUI ();
                yield return new WaitForSeconds (2f);
            }
            break;

            case GameEventTypeEnum.PLAYER_PRE_ROLL_USE_ITEM: {
                var playerUseItemEvent = (PlayerUseItemGameEventDto)gameEvent;
                matchClient.GetPlayerWithId (playerUseItemEvent.playerId, out var playerItem);
                playerItem.UseItemId (playerUseItemEvent.itemId);
                if (matchClient.GetItemWithId (playerUseItemEvent.itemId, out var item)) {
                    yield return StartCoroutine (AnimationController.Instance.PlayerUseItemEvent (item.ItemType));
                }
                //DisablePlayerInteraction(playerUseItemEvent.playerId);
            }
            break;

            case GameEventTypeEnum.PLAYER_BUY_PROPERTY_PROMPT: {
                //Note these methods needs to specify the currentPlayerPrompt from the currentObjectIdPrompt
                var playerBuyPropertyPromptEvent = (PlayerBuyPropertyPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (playerBuyPropertyPromptEvent.playerId, PlayerPromptEnum.PROPERTY_TILE_BUY_PROPERTY, playerBuyPropertyPromptEvent.propertyTileId, playerBuyPropertyPromptEvent.promptSourceId, playerBuyPropertyPromptEvent.promptSourceType);
                if (!matchClient.GetTileWithId (playerBuyPropertyPromptEvent.propertyTileId,
                        out var tile) || tile.TileType != TileTypeEnum.PROPERTY) {
                    break;
                }


                tileIDPrompt = playerBuyPropertyPromptEvent.propertyTileId;

                var propertyTile = (PropertyTileGameModel)tile;
                yield return StartCoroutine (AnimationController.Instance.PlayerBuyPropertyPromptEvent (playerBuyPropertyPromptEvent.playerId));
                DisableRollUI ();
            }
            break;

            case GameEventTypeEnum.PLAYER_BUY_PROPERTY_CONFIRM: {
                var playerBuyPropertyConfirmEvent = (PlayerBuyPropertyConfirmGameEventDto)gameEvent;
                AnimationController.Instance.ResetTilePosition ();
                AnimationController.Instance.CheckColorMonopoly (playerBuyPropertyConfirmEvent.playerId);
                //DisablePlayerInteraction(playerBuyPropertyConfirmEvent.playerId);
                yield return StartCoroutine (AnimationController.Instance.PlayerBuyPropertyConfirmEvent (playerBuyPropertyConfirmEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_BUY_PROPERTY_DECLINE: {
                _playerBuyPropertyTimeOut = true;
                var playerBuyPropertyDeclineEvent = (PlayerBuyPropertyDeclineGameEventDto)gameEvent;
                yield return StartCoroutine (AnimationController.Instance.PlayerBuyPropertyDeclineEvent (playerBuyPropertyDeclineEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_UPGRADE_PROPERTY_PROMPT: {
                //Note these methods needs to specify the currentPlayerPrompt from the currentObjectIdPrompt
                var playerUpgradePropertyPromptEvent = (PlayerUpgradePropertyPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (playerUpgradePropertyPromptEvent.playerId, PlayerPromptEnum.PROPERTY_TILE_UPGRADE_PROPERTY, playerUpgradePropertyPromptEvent.propertyTileId, playerUpgradePropertyPromptEvent.promptSourceId, playerUpgradePropertyPromptEvent.promptSourceType);
                DisableRollUI ();
                tileIDPrompt = playerUpgradePropertyPromptEvent.propertyTileId;
                yield return StartCoroutine (AnimationController.Instance.PlayerUpgradePropertyPromptEvent (playerUpgradePropertyPromptEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_UPGRADE_PROPERTY_CONFIRM: {
                var playerUpgradePropertyConfirmEvent = (PlayerUpgradePropertyConfirmGameEventDto)gameEvent;
                yield return StartCoroutine (AnimationController.Instance.PlayerUpgradePropertyConfirmEvent (playerUpgradePropertyConfirmEvent.playerId, playerUpgradePropertyConfirmEvent.propertyTileId));
                AnimationController.Instance.remoteUpgrade = false;
            }
            break;

            case GameEventTypeEnum.PLAYER_UPGRADE_PROPERTY_DECLINE: {
                _playerUpgradePropertyTimeOut = true;
                var playerUpgradePropertyDeclineEvent = (PlayerUpgradePropertyDeclineGameEventDto)gameEvent;
                yield return StartCoroutine (AnimationController.Instance.PlayerUpgradePropertyDeclineEvent (playerUpgradePropertyDeclineEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_BUYOUT_PROPERTY_PROMPT: {
                var playerBuyOutPropertyPromptEvent = (PlayerBuyoutPropertyPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (playerBuyOutPropertyPromptEvent.playerId, PlayerPromptEnum.PROPERTY_TILE_BUYOUT_PROPERTY, playerBuyOutPropertyPromptEvent.propertyTileId, playerBuyOutPropertyPromptEvent.promptSourceId, playerBuyOutPropertyPromptEvent.promptSourceType);
                DisableRollUI ();
                yield return StartCoroutine (AnimationController.Instance.PlayerBuyoutPropertyPromptEvent (playerBuyOutPropertyPromptEvent.playerId, playerBuyOutPropertyPromptEvent.currentOwnerPlayerId, playerBuyOutPropertyPromptEvent.propertyTileId));
                tileIDPrompt = playerBuyOutPropertyPromptEvent.propertyTileId;
            }
            break;

            case GameEventTypeEnum.PLAYER_BUYOUT_PROPERTY_CONFIRM: {
                var playerBuyOutPropertyConfirmEvent = (PlayerBuyoutPropertyConfirmGameEventDto)gameEvent;
                yield return StartCoroutine (AnimationController.Instance.PlayerBuyoutPropertyConfirmEvent (playerBuyOutPropertyConfirmEvent.playerId,
                   playerBuyOutPropertyConfirmEvent.propertyTileId, playerBuyOutPropertyConfirmEvent.oldOwnerPlayerId));
                AnimationController.Instance.CheckColorMonopoly (playerBuyOutPropertyConfirmEvent.playerId);
                AnimationController.Instance.CheckColorMonopoly (playerBuyOutPropertyConfirmEvent.oldOwnerPlayerId);
                //DisablePlayerInteraction(playerBuyOutPropertyConfirmEvent.playerId);
            }
            break;

            case GameEventTypeEnum.PLAYER_BUYOUT_PROPERTY_DECLINE: {
                _playerBuyoutPropertyTimeOut = true;
                var playerBuyOutPropertyDeclineEvent = (PlayerBuyoutPropertyDeclineGameEventDto)gameEvent;
                yield return StartCoroutine (AnimationController.Instance.PlayerBuyoutPropertyDeclineEvent (playerBuyOutPropertyDeclineEvent.playerId));
                tileIDPrompt = 0;
            }
            break;

            case GameEventTypeEnum.PLAYER_BUY_SHOP_ITEMS_PROMPT: {
                var playerBuyShopItemsPromptEvent = (PlayerBuyShopItemsPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (playerBuyShopItemsPromptEvent.playerId, PlayerPromptEnum.SHOP_BUY_ITEMS, playerBuyShopItemsPromptEvent.shopTileId, 0, PlayerPromptSourceTypeEnum.NONE);
                DisableRollUI ();
                yield return StartCoroutine (AnimationController.Instance.PlayerBuyShopItemsPromptEvent (playerBuyShopItemsPromptEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_BUY_SHOP_ITEMS_CONFIRM: {
                var playerBuyShopItemsConfirmEvent = (PlayerBuyShopItemsConfirmGameEventDto)gameEvent;
                yield return StartCoroutine (AnimationController.Instance.PlayerBuyShopItemsConfirmEvent (playerBuyShopItemsConfirmEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_BUY_SHOP_ITEMS_DECLINE: {
                _playerBuyShopItemsTimeOut = true;
                var playerBuyShopItemsDeclineEvent = (PlayerBuyShopItemsDeclineGameEventDto)gameEvent;
                yield return StartCoroutine (AnimationController.Instance.PlayerBuyShopItemsDeclineEvent (playerBuyShopItemsDeclineEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_JAIL_GET_IN: {
                var playerJailGetInEvent = (PlayerJailGetInGameEventDto)gameEvent;
                matchClient.PlayerGetInJail (playerJailGetInEvent.playerId, playerJailGetInEvent.jailTurnCount);
                DisableRollUI ();
                yield return PlayerMovement.Instance.PlayerLandOnTileCoroutine (playerJailGetInEvent.jailTileId, playerJailGetInEvent.playerId);
                yield return StartCoroutine (AnimationController.Instance.PlayerJailGetInEvent (playerJailGetInEvent.playerId, playerJailGetInEvent.jailTurnCount));
            }
            break;

            case GameEventTypeEnum.PLAYER_JAIL_GET_OUT: {
                var playerJailGetOutEvent = (PlayerJailGetOutGameEventDto)gameEvent;
                matchClient.PlayerGetOutOfJail (playerJailGetOutEvent.playerId);
                DisablePlayerInteraction (playerJailGetOutEvent.playerId);
                yield return StartCoroutine (AnimationController.Instance.PlayerJailGetOutEvent (playerJailGetOutEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_JAIL_BAIL_OUT_PROMPT: {
                var playerJailBailOutPromptEvent = (PlayerJailBailOutPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (playerJailBailOutPromptEvent.playerId, PlayerPromptEnum.JAIL_TILE_BAIL_OUT, playerJailBailOutPromptEvent.jailTileId, 0, PlayerPromptSourceTypeEnum.NONE);
                DisableRollUI ();
                yield return StartCoroutine (AnimationController.Instance.PlayerJailBailoutPromptEvent (playerJailBailOutPromptEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_JAIL_BAIL_OUT_CONFIRM: {
                var playerJailBailOutConfirmEvent = (PlayerJailBailOutConfirmGameEventDto)gameEvent;
                DisablePlayerInteraction (playerJailBailOutConfirmEvent.playerId);
                yield return StartCoroutine (AnimationController.Instance.PlayerJailBailoutConfirmEvent (playerJailBailOutConfirmEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_JAIL_BAIL_OUT_DECLINE: {
                _playerJailBailOutTimeOut = true;
                var playerJailBailOutDeclineEvent = (PlayerJailBailOutDeclineGameEventDto)gameEvent;
                DisablePlayerInteraction (playerJailBailOutDeclineEvent.playerId);
                yield return StartCoroutine (AnimationController.Instance.PlayerJailBailoutDeclineEvent (playerJailBailOutDeclineEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_CARD_TRIGGERED: {
                var playerCardTriggeredEvent = (PlayerCardTriggeredGameEventDto)gameEvent;
                chanceCardID = playerCardTriggeredEvent.cardEffectId;
                yield return StartCoroutine (AnimationController.Instance.PlayerCardTriggeredEvent (playerCardTriggeredEvent.cardEffectId));
            }
            break;

            case GameEventTypeEnum.PLAYER_CARD_TARGET_PROMPT: {
                var playerCardTargetPromptEvent = (PlayerCardTargetPromptGameEventDto)gameEvent;
                matchClient.GetCardEffectWithId (playerCardTargetPromptEvent.cardEffectId, out var cardEffect);
                matchClient.PlayerSetTurnPrompt (playerCardTargetPromptEvent.playerId, PlayerPromptEnum.CARD_TARGET, playerCardTargetPromptEvent.cardEffectId, 0, PlayerPromptSourceTypeEnum.NONE);
                chanceCardID = playerCardTargetPromptEvent.cardEffectId;
                DisableRollUI ();
                yield return StartCoroutine (AnimationController.Instance.PlayerCardTargetPromptEvent (playerCardTargetPromptEvent.playerId, playerCardTargetPromptEvent.cardEffectId, cardEffect.TargetEntityType));
            }
            break;

            case GameEventTypeEnum.PLAYER_CARD_TARGET_CONFIRM: {
                var playerCardTargetConfirmEvent = (PlayerCardTargetConfirmGameEventDto)gameEvent;
                //DisablePlayerInteraction(playerCardTargetConfirmEvent.playerId);
                yield return StartCoroutine (AnimationController.Instance.PlayerCardTargetConfirmedEvent (playerCardTargetConfirmEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_CARD_TARGET_DECLINE: {
                _playerCardTargetTimeOut = true;
                var playerCardTargetDecline = (PlayerCardTargetDeclineGameEventDto)gameEvent;
                //DisablePlayerInteraction(playerCardTargetDecline.playerId);
                yield return StartCoroutine (AnimationController.Instance.PlayerCardTargetDeclinedEvent (playerCardTargetDecline.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_MORTGAGE_PROMPT: {
                var playerMortgagePromptEvent = (PlayerMortgagePromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (playerMortgagePromptEvent.playerId, PlayerPromptEnum.MORTGAGE, 0, 0, PlayerPromptSourceTypeEnum.NONE);
                yield return StartCoroutine (AnimationController.Instance.PlayerMortgagePromptEvent (playerMortgagePromptEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_MORTGAGE_SELL_PROPERTY: {
                var playerMortgageSellPropertyEvent = (PlayerMortgageSellPropertyGameEventDto)gameEvent;
                yield return StartCoroutine (AnimationController.Instance.PlayerMortgageSellPropertyEvent (playerMortgageSellPropertyEvent.playerId, playerMortgageSellPropertyEvent.propertyTileId));
                AnimationController.Instance.CheckColorMonopoly (playerMortgageSellPropertyEvent.playerId);
                tileIDPrompt = playerMortgageSellPropertyEvent.propertyTileId;
            }
            break;

            case GameEventTypeEnum.PLAYER_MORTGAGE_DECLARE_BANKRUPT: {
                _playerMortgageTimeOut = true;
                var playerMortgageDeclareBankruptEvent = (PlayerMortgageDeclareBankruptGameEventDto)gameEvent;
                yield return StartCoroutine (AnimationController.Instance.PlayerMortgageDeclareBankruptEvent (playerMortgageDeclareBankruptEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_TURN_ORDER_ROLL_PROMPT: {
                if (!_playerAssigned)
                    AssignPlayers ();

                var playerTurnOrderRollPromptEvent = (PlayerTurnOrderRollPromptGameEventDto)gameEvent;
                Debug.Log ("PROMPT");
                foreach (var waitPlayer in eventPlayerLists) {
                    matchClient.PlayerSetTurnPrompt (waitPlayer.playerId, PlayerPromptEnum.WAITING, playerTurnOrderRollPromptEvent.playerId, 0, PlayerPromptSourceTypeEnum.NONE);
                }
                matchClient.PlayerSetTurnPrompt (playerTurnOrderRollPromptEvent.playerId, PlayerPromptEnum.TURN_ORDER_ROLLING, playerTurnOrderRollPromptEvent.playerId, 0, PlayerPromptSourceTypeEnum.NONE);
                DisablePlayerInteraction (playerTurnOrderRollPromptEvent.playerId);
                yield return StartCoroutine (AnimationController.Instance.MachPlayerTurnStartEventCoroutine (playerTurnOrderRollPromptEvent.playerId, rollButton));
                yield return StartCoroutine (AnimationController.Instance.PlayerTurnOrderRollPromptEvent (playerTurnOrderRollPromptEvent.playerId));
            }
            break;

            case GameEventTypeEnum.PLAYER_TURN_ORDER_ROLL_DICE_ROLL: {
                _turnOrderTimeOut = true;
                var playerTurnOrderDiceRollEvent = (PlayerTurnOrderRollDiceRollGameEventDto)gameEvent;

                yield return AnimationController.Instance.PlayerTurnOrderDiceRollEvent (playerTurnOrderDiceRollEvent.playerId);

                foreach (UInt16 result in playerTurnOrderDiceRollEvent.diceResults) {
                    RollDiceAction?.Invoke (result, playerTurnOrderDiceRollEvent.playerId);
                    Debug.Log ("RASULT " + result);
                }

                yield return new WaitForSeconds (2f);
            }

            break;
            case GameEventTypeEnum.PLAYER_TURN_ORDER_ESTABLISHED: {
                var playerTurnOrderEstablishedEvent = (PlayerTurnOrderEstablishedGameEventDto)gameEvent;
                matchClient.SetPlayerIdTurnOrder (playerTurnOrderEstablishedEvent.playerIdTurnOrder);
                isGameStarting = true;
                DisableRollUI ();
                preRollUI.SetActive (false);
                yield return AnimationController.Instance.PlayerTurnOrderEstablishedEvent (playerTurnOrderEstablishedEvent.playerIdTurnOrder);
            }
            break;

            //=============================================== Tile Game Events ===================================================//

            case GameEventTypeEnum.TILE_PROPERTY_BUY: {
                StopCoroutine (RollCoroutine (fillDuration));
                var tilePropertyBuyEvent = (TilePropertyBuyGameEventDto)gameEvent;
                matchClient.TileChangeOwner (tilePropertyBuyEvent.tileId, tilePropertyBuyEvent.newOwnerPlayerId);
                matchClient.PropertyTileChangeUpgradeLevel (tilePropertyBuyEvent.tileId, tilePropertyBuyEvent.upgradeLevel);
                AnimationController.Instance.tilePropertyBuyUI.SetActive (false);
                yield return StartCoroutine (AnimationController.Instance.TilePropertyBuyEvent (tilePropertyBuyEvent.newOwnerPlayerId, tilePropertyBuyEvent.tileId, tilePropertyBuyEvent.upgradeLevel));
                yield return AnimationController.Instance.PlayerGainAndLosePoints (tilePropertyBuyEvent.newOwnerPlayerId);
                AnimationController.Instance.CheckColorMonopoly (tilePropertyBuyEvent.newOwnerPlayerId);
                AnimationController.Instance.freePropertyUpgrade = false;
            }
            break;

            case GameEventTypeEnum.TILE_PROPERTY_UPGRADE: {
                StopCoroutine (RollCoroutine (fillDuration));
                var tilePropertyUpgradeEvent = (TilePropertyUpgradeGameEventDto)gameEvent;
                matchClient.PropertyTileChangeUpgradeLevel (tilePropertyUpgradeEvent.tileId, tilePropertyUpgradeEvent.newUpgradeLevel);

                yield return StartCoroutine (AnimationController.Instance.TilePropertyUpgradeEvent (tilePropertyUpgradeEvent.ownerPlayerId, tilePropertyUpgradeEvent.tileId));
                yield return AnimationController.Instance.PlayerGainAndLosePoints (tilePropertyUpgradeEvent.ownerPlayerId);
            }
            break;

            case GameEventTypeEnum.TILE_PROPERTY_BUYOUT: {
                StopCoroutine (RollCoroutine (fillDuration));
                var tilePropertyBuyoutEvent = (TilePropertyBuyoutGameEventDto)gameEvent;
                matchClient.TileChangeOwner (tilePropertyBuyoutEvent.tileId, tilePropertyBuyoutEvent.newOwnerPlayerId);
                yield return StartCoroutine (AnimationController.Instance.TilePropertyBuyoutEvent (tilePropertyBuyoutEvent.tileId, tilePropertyBuyoutEvent.newOwnerPlayerId));
                yield return AnimationController.Instance.PlayerGainAndLosePoints (tilePropertyBuyoutEvent.oldOwnerPlayerId);
                yield return AnimationController.Instance.PlayerGainAndLosePoints (tilePropertyBuyoutEvent.newOwnerPlayerId);
                yield return AnimationController.Instance.OlympticTileEnd (tilePropertyBuyoutEvent.tileId);
                AnimationController.Instance.CheckColorMonopoly (tilePropertyBuyoutEvent.newOwnerPlayerId);
                AnimationController.Instance.CheckColorMonopoly (tilePropertyBuyoutEvent.oldOwnerPlayerId);
            }
            break;
            //Bank Collect =========================================
            case GameEventTypeEnum.TILE_BANK_COLLECT_CHIPS: {
                var tileBankCollectChipsEvent = (TileBankCollectChipsGameEventDto)gameEvent;
                matchClient.BankTileGainChips (tileBankCollectChipsEvent.chipsAmount);
                yield return StartCoroutine (AnimationController.Instance.TileBankCollectChipsEvent (tileBankCollectChipsEvent.giverPlayerId, tileBankCollectChipsEvent.chipsAmount));
            }
            break;

            case GameEventTypeEnum.TILE_BANK_REWARD_CHIPS: {
                var tileBankRewardChipsEvent = (TileBankRewardChipsGameEventDto)gameEvent;
                matchClient.BankTileLoseChips ();
                yield return StartCoroutine (AnimationController.Instance.TileBankRewardChipsEvent (tileBankRewardChipsEvent.receiverPlayerId, tileBankRewardChipsEvent.chipsAmount));
            }
            break;
            //===================================== 
            //=========================== Go Tile Remote Upgrade
            case GameEventTypeEnum.GO_TILE_REMOTE_UPGRADE_TARGET_CONFIRM: {
                var goTileRemoteUpgradeTargetConfirmEvent = (GoTileRemoteUpgradeTargetConfirmGameEventDto)gameEvent;
                yield return StartCoroutine (AnimationController.Instance.GoTileRemoteUpgradeTargetConfirmEvent (goTileRemoteUpgradeTargetConfirmEvent.playerId));
                //DisablePlayerInteraction(goTileRemoteUpgradeTargetConfirmEvent.playerId);
                yield return AnimationController.Instance.PlayerGainAndLosePoints (goTileRemoteUpgradeTargetConfirmEvent.playerId);
            }
            break;

            case GameEventTypeEnum.GO_TILE_REMOTE_UPGRADE_TARGET_DECLINE: {
                _goTileRemoteUpgradeTimeOut = true;
                var goTileRemoteUpgradeTargetDeclineEvent = (GoTileRemoteUpgradeTargetDeclineGameEventDto)gameEvent;
                //DisablePlayerInteraction(goTileRemoteUpgradeTargetDeclineEvent.playerId);
                yield return StartCoroutine (AnimationController.Instance.GoTileRemoteUpgradeTargetDeclineEvent (goTileRemoteUpgradeTargetDeclineEvent.playerId));
            }
            break;

            case GameEventTypeEnum.GO_TILE_REMOTE_UPGRADE_TARGET_PROMPT: {
                var goTileRemoteUpgradeTargetPromptEvent = (GoTileRemoteUpgradeTargetPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (goTileRemoteUpgradeTargetPromptEvent.playerId, PlayerPromptEnum.GO_START_TILE_UPGRADE_TARGET, 0, 0, PlayerPromptSourceTypeEnum.NONE);
                DisableRollUI ();
                yield return StartCoroutine (AnimationController.Instance.GoTileRemoteUpgradeTargetPromptEvent (goTileRemoteUpgradeTargetPromptEvent.playerId));
            }
            break;
            //============================
            case GameEventTypeEnum.TILE_SHOP_UPDATE_SHOP_ITEMS: {
                var shopTileUpdateShopItems = (TileShopUpdateShopItemsGameEventDto)gameEvent;
                matchClient.ShopTileUpdateOfferedItems (shopTileUpdateShopItems.offeredItemIds);
            }
            break;

            case GameEventTypeEnum.TILE_SHOP_ITEM_BOUGHT: {
                var shopTileItemBought = (TileShopItemBoughtGameEventDto)gameEvent;
                //yield return StartCoroutine(AnimationController.Instance.PlayerUseItemEvent(shopTileItemBought.playerId));
            }
            break;
            case GameEventTypeEnum.PLAYER_DISCONNECT: {
            }
            //Add that the player will get removed from the board after it dc's
            break;
            case GameEventTypeEnum.PLAYER_RECONNECT: {
                var playerReconnectGameEventDTO = (PlayerReconnectGameEventDto)gameEvent;
                DisablePlayerInteraction (playerReconnectGameEventDTO.playerIgid);
            }
            break;
            case GameEventTypeEnum.PLAYER_ABANDON: {
            }
            break;
            case GameEventTypeEnum.MATCH_TURN_LIMIT_REACHED: {
            }
            break;
            case GameEventTypeEnum.MATCH_TIME_UPDATE: {
                var matchGameUpdateGameEvent = (MatchTimeUpdateGameEventDto)gameEvent;
                matchClient.SetCurrentPromptTimeMs (matchGameUpdateGameEvent.promptTimeMs);
                matchClient.SetGameTimeMs (matchGameUpdateGameEvent.gameTimeMs);
                yield return StartCoroutine (AnimationController.Instance.SetPromptTime (matchGameUpdateGameEvent.promptTimeMs));

                if (!_playerAssigned)
                   AssignPlayers();

                }
            break;
            case GameEventTypeEnum.MATCH_TIME_LIMIT_REACHED: {
            }
            break;
            case GameEventTypeEnum.TILE_PROPERTY_SELL: {
                var tilePropertySellGameEventDTO = (TilePropertySellGameEventDto)gameEvent;
                matchClient.TileRemoveOwner (tilePropertySellGameEventDTO.tileId);
                matchClient.PropertyTileChangeUpgradeLevel (tilePropertySellGameEventDTO.tileId, PropertyUpgradeLevelEnum.NONE);
                yield return AnimationController.Instance.PlayerGainAndLosePoints (tilePropertySellGameEventDTO.sellerPlayerId);
                yield return AnimationController.Instance.PlayerMortgageSellPropertyEvent (tilePropertySellGameEventDTO.sellerPlayerId, tilePropertySellGameEventDTO.tileId);
            }
            break;
            //Timeout Events ======================================================
            case GameEventTypeEnum.PLAYER_TURN_ORDER_ROLL_TIMEOUT: {
                _turnOrderTimeOut = false;
            }
            break;
            case GameEventTypeEnum.PLAYER_PRE_ROLL_TIMEOUT: {
                _preRollTimeOut = false;
            }
            break;
            case GameEventTypeEnum.PLAYER_BUY_PROPERTY_TIMEOUT: {
                _playerBuyPropertyTimeOut = false;
            }
            break;
            case GameEventTypeEnum.PLAYER_UPGRADE_PROPERTY_TIMEOUT: {
                _playerUpgradePropertyTimeOut = false;
            }
            break;
            case GameEventTypeEnum.PLAYER_BUYOUT_PROPERTY_TIMEOUT: {
                _playerBuyoutPropertyTimeOut = false;
            }
            break;
            case GameEventTypeEnum.PLAYER_BUY_SHOP_ITEMS_TIMEOUT: {
                _playerBuyShopItemsTimeOut = false;
            }
            break;
            case GameEventTypeEnum.PLAYER_JAIL_BAIL_OUT_TIMEOUT: {
                _playerJailBailOutTimeOut = false;
            }
            break;
            case GameEventTypeEnum.PLAYER_CARD_TARGET_TIMEOUT: {
                _playerCardTargetTimeOut = false;
            }
            break;
            case GameEventTypeEnum.GO_TILE_REMOTE_UPGRADE_TARGET_TIMEOUT: {
                _goTileRemoteUpgradeTimeOut = false;
            }
            break;
            case GameEventTypeEnum.PLAYER_MORTGAGE_TIMEOUT: {
                _playerMortgageTimeOut = false;
            }
            break;
            case GameEventTypeEnum.OLYMPICS_TILE_TARGET_TIMEOUT: {
                _olympicEventTimeout = false;
            }
            break;
            case GameEventTypeEnum.WORLD_TRAVEL_TILE_TARGET_TIMEOUT: {
                _worldTileEventTimeout = false;
            }
            break;
            //=============================================================================
            case GameEventTypeEnum.MATCH_TILE_FESTIVAL_START: {
                var matchTileFestivalStart = (MatchTileFestivalStartGameEventDto)gameEvent;
                matchClient.StartTileFestival (matchTileFestivalStart.propertyTileIds);
                yield return AnimationController.Instance.SetFestivalTiles (matchTileFestivalStart.propertyTileIds);
            }
            break;
            case GameEventTypeEnum.MATCH_TILE_FESTIVAL_END: {
                var matchTileFestivalEnd = (MatchTileFestivalEndGameEventDto)gameEvent;
                matchClient.EndTileFestival (matchTileFestivalEnd.propertyTileIds);
            }
            //Just in case 
            break;
            //=================================
            //=============Olympics Tile Target
            case GameEventTypeEnum.OLYMPICS_TILE_TARGET_PROMPT: {
                DisableRollUI ();
                var olympicTilePromptGameEventDTO = (OlympicsTileTargetPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (olympicTilePromptGameEventDTO.playerId, PlayerPromptEnum.OLYMPICS_TARGET, 0, 0, PlayerPromptSourceTypeEnum.NONE);
                yield return AnimationController.Instance.OlympticTilePrompt (olympicTilePromptGameEventDTO.playerId);
            }
            break;
            case GameEventTypeEnum.OLYMPICS_TILE_TARGET_CONFIRM: {
                DisableRollUI ();
                var olympicTileConfirmGameEventDTO = (OlympicsTileTargetConfirmGameEventDto)gameEvent;
                yield return AnimationController.Instance.OlympicTileConfirm (olympicTileConfirmGameEventDTO.playerId, olympicTileConfirmGameEventDTO.propertyTileId);
            }
            break;
            case GameEventTypeEnum.OLYMPICS_TILE_TARGET_DECLINE: {
                DisableRollUI ();
                _olympicEventTimeout = true;
                var olympicTileDeclineGameEventDTO = (OlympicsTileTargetDeclineGameEventDto)gameEvent;
                yield return AnimationController.Instance.OlympicTileDecline (olympicTileDeclineGameEventDTO.playerId);
            }
            break;
            //=============
            //===========Match Tile Olympics Start and End
            case GameEventTypeEnum.MATCH_TILE_OLYMPICS_START: {
                var olympicsStartGameEventDTO = (MatchTileOlympicsStartGameEventDto)gameEvent;
                matchClient.StartTileOlympics (olympicsStartGameEventDTO.propertyTileId);
                yield return AnimationController.Instance.OlympticTileStart (olympicsStartGameEventDTO.propertyTileId);
            }
            break;
            case GameEventTypeEnum.MATCH_TILE_OLYMPICS_END: {
                var olympicsEndGameEventDTO = (MatchTileOlympicsEndGameEventDto)gameEvent;
                matchClient.EndTileOlympics (olympicsEndGameEventDTO.propertyTileId);
                yield return AnimationController.Instance.OlympticTileEnd (olympicsEndGameEventDTO.propertyTileId);
            }
            break;
            //=================
            //===========Tax Collect
            case GameEventTypeEnum.TILE_TAX_COLLECT_TAX: {
                var taxCollectGameEventDTO = (TileTaxCollectTaxGameEventDto)gameEvent;
                yield return AnimationController.Instance.CollectTax (taxCollectGameEventDTO.taxedAmount);
            }

            break;
            //===========World Travel
            case GameEventTypeEnum.WORLD_TRAVEL_TILE_TARGET_PROMPT: {
                var worldTravelGameEventDTO = (WorldTravelTileTargetPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (worldTravelGameEventDTO.playerId, PlayerPromptEnum.WORLD_TRAVEL_TARGET, 0, 0, PlayerPromptSourceTypeEnum.NONE);
                yield return AnimationController.Instance.WorldTravelTilePrompt (worldTravelGameEventDTO.playerId);
                DisableRollUI ();
            }
            break;
            case GameEventTypeEnum.WORLD_TRAVEL_TILE_TARGET_CONFIRM: {
                var worldTravelConfirmGameEventDTO = (WorldTravelTileTargetConfirmGameEventDto)gameEvent;
                _worldTileEventTimeout = true;
                yield return AnimationController.Instance.WorldTravelTileConfirm (worldTravelConfirmGameEventDTO.playerId);
                //DisablePlayerInteraction(worldTravelConfirmGameEventDTO.playerId);
            }
            break;
            case GameEventTypeEnum.WORLD_TRAVEL_TILE_TARGET_DECLINE: {
                var worldTravelDeclineGameEventDTO = (WorldTravelTileTargetDeclineGameEventDto)gameEvent;
                _worldTileEventTimeout = true;
                yield return AnimationController.Instance.WorldTravelTileDecline (worldTravelDeclineGameEventDTO.playerId);
                //DisablePlayerInteraction(worldTravelDeclineGameEventDTO.playerId);
            }
            break;
            case GameEventTypeEnum.PLAYER_MISC_SURRENDER: {
                //Add support when a player quits
            }
            break;
            case GameEventTypeEnum.PLAYER_LAP_COUNT_UPDATE: {
                var playerLapCountUpdateGameEventDTO = (PlayerLapCountUpdateGameEventDto)gameEvent;
                matchClient.GetPlayerWithId (playerLapCountUpdateGameEventDTO.playerId, out var player);
                player.SetLapCount (playerLapCountUpdateGameEventDTO.lapCount);
                StartCoroutine (AnimationController.Instance.PlayerLapCounter (playerLapCountUpdateGameEventDTO.playerId, playerLapCountUpdateGameEventDTO.lapCount));
                if (playerLapCountUpdateGameEventDTO.lapCount < 2)
                    ChangeUpgradeLevel (playerLapCountUpdateGameEventDTO.lapCount);
            }
            break;

            //==========Sky Blue Holiday Tile
            case GameEventTypeEnum.TILE_OWNER_HOLIDAY_BUY: {
                var tileOwnerHolidayBuy = (TileOwnerHolidayBuyGameEventDto)gameEvent;
                //The Action of the tile being bought
                matchClient.TileChangeOwner (tileOwnerHolidayBuy.tileId, tileOwnerHolidayBuy.newOwnerPlayerId);
                yield return AnimationController.Instance.TilePropertyBuyEventForHolidayAndVisit (tileOwnerHolidayBuy.newOwnerPlayerId, tileOwnerHolidayBuy.tileId, PropertyUpgradeLevelEnum.LANDMARK);
                yield return AnimationController.Instance.PlayerGainAndLosePoints (tileOwnerHolidayBuy.newOwnerPlayerId);
            }
            break;
            case GameEventTypeEnum.TILE_OWNER_HOLIDAY_BUYOUT: {
                var tileOwnerHolidayBuyoutGameEventDTO = (TileOwnerHolidayBuyoutGameEventDto)gameEvent;
                //The Action of the tile being bought
                matchClient.TileChangeOwner (tileOwnerHolidayBuyoutGameEventDTO.tileId, tileOwnerHolidayBuyoutGameEventDTO.newOwnerPlayerId);
                yield return AnimationController.Instance.PlayerHolidayAndVisitBuyoutPropertyEvent (tileOwnerHolidayBuyoutGameEventDTO.newOwnerPlayerId, tileOwnerHolidayBuyoutGameEventDTO.tileId, tileOwnerHolidayBuyoutGameEventDTO.oldOwnerPlayerId);
                AnimationController.Instance.CheckColorMonopoly (tileOwnerHolidayBuyoutGameEventDTO.oldOwnerPlayerId);
                AnimationController.Instance.CheckColorMonopoly (tileOwnerHolidayBuyoutGameEventDTO.newOwnerPlayerId);
            }
            break;
            case GameEventTypeEnum.TILE_OWNER_HOLIDAY_SELL: {
                var tileOwnerHolidaySellGameEventDTO = (TileOwnerHolidaySellGameEventDto)gameEvent;
                //The Action of the tile being bought
                matchClient.TileRemoveOwner (tileOwnerHolidaySellGameEventDTO.tileId);
                yield return AnimationController.Instance.PlayerHolidayAndVisitMortgageSellPropertyEvent (tileOwnerHolidaySellGameEventDTO.sellerPlayerId, tileOwnerHolidaySellGameEventDTO.tileId);
                yield return AnimationController.Instance.PlayerGainAndLosePoints (tileOwnerHolidaySellGameEventDTO.sellerPlayerId);
            }
            break;
            case GameEventTypeEnum.PLAYER_BUY_OWNER_HOLIDAY_PROMPT: {
                var playerBuyOwnerHolidayPromptGameEventDTo = (PlayerBuyOwnerHolidayPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (playerBuyOwnerHolidayPromptGameEventDTo.playerId, PlayerPromptEnum.TILE_BUY_OWNER_HOLIDAY, 0, 0, PlayerPromptSourceTypeEnum.NONE);
                tileIDPrompt = playerBuyOwnerHolidayPromptGameEventDTo.holidayTileId;
                yield return AnimationController.Instance.PlayerBuyHolidayPrompt (playerBuyOwnerHolidayPromptGameEventDTo.playerId);
                DisableRollUI ();
            }
            break;
            case GameEventTypeEnum.PLAYER_BUY_OWNER_HOLIDAY_CONFIRM: {
                var playerBuyOwnerHolidayConfirmGameEventDTo = (PlayerBuyOwnerHolidayConfirmGameEventDto)gameEvent;
                yield return AnimationController.Instance.PlayerBuyHolidayConfirm (playerBuyOwnerHolidayConfirmGameEventDTo.playerId);
                yield return AnimationController.Instance.PlayerGainAndLosePoints (playerBuyOwnerHolidayConfirmGameEventDTo.playerId);
                _holidayTile = true;
                //DisablePlayerInteraction(playerBuyOwnerHolidayConfirmGameEventDTo.playerId);
            }
            break;
            case GameEventTypeEnum.PLAYER_BUY_OWNER_HOLIDAY_DECLINE: {
                var playerBuyOwnerHolidayDeclineGameEventDTo = (PlayerBuyOwnerHolidayDeclineGameEventDto)gameEvent;
                yield return AnimationController.Instance.PlayerBuyHolidayDecline (playerBuyOwnerHolidayDeclineGameEventDTo.playerId);
                _holidayTile = true;
                //   DisablePlayerInteraction(playerBuyOwnerHolidayDeclineGameEventDTo.playerId);
            }
            break;
            case GameEventTypeEnum.PLAYER_BUY_OWNER_HOLIDAY_TIMEOUT: {
                var playerBuyOwnerHolidayTimourGameEventDTo = (PlayerBuyOwnerHolidayTimeoutGameEventDto)gameEvent;
                _holidayTile = false;
            }
            break;
            case GameEventTypeEnum.PLAYER_BUYOUT_OWNER_HOLIDAY_PROMPT: {
                var playerBuyoutOwnerHolidayPromptGameEventDTo = (PlayerBuyoutOwnerHolidayPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (playerBuyoutOwnerHolidayPromptGameEventDTo.playerId, PlayerPromptEnum.TILE_BUYOUT_OWNER_HOLIDAY, 0, 0, PlayerPromptSourceTypeEnum.NONE);
                tileIDPrompt = playerBuyoutOwnerHolidayPromptGameEventDTo.holidayTileId;
                yield return AnimationController.Instance.PlayerHolidayBuyoutPropertyPromptEvent (playerBuyoutOwnerHolidayPromptGameEventDTo.playerId, playerBuyoutOwnerHolidayPromptGameEventDTo.currentOwnerPlayerId, playerBuyoutOwnerHolidayPromptGameEventDTo.holidayTileId);
                DisableRollUI ();
            }
            break;
            case GameEventTypeEnum.PLAYER_BUYOUT_OWNER_HOLIDAY_CONFIRM: {
                var playerButoutOwnerHolidayConfirmGameEventDTo = (PlayerBuyoutOwnerHolidayConfirmGameEventDto)gameEvent;
                //yield return AnimationController.Instance.PlayerHolidayAndVisitBuyoutPropertyEvent(playerButoutOwnerHolidayConfirmGameEventDTo.playerId, playerButoutOwnerHolidayConfirmGameEventDTo.holidayTileId, playerButoutOwnerHolidayConfirmGameEventDTo.oldOwnerPlayerId);
                yield return AnimationController.Instance.PlayerGainAndLosePoints (playerButoutOwnerHolidayConfirmGameEventDTo.playerId);
                _holidayTileBuyout = true;
                // DisablePlayerInteraction(playerButoutOwnerHolidayConfirmGameEventDTo.playerId);
            }
            break;
            case GameEventTypeEnum.PLAYER_BUYOUT_OWNER_HOLIDAY_DECLINE: {
                var playerBuyoutOwnerHolidayDeclineGameEventDTo = (PlayerBuyoutOwnerHolidayDeclineGameEventDto)gameEvent;
                yield return AnimationController.Instance.PlayerHolidayBuyoutPropertyDeclineEvent (playerBuyoutOwnerHolidayDeclineGameEventDTo.playerId);
                _holidayTileBuyout = true;
                // DisablePlayerInteraction(playerBuyoutOwnerHolidayDeclineGameEventDTo.playerId);
            }
            break;
            case GameEventTypeEnum.PLAYER_BUYOUT_OWNER_HOLIDAY_TIMEOUT: {
                var playerBuyoutOwnerHolidayTimeOutGameEventDTo = (PlayerBuyoutOwnerHolidayTimeoutGameEventDto)gameEvent;
                _holidayTileBuyout = false;
            }
            break;
            //==============================
            //==============================Pink Holiday Tile
            case GameEventTypeEnum.TILE_VISIT_HOLIDAY_BUY: {
                var tileVisintHolidayBuyGameEventDTO = (TileVisitHolidayBuyGameEventDto)gameEvent;
                matchClient.TileChangeOwner (tileVisintHolidayBuyGameEventDTO.tileId, tileVisintHolidayBuyGameEventDTO.newOwnerPlayerId);
                yield return AnimationController.Instance.TilePropertyBuyEventForHolidayAndVisit (tileVisintHolidayBuyGameEventDTO.newOwnerPlayerId, tileVisintHolidayBuyGameEventDTO.tileId, PropertyUpgradeLevelEnum.LANDMARK);
                yield return AnimationController.Instance.PlayerGainAndLosePoints (tileVisintHolidayBuyGameEventDTO.newOwnerPlayerId);
                DisableRollUI ();
            }
            break;
            case GameEventTypeEnum.TILE_VISIT_HOLIDAY_BUYOUT: {
                var tileVisitHolidayBuyoutGameEventDTO = (TileVisitHolidayBuyoutGameEventDto)gameEvent;
                matchClient.TileChangeOwner (tileVisitHolidayBuyoutGameEventDTO.tileId, tileVisitHolidayBuyoutGameEventDTO.newOwnerPlayerId);
                yield return AnimationController.Instance.PlayerHolidayAndVisitBuyoutPropertyEvent (tileVisitHolidayBuyoutGameEventDTO.newOwnerPlayerId, tileVisitHolidayBuyoutGameEventDTO.tileId, tileVisitHolidayBuyoutGameEventDTO.oldOwnerPlayerId);
                AnimationController.Instance.CheckColorMonopoly (tileVisitHolidayBuyoutGameEventDTO.newOwnerPlayerId);
                AnimationController.Instance.CheckColorMonopoly (tileVisitHolidayBuyoutGameEventDTO.oldOwnerPlayerId);
                DisableRollUI ();
            }
            break;
            case GameEventTypeEnum.TILE_VISIT_HOLIDAY_SELL: {
                var tileVisitHolidaySellGameEventDTo = (TileVisitHolidaySellGameEventDto)gameEvent;
                matchClient.TileRemoveOwner (tileVisitHolidaySellGameEventDTo.tileId);
                yield return AnimationController.Instance.PlayerHolidayAndVisitMortgageSellPropertyEvent (tileVisitHolidaySellGameEventDTo.sellerPlayerId, tileVisitHolidaySellGameEventDTo.tileId);
                DisableRollUI ();
            }
            break;
            case GameEventTypeEnum.TILE_VISIT_HOLIDAY_VISIT_COUNT_UPDATE: {
                var tileVisitHolidayCountUpdateGameEventDTo = (TileVisitHolidayVisitCountUpdateGameEventDto)gameEvent;
                matchClient.GetTileWithId (tileVisitHolidayCountUpdateGameEventDTo.tileId, out var tile);
                var visitHolidayTile = (VisitHolidayTileGameModel)tile;
                visitHolidayTile.SetVisitCount (tileVisitHolidayCountUpdateGameEventDTo.visitCount);
            }
            break;
            case GameEventTypeEnum.PLAYER_BUY_VISIT_HOLIDAY_PROMPT: {
                var playerBuyVisitHolidayPromptGameEventDTo = (PlayerBuyVisitHolidayPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (playerBuyVisitHolidayPromptGameEventDTo.playerId, PlayerPromptEnum.TILE_BUY_VISIT_HOLIDAY, 0, 0, PlayerPromptSourceTypeEnum.NONE);
                tileIDPrompt = playerBuyVisitHolidayPromptGameEventDTo.holidayTileId;
                yield return AnimationController.Instance.PlayerBuyHolidayVisitPrompt (playerBuyVisitHolidayPromptGameEventDTo.playerId);
                DisableRollUI ();
            }
            break;
            case GameEventTypeEnum.PLAYER_BUY_VISIT_HOLIDAY_CONFIRM: {
                var playerBuyVisitHolidayConfirmGameEventDTo = (PlayerBuyVisitHolidayConfirmGameEventDto)gameEvent;
                _holidayVisit = true;
                yield return AnimationController.Instance.PlayerBuyHolidayVisitConfirm (playerBuyVisitHolidayConfirmGameEventDTo.playerId);
                yield return AnimationController.Instance.PlayerGainAndLosePoints (playerBuyVisitHolidayConfirmGameEventDTo.playerId);
                // DisablePlayerInteraction(playerBuyVisitHolidayConfirmGameEventDTo.playerId);
            }
            break;
            case GameEventTypeEnum.PLAYER_BUY_VISIT_HOLIDAY_DECLINE: {
                var playerBuyVisitHolidayDeclineGameEventDTo = (PlayerBuyVisitHolidayDeclineGameEventDto)gameEvent;
                yield return AnimationController.Instance.PlayerBuyHolidayVisitDecline (playerBuyVisitHolidayDeclineGameEventDTo.playerId);
                _holidayVisit = true;
                //  DisablePlayerInteraction(playerBuyVisitHolidayDeclineGameEventDTo.playerId);
            }
            break;
            case GameEventTypeEnum.PLAYER_BUY_VISIT_HOLIDAY_TIMEOUT: {
                var playerBuyVisitHolidayTimeOutGameEventDTo = (PlayerBuyVisitHolidayTimeoutGameEventDto)gameEvent;
                _holidayVisit = false;
                DisableRollUI ();
            }
            break;
            case GameEventTypeEnum.PLAYER_BUYOUT_VISIT_HOLIDAY_PROMPT: {
                var playerBuyOutVisitHolidayPromptGameEventDTo = (PlayerBuyoutVisitHolidayPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (playerBuyOutVisitHolidayPromptGameEventDTo.playerId, PlayerPromptEnum.TILE_BUYOUT_VISIT_HOLIDAY, 0, 0, PlayerPromptSourceTypeEnum.NONE);
                tileIDPrompt = playerBuyOutVisitHolidayPromptGameEventDTo.holidayTileId;
                yield return AnimationController.Instance.PlayerHolidayVisitBuyoutPropertyPromptEvent (playerBuyOutVisitHolidayPromptGameEventDTo.playerId, playerBuyOutVisitHolidayPromptGameEventDTo.currentOwnerPlayerId, playerBuyOutVisitHolidayPromptGameEventDTo.holidayTileId);
                DisableRollUI ();
            }
            break;
            case GameEventTypeEnum.PLAYER_BUYOUT_VISIT_HOLIDAY_CONFIRM: {
                var playerBuyoutVisitHolidayConfirmGameEventDTo = (PlayerBuyoutVisitHolidayConfirmGameEventDto)gameEvent;
                _holidayVisitBuyout = true;
                //yield return AnimationController.Instance.PlayerHolidayVisitBuyoutPropertyConfirmEvent(playerBuyoutVisitHolidayConfirmGameEventDTo.playerId, playerBuyoutVisitHolidayConfirmGameEventDTo.holidayTileId, playerBuyoutVisitHolidayConfirmGameEventDTo.oldOwnerPlayerId);
                yield return AnimationController.Instance.PlayerGainAndLosePoints (playerBuyoutVisitHolidayConfirmGameEventDTo.playerId);
                // DisablePlayerInteraction(playerBuyoutVisitHolidayConfirmGameEventDTo.playerId);
            }
            break;
            case GameEventTypeEnum.PLAYER_BUYOUT_VISIT_HOLIDAY_DECLINE: {
                var playerBuyoutVisitHolidayDeclineGameEventDTo = (PlayerBuyoutVisitHolidayDeclineGameEventDto)gameEvent;
                _holidayVisitBuyout = true;
                yield return AnimationController.Instance.PlayerHolidayVisitBuyoutPropertyDeclineEvent (playerBuyoutVisitHolidayDeclineGameEventDTo.playerId);
                //  DisablePlayerInteraction(playerBuyoutVisitHolidayDeclineGameEventDTo.playerId);
            }
            break;
            case GameEventTypeEnum.PLAYER_BUYOUT_VISIT_HOLIDAY_TIMEOUT: {
                var playerBuyoutVisitHolidayTimeoutGameEventDTo = (PlayerBuyoutVisitHolidayTimeoutGameEventDto)gameEvent;
                _holidayVisitBuyout = false;
                DisableRollUI ();
            }
            break;
            case GameEventTypeEnum.TILE_TRANSFER_OWNERSHIP: {
                var tileTransferOwnershipGameEventDTO = (TileTransferOwnershipGameEventDto)gameEvent;
                matchClient.TileChangeOwner (tileTransferOwnershipGameEventDTO.tileId, tileTransferOwnershipGameEventDTO.newOwnerPlayerId);
                AnimationController.Instance.CheckColorMonopoly (tileTransferOwnershipGameEventDTO.newOwnerPlayerId);
                yield return AnimationController.Instance.PlayerGainAndLosePoints (tileTransferOwnershipGameEventDTO.newOwnerPlayerId);
                yield return AnimationController.Instance.ChangeTileOwnership (tileTransferOwnershipGameEventDTO.tileId, tileTransferOwnershipGameEventDTO.newOwnerPlayerId);
                DisableRollUI ();
            }
            break;
            case GameEventTypeEnum.PLAYER_JAIL_ESCAPE_CONFIRM: {
            }
            break;
            case GameEventTypeEnum.PLAYER_TURN_ORDER_ROLLING_START: {
            }
            break;
            case GameEventTypeEnum.CITY_DONATION_PROMPT: {
                var cityDonationPromptGameEventDTO = (CityDonationPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (cityDonationPromptGameEventDTO.playerId, PlayerPromptEnum.CITY_DONATION, 0, 0, PlayerPromptSourceTypeEnum.NONE);
                yield return AnimationController.Instance.CityDonationPrompt (cityDonationPromptGameEventDTO.playerId);
                DisableRollUI ();
            }
            break;
            case GameEventTypeEnum.CITY_DONATION_CONFIRM: {
                var cityDonationConfirmGameEventDTO = (CityDonationConfirmGameEventDto)gameEvent;
                yield return AnimationController.Instance.CityDonationConfirm (cityDonationConfirmGameEventDTO.playerId, cityDonationConfirmGameEventDTO.tileId);
                yield return AnimationController.Instance.PlayerGainAndLosePoints (cityDonationConfirmGameEventDTO.playerId);
                _cityDonation = true;
            }
            break;
            case GameEventTypeEnum.CITY_DONATION_TIMEOUT: {
                _cityDonation = false;
                DisableRollUI ();
            }
            break;
            case GameEventTypeEnum.CITY_SWAP_PROMPT: {
                var citySwapPromptGameEventDTO = (CitySwapPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (citySwapPromptGameEventDTO.playerId, PlayerPromptEnum.CITY_SWAP, 0, 0, PlayerPromptSourceTypeEnum.NONE);
                yield return AnimationController.Instance.CitySwapPrompt (citySwapPromptGameEventDTO.playerId);
                DisableRollUI ();
            }
            break;
            case GameEventTypeEnum.CITY_SWAP_CONFIRM: {
                var citySwapConfirmGameEventDTO = (CitySwapConfirmGameEventDto)gameEvent;
                _citySwap = true;
                yield return AnimationController.Instance.CitySwapConfirm (citySwapConfirmGameEventDTO.tileIdSelf, citySwapConfirmGameEventDTO.tileIdOther, citySwapConfirmGameEventDTO.playerId);
                yield return AnimationController.Instance.PlayerGainAndLosePoints (citySwapConfirmGameEventDTO.playerId);
                AnimationController.Instance.CheckColorMonopoly (citySwapConfirmGameEventDTO.playerId);
            }
            break;
            case GameEventTypeEnum.CITY_SWAP_DECLINE: {
                var citySwapDeclineGameEventDTO = (CitySwapDeclineGameEventDto)gameEvent;
                _citySwap = true;
                yield return AnimationController.Instance.CitySwapDecline ();
            }
            break;
            case GameEventTypeEnum.CITY_SWAP_TIMEOUT: {
                _citySwap = false;
                DisableRollUI ();
            }
            break;
            case GameEventTypeEnum.GAME_EVENT_TYPE_PREFIX: {
                // NON_IMPLEMENT
            }
            break;
            case GameEventTypeEnum.MATCH_PLAYER_TURN_RESUME: {
                // NON_IMPLEMENT
            }
            break;
            case GameEventTypeEnum.MATCH_PLAYER_TURN_PRIORITY_START: {
                var priorityTurnEventDto = (MatchPlayerTurnPriorityStartGameEventDto)gameEvent;
                matchClient.SetPriorityTurnPlayerId (priorityTurnEventDto.playerId);
            }
            break;
            case GameEventTypeEnum.MATCH_PLAYER_TURN_PRIORITY_END: {
                var priorityTurnEventDto = (MatchPlayerTurnPriorityEndGameEventDto)gameEvent;
                matchClient.SetPriorityTurnPlayerId (0uL);
            }
            break;
            case GameEventTypeEnum.FORCED_SALE_PROMPT: {
                var forcedSalePromptGameEventDTO = (ForcedSalePromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (forcedSalePromptGameEventDTO.playerId, PlayerPromptEnum.FORCED_SALE, 0, 0, PlayerPromptSourceTypeEnum.NONE);
                yield return AnimationController.Instance.ForcedSellPrompt (forcedSalePromptGameEventDTO.playerId);
            }
            break;
            case GameEventTypeEnum.FORCED_SALE_CONFIRM: {
                var forcedSaleConfirmGameEventDTO = (ForcedSaleConfirmGameEventDto)gameEvent;
                WebsocketHandler.Instance.matchClient.TileRemoveOwner(forcedSaleConfirmGameEventDTO.tileId);
                WebsocketHandler.Instance.matchClient.PropertyTileChangeUpgradeLevel (forcedSaleConfirmGameEventDTO.tileId, PropertyUpgradeLevelEnum.NONE);
                yield return AnimationController.Instance.ForcedSellConfirm (forcedSaleConfirmGameEventDTO.playerId, forcedSaleConfirmGameEventDTO.tileId);
                yield return AnimationController.Instance.PlayerGainAndLosePoints (forcedSaleConfirmGameEventDTO.playerId);
                AnimationController.Instance.CheckColorMonopoly (forcedSaleConfirmGameEventDTO.playerId);
            }
            break;
            case GameEventTypeEnum.FORCED_SALE_TIMEOUT: {
                var forcedSaleTimeoutGameEventDTO = (ForcedSaleTimeoutGameEventDto)gameEvent;
                yield return AnimationController.Instance.ForcedSellDecline (forcedSaleTimeoutGameEventDTO.playerId);
            }
            break;
            case GameEventTypeEnum.CASINO_BACCARAT_PROMPT: {
                var casinoBaccaratPromptGameEventDTO = (CasinoBaccaratPromptGameEventDto)gameEvent;
                matchClient.PlayerSetTurnPrompt (casinoBaccaratPromptGameEventDTO.playerId, PlayerPromptEnum.CASINO_BACCARAT, 0, 0, PlayerPromptSourceTypeEnum.NONE);
                yield return AnimationController.Instance.BaccaratPrompt (casinoBaccaratPromptGameEventDTO.playerId);
            }
            break;
            case GameEventTypeEnum.CASINO_BACCARAT_BET_CONFIRM: {
                var casinoBaccaratConfirmGameEventDTO = (CasinoBaccaratBetConfirmGameEventDto)gameEvent;
            }
            break;
            case GameEventTypeEnum.CASINO_BACCARAT_DECLINE: {
                var casinoBaccaratGameEventDecline = (CasinoBaccaratDeclineGameEventDto)gameEvent;
                yield return AnimationController.Instance.BaccaratDecline (casinoBaccaratGameEventDecline.playerId);
            }
            break;
            case GameEventTypeEnum.CASINO_BACCARAT_COMMENCE: {
                var casinoBaccaratCommenceGameEventDTO = (CasinoBaccaratCommenceGameEventDto)gameEvent;
                yield return AnimationController.Instance.BaccaratCommence (casinoBaccaratCommenceGameEventDTO.baccaratData, casinoBaccaratCommenceGameEventDTO.playerId);
            }
            break;
            case GameEventTypeEnum.CASINO_BACCARAT_RESULT: {
                var casinoBaccaratResultGameEventDTO = (CasinoBaccaratResultGameEventDto)gameEvent;
                yield return AnimationController.Instance.BaccaratResult (casinoBaccaratResultGameEventDTO.playerId, casinoBaccaratResultGameEventDTO.result, casinoBaccaratResultGameEventDTO.winAmountChips);
            }
            break;
            case GameEventTypeEnum.CASINO_BACCARAT_TIMEOUT: {
                var casinoBaccaratTimeOut = (CasinoBaccaratTimeoutGameEventDto)gameEvent;
            }
            break;
            case GameEventTypeEnum.MATCH_TIMER_PAUSE: {
                var matchTimerPauseGameEventDTO = (MatchTimerPauseGameEventDto)gameEvent;
                matchClient.SetIsTimerPaused (true);
                yield return AnimationController.Instance.SetPromptTime (matchTimerPauseGameEventDTO.promptTimeMs);
            }
            break;
            case GameEventTypeEnum.MATCH_TIMER_RESUME: {
                var matchTimeResumeGameEventDTO = (MatchTimerResumeGameEventDto)gameEvent;
                matchClient.SetIsTimerPaused (false);
                yield return AnimationController.Instance.SetPromptTime (matchTimeResumeGameEventDTO.promptTimeMs);
            }
            break;
            case GameEventTypeEnum.PLAYER_MORTGAGE_ADD_BUY_IN_ATTEMPT: {
                var playerMortgageAddBuyInGameEventDTO = (PlayerMortgageAddBuyInAttemptGameEventDto)gameEvent;
                yield return AnimationController.Instance.PlayerMortgageBuyInAttempt (playerMortgageAddBuyInGameEventDTO.playerId);
            }
            break;
            case GameEventTypeEnum.PLAYER_MORTGAGE_ADD_BUY_IN_SUCCESS: {
                var playerMortgageAddBuyInSuccessGameEventDTO = (PlayerMortgageAddBuyInSuccessGameEventDto)gameEvent;
                yield return AnimationController.Instance.PlayerMortgageBuyInSuccess (playerMortgageAddBuyInSuccessGameEventDTO.playerId);
            }
            break;
            case GameEventTypeEnum.PLAYER_MORTGAGE_ADD_BUY_IN_FAIL: {
                var playerMortgageAddBuyInFailGameEventDTO = (PlayerMortgageAddBuyInFailGameEventDto)gameEvent;
                yield return AnimationController.Instance.PlayerMortgageBuyInFail (playerMortgageAddBuyInFailGameEventDTO.playerId);
            }
            break;
            case GameEventTypeEnum.PLAYER_TRANSATION_ATTEMPT: {
                var playerTransactionAttemptGameEventDTO = (PlayerTransactionAttemptGameEventDto)gameEvent;
                matchClient.SetIsTransactionOngoing (true);
            }
            break;
            case GameEventTypeEnum.PLAYER_TRANSATION_SUCCESS: {
                var playerTransactionSuccessGameEventDTO = (PlayerTransactionSuccessGameEventDto)gameEvent;
                matchClient.SetIsTransactionOngoing (false);
            }
            break;
            case GameEventTypeEnum.PLAYER_TRANSATION_FAIL: {
                var playerTransactionFailGameEventDTO = (PlayerTransactionFailGameEventDto)gameEvent;
                matchClient.SetIsTransactionOngoing (false);
            }
            break;
            case GameEventTypeEnum.MATCH_PRIZE_POT_UPDATE: {
                var prizePotUpdateGameEventDTO = (MatchPrizePotUpdateGameEventDto)gameEvent;
                matchClient.SetTotalPrizePot (prizePotUpdateGameEventDTO.prizePot);
                yield return AnimationController.Instance.UpdatePrizePot (prizePotUpdateGameEventDTO.prizePot);
            }
            break;
            //==============================      
        }
    }



    #region Websocket DTO Sending

    public void BuyOutPropertySend(ulong tile)
    {
        BuyoutPropertyConfirmGameInputDto buyoutPropertyMsgDTO = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId,
            tileId = tile

        };

        byte[] messageBytes = buyoutPropertyMsgDTO.BytesFromParams();

        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        AudioManager.Instance.PlaySFX(14);
    }

    public void BuyOutPropertyDeclineSend()
    {
        BuyoutPropertyDeclineGameInputDto buyoutPropertyDeclineMsgDTO = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId,
            
            

        };
        byte[] messageBytes = buyoutPropertyDeclineMsgDTO.BytesFromParams();

        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        AudioManager.Instance.PlaySFX(14);
    }

    public void BuyOutPropertyHolidaySend(ulong tile)
    {
        if (_holidayTileBuyout)
        {
            BuyoutOwnerHolidayConfirmGameInputDto buyoutPropertyHolidayMsgDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                tileId = tile

            };

            byte[] messageBytes = buyoutPropertyHolidayMsgDTO.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
            AudioManager.Instance.PlaySFX(14);
        }
    }

    public void BuyOutPropertyHolidayDeclineSend()
    {
        if (_holidayTileBuyout)
        {
            BuyoutOwnerHolidayDeclineGameInputDto buyoutPropertyHolidayDeclineMsgDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,



            };
            byte[] messageBytes = buyoutPropertyHolidayDeclineMsgDTO.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
            AudioManager.Instance.PlaySFX(14);
        }
    }

    public void BuyOutPropertyHolidayVisitSend(ulong tile)
    {
        if (_holidayVisitBuyout)
        {
            BuyoutVisitHolidayConfirmGameInputDto buyoutPropertyVisitHolidayMsgDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                tileId = tile

            };

            byte[] messageBytes = buyoutPropertyVisitHolidayMsgDTO.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
            AudioManager.Instance.PlaySFX(14);
        }
    }

    public void BuyOutPropertyHolidayVisitDeclineSend()
    {
        if (_holidayVisitBuyout)
        {
            BuyoutVisitHolidayDeclineGameInputDto buyoutPropertyHolidayVisitDeclineMsgDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,



            };
            byte[] messageBytes = buyoutPropertyHolidayVisitDeclineMsgDTO.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
            AudioManager.Instance.PlaySFX(14);
        }
    }


    public void CardTargetConfirmGameInputSend()
    {
        if (_playerCardTargetTimeOut)
        {
            WebsocketHandler.Instance.matchClient.GetCardEffectWithId(chanceCardID, out var cardEffect);
            CardTargetConfirmGameInputDto cardTargetConfirmGameInputDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                cardId = cardEffect.CardEffectId,
                targetId = chanceCardTargetID,
                targetType = cardEffect.TargetEntityType,
            };
            byte[] messageBytes = cardTargetConfirmGameInputDTO.BytesFromParams();
            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        }
        AudioManager.Instance.PlaySFX(14);
    }

    public void JailBailOutConfirmGameInputSend()
    {
        if (_playerJailBailOutTimeOut)
        {
            JailBailOutConfirmGameInputDto jailBailOutConfirmDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,


            };
            byte[] messageBytes = jailBailOutConfirmDTO.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        }
        AudioManager.Instance.PlaySFX(14);
    }

    public void MortageSellPropertyGameInputSend()
    {
        if (_playerMortgageTimeOut)
        {
            MortgageSellPropertyGameInputDto mortgageSellPropertyDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                tileId = tileIDPrompt,

            };
            byte[] messageBytes = mortgageSellPropertyDTO.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        }
        AudioManager.Instance.PlaySFX(14);
    }

    public void UpgradePropertyDeclineGameInputSend(ulong tile)
    {
        UpgradePropertyConfirmGameInputDto upgradePropertyDTO = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId,
            tileId = tile,

        };
        byte[] messageBytes = upgradePropertyDTO.BytesFromParams();

        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        AudioManager.Instance.PlaySFX(14);
    }

    public void BuyOutPropertyConfirmGameInputSend()
    {
        if (_playerBuyoutPropertyTimeOut)
        {
            BuyoutPropertyConfirmGameInputDto buyoutPropertyDeclineDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                tileId = tileIDPrompt,

            };
            byte[] messageBytes = buyoutPropertyDeclineDTO.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
            AudioManager.Instance.PlaySFX(14);
        }
    }

    public void BuyOutPropertyHolidayConfirmGameInputSend()
    {
        if (_holidayTileBuyout)
        {
            BuyoutOwnerHolidayConfirmGameInputDto buyoutPropertyHolidayDeclineDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                tileId = tileIDPrompt,

            };
            byte[] messageBytes = buyoutPropertyHolidayDeclineDTO.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
            AudioManager.Instance.PlaySFX(14);
        }
    }

    public void BuyOutPropertyVisitConfirmGameInputSend()
    {
        if (_holidayVisitBuyout)
        {
            BuyoutVisitHolidayConfirmGameInputDto buyoutPropertyVisitDeclineDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                tileId = tileIDPrompt,

            };
            byte[] messageBytes = buyoutPropertyVisitDeclineDTO.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
            AudioManager.Instance.PlaySFX(14);
        }
    }

    public void BuyShopItemsConfirmSend(int itemNumber)
    {
        if (_playerBuyShopItemsTimeOut)
        {
            BuyShopItemsConfirmGameInputDto buyShopItemsConfirmDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                boughtItemId = (ulong)itemNumber


            };
            byte[] messageBytes = buyShopItemsConfirmDTO.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        }
        AudioManager.Instance.PlaySFX(14);
    }

    public void CardTargetDeclineSend()
    {
        CardTargetDeclineGameInputDto cardTargetDeclineGameInputDTO = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId,

            

        };
        byte[] messageBytes = cardTargetDeclineGameInputDTO.BytesFromParams();

        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        AudioManager.Instance.PlaySFX(14);
    }

    public void JailBailOutDeclineSend()
    {
        JailBailOutDeclineGameInputDto jailBailOutDeclineDTO = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId,

        };
        byte[] messageBytes = jailBailOutDeclineDTO.BytesFromParams();

        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        AudioManager.Instance.PlaySFX(14);
    }

    public void SurrenderMatchGameInputSend()
    {
        SurrenderMatchGameInputDto surrenderMatchDTO = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId,

        };
        byte[] messageBytes = surrenderMatchDTO.BytesFromParams();

        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        AudioManager.Instance.PlaySFX(14);
        Debug.Log("Surrender");
    }

/*    public void UserItemGameInputSend(UInt64 target, UInt64 item, BoardEntityTypeEnum targetType)
    {
        UseItemGameInputDto useItemGameInputDTO = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId,
            targetId = target,
            itemId = item,
            targetType = targetType



        };
        byte[] messageBytes = useItemGameInputDTO.BytesFromParams();

        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
    }
*/
    public void BuyPropertyConfirmSend()
    {
        if (_playerBuyPropertyTimeOut)
        {
            BuyPropertyConfirmGameInputDto buyPropertyConfirmDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                tileId = tileIDPrompt,
                upgradeLevel = upgradeLevel

            };
            byte[] messageBytes = buyPropertyConfirmDTO.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        }
        AudioManager.Instance.PlaySFX(14);
    }

    public void BuyPropertyHolidayConfirmSend()
    {
        if (_holidayTile)
        {
            BuyOwnerHolidayConfirmGameInputDto buyPropertyHolidayConfirmDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                tileId = tileIDPrompt,

            };
            byte[] messageBytes = buyPropertyHolidayConfirmDTO.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        }
        AudioManager.Instance.PlaySFX(14);
    }

    public void BuyPropertyHolidayVisitConfirmSend()
    {
        if (_holidayVisit)
        {
            BuyVisitHolidayConfirmGameInputDto buyPropertyHolidayVisitConfirmDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                tileId = tileIDPrompt,

            };
            byte[] messageBytes = buyPropertyHolidayVisitConfirmDTO.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        }
        AudioManager.Instance.PlaySFX(14);
    }

    public void ChangeUpgradeLevel(int level)
    {
        AnimationController.Instance.ChangePropertyBuyUIPurchaseText(tileIDPrompt, level);
        switch (level)
        {
            case 1:
                upgradeLevel = PropertyUpgradeLevelEnum.HOUSE;
                break;
            case 2:
                upgradeLevel = PropertyUpgradeLevelEnum.MANSION;
                break;
            case 3:
                upgradeLevel = PropertyUpgradeLevelEnum.HOTEL;
                break;
            default:
                upgradeLevel = PropertyUpgradeLevelEnum.HOUSE;
                break;
        }
       
    }

    public void UseEscapeDesertedIslandCard()
    {
        JailEscapeConfirmGameInputDto jailEscapeGameInputDTO = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId,

        };
          byte[] messageBytes = jailEscapeGameInputDTO.BytesFromParams();
        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        AudioManager.Instance.PlaySFX(14);
    }
    public void BuyPropertyDeclineSend()
    {
        BuyPropertyDeclineGameInputDto buyPropertyConfirmDTO = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId,

        };
        byte[] messageBytes = buyPropertyConfirmDTO.BytesFromParams();
        tileIDPrompt = 0;
        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        AudioManager.Instance.PlaySFX(14);
    }

    public void BuyPropertyHolidayDeclineSend()
    {
        BuyOwnerHolidayDeclineGameInputDto buyPropertyHolidayConfirmDTO = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId,

        };
        byte[] messageBytes = buyPropertyHolidayConfirmDTO.BytesFromParams();
        tileIDPrompt = 0;
        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        AudioManager.Instance.PlaySFX(14);
    }

    public void BuyPropertyVisitDeclineSend()
    {
        BuyVisitHolidayDeclineGameInputDto buyPropertyVisitConfirmDTO = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId,

        };
        byte[] messageBytes = buyPropertyVisitConfirmDTO.BytesFromParams();
        tileIDPrompt = 0;
        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        AudioManager.Instance.PlaySFX(14);
    }

    public void BuyShopItemsDeclineSend()
    {
        BuyShopItemsDeclineGameInputDto buyShopITemsDeclineDTO = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId,

        };
        byte[] messageBytes = buyShopITemsDeclineDTO.BytesFromParams();

        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        AudioManager.Instance.PlaySFX(14);
    }

    public void MortgageDeclareBankruptSend()
    {
        MortgageDeclareBankruptGameInputDto mortgageDeclareGameInputDTO = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId,

        };
        byte[] messageBytes = mortgageDeclareGameInputDTO.BytesFromParams();

        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        AudioManager.Instance.PlaySFX(14);
    }




    public void UpgradedPropertyConfirmSend(ulong tile, PropertyUpgradeLevelEnum level)
    {
        if (_playerUpgradePropertyTimeOut)
        {
            UpgradePropertyConfirmGameInputDto upgradePropertyConfirmGameInputDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                tileId = tile,
                upgradeLevel = level

            };
            byte[] messageBytes = upgradePropertyConfirmGameInputDTO.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        }
        AudioManager.Instance.PlaySFX(14);
    }

    public void RollDice(Int32 rollStr)
    {
        if (_preRollTimeOut)
        {
            var wsh = WebsocketHandler.Instance;
            if (!wsh.matchClient.GetPlayerWithId(wsh.selfPlayerId, out var selfPlayer))
            {
                return;
            }

            if (selfPlayer.CurrentPlayerPrompt == PlayerPromptEnum.TURN_ORDER_ROLLING)
            {
                PlayerTurnOrderDiceRollGameInputDto turnOrderDiceRollGameInputDTO = new()
                {
                    matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                    playerId = WebsocketHandler.Instance.selfPlayerId,
                    rollStrength = rollStr,
                };

                byte[] messageBytes = turnOrderDiceRollGameInputDTO.BytesFromParams();

                WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
                AudioManager.Instance.PlaySFX(14);
            }
            else if (selfPlayer.CurrentPlayerPrompt == PlayerPromptEnum.PRE_ROLL)
            {
                DiceRollGameInputDto diceRollMsg = new()
                {
                    matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                    playerId = WebsocketHandler.Instance.selfPlayerId,
                    rollStrength = rollStr,
                };

                Debug.Log("WOWOW" + rollStr);

                byte[] messageBytes = diceRollMsg.BytesFromParams();


                WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
            }   
            AudioManager.Instance.PlaySFX(14);

        }
    }

    public void GoTileRemoteUpgradeConfirmSend()
    {
        if (_goTileRemoteUpgradeTimeOut)
        {
            GoTileRemoteUpgradeTargetConfirmGameInputDto goTileUpgradeConfirmDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                propertyTileId = WebsocketHandler.Instance.matchClient.GetGoStartTile().TileId

            };
            byte[] messageBytes = goTileUpgradeConfirmDTO.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        }
        AudioManager.Instance.PlaySFX(14);
    }

    public void GoTileRemoteUpgradeDeclineSend()
    {
        GoTileRemoteUpgradeTargetDeclineGameInputDto goTileUpgradeDeclineDTO = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId

        };
        byte[] messageBytes = goTileUpgradeDeclineDTO.BytesFromParams();

        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        AudioManager.Instance.PlaySFX(14);
    }

    public void UpgradePropertyConfirm()
    {
        if (_playerUpgradePropertyTimeOut)
        {
            UpgradePropertyConfirmGameInputDto upgradePropertyConfirm = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                upgradeLevel = PropertyUpgradeLevelEnum.LANDMARK,
                tileId = tileIDPrompt


            };
            byte[] messageBytes = upgradePropertyConfirm.BytesFromParams();

            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        }
        AudioManager.Instance.PlaySFX(14);
    }

    public void UpgradePropertyDecline()
    {
        UpgradePropertyDeclineGameInputDto upgradePropertyDecline = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId

        };
        byte[] messageBytes = upgradePropertyDecline.BytesFromParams();

        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        AudioManager.Instance.PlaySFX(14);
    }
    public void DeclareBankruptcy()
    {
        MortgageDeclareBankruptGameInputDto declareBankruptcy = new()
        {
            matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
            playerId = WebsocketHandler.Instance.selfPlayerId

        };
        byte[] messageBytes = declareBankruptcy.BytesFromParams();

        WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
        AudioManager.Instance.PlaySFX(14);
    }

    public void DeclineTileForOlympics()
    {
        if (_olympicEventTimeout)
        {
            OlympicsTileTargetConfirmGameInputDto olympicGameInputDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                propertyTileId = tileIDPrompt
            };
            byte[] messageBytes = olympicGameInputDTO.BytesFromParams();


            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
            AudioManager.Instance.PlaySFX(14);
        }
    }

    public void DeclineWorldTravel()
    {
        if (_worldTileEventTimeout)
        {
            WorldTravelTileTargetDeclineGameInputDto worldTravelDeclineGameInputDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,
                
            };
            byte[] messageBytes = worldTravelDeclineGameInputDTO.BytesFromParams();


            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
            AudioManager.Instance.PlaySFX(14);
        }
    }

    public void CitySwapDecline()
    {
        if (_citySwap)
        {
            CitySwapDeclineGameInputDto citySwapDeclineGameInputDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,

            };
            byte[] messageBytes = citySwapDeclineGameInputDTO.BytesFromParams();


            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
            AudioManager.Instance.PlaySFX(14);
        }
    }

    public void BuyInAttempt()
    {

            MortgageAddBuyInGameInputDto mortgageAddBuyInGameInputDTO = new()
            {
                matchId = WebsocketHandler.Instance.matchClient.GameMatchId,
                playerId = WebsocketHandler.Instance.selfPlayerId,

            };
            byte[] messageBytes = mortgageAddBuyInGameInputDTO.BytesFromParams();


            WebsocketHandler.Instance.SendWebSocketMessageBytes(messageBytes);
            AudioManager.Instance.PlaySFX(14);
        
    }



    #endregion

    private IEnumerator RollCoroutine(float speed)
    {
        //AudioManager.Instance.sfxSource.Play();
        while (isRolling)
        {
            // Decrease padding
            while (thermometer.padding.w > 0)
            {
                thermometer.padding = new Vector4(0, 0, 0, thermometer.padding.w - speed * Time.deltaTime);
                thermometer.padding = new Vector4(0, 0, 0, Mathf.Max(0, thermometer.padding.w));

                if (thermometer.padding.w == 0)
                {
                    //AudioManager.Instance.StopSFX();
                    AudioManager.Instance.PlaySFX(17);
                }
                percentage = (1 - (thermometer.padding.w / 660f)) * 100;

                yield return null;

                if (!isRolling)
                    yield break;
            }

            // Increase padding
            while (thermometer.padding.w < 660)
            {
                thermometer.padding = new Vector4(0, 0, 0, thermometer.padding.w + speed * Time.deltaTime);
                thermometer.padding = new Vector4(0, 0, 0, Mathf.Min(660, thermometer.padding.w));

                if (thermometer.padding.w == 660)
                {
                  //  AudioManager.Instance.StopSFX();
                    AudioManager.Instance.PlaySFX(16); 
                }
                percentage = (1 - (thermometer.padding.w / 660f)) * 100;

                yield return null;

                if (!isRolling)
                   
                yield break;
            }
        }
    }





    #region Private Methods

    public bool CheckIfDouble(List<ushort> dice)
    {
        ushort firstElement = dice[0];
        foreach (ushort rollResult in dice)
        {
            if(rollResult != firstElement)
            {
                rolledDoubleUI.SetActive(false);
                return false;
            }

        }

        if (dice.Count > 1) 
        { 
            rolledDoubleUI.SetActive(true);
        }
       return true;
    }

    private void DisablePlayerInteraction(UInt64 playerid) 
    {
        var wsh = WebsocketHandler.Instance;
        if (!wsh.matchClient.GetPlayerWithId(wsh.selfPlayerId, out var selfPlayer))
        {
            return;
        }

        if (selfPlayer.PlayerId != playerid)
        {
            StopCoroutine(RollCoroutine(fillDuration));
            //AnimationController.Instance.promptTimerGO.SetActive(false);
            isRolling = false;
            //overlay.SetActive(true);
            //targetPlayer.SetActive(false);
            thermometerGO.SetActive(false);
            rollButton.interactable = false;
        }
        else
        {
            //AnimationController.Instance.promptTimerGO.SetActive(true);
            rollButton.interactable = true;
            thermometerGO.SetActive(true);
            //overlay.SetActive(false);
            //optionScreen.SetActive(false);
            //targetPlayer.SetActive(true);
        }

    }

    private void DisableRollUI()
    {
        StopCoroutine(RollCoroutine(fillDuration));
        //AnimationController.Instance.promptTimerGO.SetActive(false);
        isRolling = false;
        rollButton.interactable = false;
        //targetPlayer.SetActive(false);
        thermometerGO.SetActive(false);
    }

    #endregion


    #region Public Methods

    public void RollButton()
    {
        isRolling = !isRolling;
        if (isRolling)
        {
            die.Play("die_spin");
            AudioManager.Instance.sfxSource.volume = PlayerPrefs.GetFloat("Sound Effects");
            StartCoroutine(RollCoroutine(fillDuration));
        }
        else
        {
            die.Play("die_idle");
            StopCoroutine(RollCoroutine(fillDuration));
            AudioManager.Instance.StopSFX();
            //Uncomment if you want it to reset back to 0
            //thermometer.padding = new Vector4(0, 0, 0, 0);
            RollDice(Convert.ToInt32(percentage));
        }
    }

    private void AssignPlayers()
    {
        var players = WebsocketHandler.Instance.matchClient.JoinedPlayers;
        int currentIndex = 0;
        foreach (var player in players)
        {
            eventPlayerLists[currentIndex].playerId = player.PlayerId;
            targetPlayerButtons[currentIndex].playerId = player.PlayerId;
            if (player.PlayerId != WebsocketHandler.Instance.selfPlayerId) targetPlayerButtonsChance[currentIndex].playerId = player.PlayerId;
            targetPlayerButtons[currentIndex].playerImage = eventPlayerLists[currentIndex].playerImage;
            if (player.PlayerId != WebsocketHandler.Instance.selfPlayerId) targetPlayerButtonsChance[currentIndex].playerImage = eventPlayerLists[currentIndex].playerImage;

            switch (player.PlayerColor)
            {
                case PlayerColorEnum.RED:
                    eventPlayerLists[0].playerColor = PlayerColorEnum.RED;
                    break;
                case PlayerColorEnum.BLUE:
                    eventPlayerLists[1].playerColor = PlayerColorEnum.BLUE;
                    break;
                case PlayerColorEnum.GREEN:
                    eventPlayerLists[2].playerColor = PlayerColorEnum.GREEN;
                    break;
                case PlayerColorEnum.YELLOW:
                    eventPlayerLists[3].playerColor = PlayerColorEnum.YELLOW;
                    break;
                default:
                    break;
            }

            eventPlayerLists[currentIndex].chips = Convert.ToInt32(player.OwnedChipsAmount);
            currentIndex++;
        }

        int assignPlayerUIIndex = 1;
        foreach (var player in eventPlayerLists)
        {
            if(WebsocketHandler.Instance.selfPlayerId == player.playerId && player.playerColor != PlayerColorEnum.NONE)
            {
                playerUIScript[0].AssignUI(player.playerColor);
                player.playerName = playerUIScript[0].playerNameText.text;
                playerUIScript[0].playerColor = player.playerColor;
                playerUIScript[0].playerId = player.playerId;

                player.playerUIText = playerUIScript[0].playerMoney;
                // player.buildPoints = playerUIScript[0].constructionPoints;
                player.playerUIAnim = playerUIScript[0].playerUIAnim;
                Debug.Log(player.playerColor + "" + assignPlayerUIIndex);

            }
            else if(player.playerColor != PlayerColorEnum.NONE)
            {
                playerUIScript[assignPlayerUIIndex].AssignUI(player.playerColor);
                player.playerName = playerUIScript[assignPlayerUIIndex].playerNameText.text;
                playerUIScript[assignPlayerUIIndex].playerColor = player.playerColor;
                playerUIScript[assignPlayerUIIndex].playerId = player.playerId;

                player.playerUIText = playerUIScript[assignPlayerUIIndex].playerMoney;
                // player.buildPoints = playerUIScript[assignPlayerUIIndex].constructionPoints;
                player.playerUIAnim = playerUIScript[assignPlayerUIIndex].playerUIAnim;
                assignPlayerUIIndex++;
                Debug.Log(player.playerColor + "" + assignPlayerUIIndex);
            }
            player.playerUIText.text = NumberWithComma.FormatNumber(player.chips);
        }

        _playerAssigned = true;
    }



    public void ChangeTarget(int playerNumber)
    {

        chanceCardTargetID = eventPlayerLists[playerNumber].playerId;
    }

    public void ReturnToMainMenu()
    {
        SceneManager.LoadScene(0);
    }

    #endregion
}
