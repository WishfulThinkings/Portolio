using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NativeWebSocket;
using ModooMarbleGameClientServerData.Src.Dtos.Factory;
using ModooMarbleGameClientServerData.Src.Dtos;
using ModooMarbleGameClientServerData.Src.Types;
using ModooMarbleGameClientServerData.Src.Entities.GameModels;
using ModooMarbleGameClientServerData.Src.Data.GameRuleData;
using ModooMarbleGameClientServerData.Src.Data.GameStateData;
using UnityEngine.SceneManagement;
using TMPro;
public class WebsocketHandler : MonoBehaviour
{
    public delegate void ProcessGameEventAction(GameEventDto gameEventDto);
    public static ProcessGameEventAction ProcessGameEvent;

    public delegate void FillPlayerListAction(List<CharacterController> playerList);
    public static FillPlayerListAction FillPlayerList;

    public delegate void ProcessMainMenuEventAction(MainMenuEventDto mainMenuEventDTO);
    public static ProcessMainMenuEventAction ProcessMainMenuEvent;

    public delegate void ProcessFeedbackMessageAction(FeedbackMessageDto feedback);
    public static ProcessFeedbackMessageAction ProcessFeedbackMessage;

    public static WebsocketHandler Instance { get; private set; }

    WebSocket websocket;
    public string url;
    public string _guidString = "";

    [Header("Int Type Enums")]
    public NetworkActivityTypeEnum activityType;
    public GameEventTypeEnum gameEventType;
    public MainMenuInputTypeEnum mainMenuInputType;


    public List<CharacterController> playerList; 
    
    public ulong selfPlayerId;
    public MatchGameModelClient matchClient;

    public TextMeshProUGUI packetSize;
   
    public Animator transitionAnim;
    public GameObject transitionCanvas;

    public GameObject serverCloseCanvas;

    [SerializeField]
    private bool connected;

    [SerializeField] private string _uuid;
    [SerializeField] private string _token;

    private readonly Queue<IEnumerator> coroutineQueue = new Queue<IEnumerator>();
    private IEnumerator runningCoroutine;

    private void Awake()
    {
        _uuid = System.Guid.NewGuid().ToString();
        _token = System.Guid.NewGuid().ToString();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

            DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartConnection();
        StartCoroutine(CoroutineCoordinator());
    }

    public async void StartConnection()
    {
        // Append UUID and token as query parameters to the URL
        string urlWithParams = $"{url}?uuid={_uuid}&token={_token}";

        Debug.Log(urlWithParams);

        // Initialize the WebSocket with the updated URL
        websocket = new WebSocket(urlWithParams);

        websocket.OnOpen += () => {
            serverCloseCanvas.SetActive(false);
            Debug.Log("Connection open!");

            ConnectionHandshakeDto handShakeDto = new()
            {
                handshakeType = ConnectionHandshakeTypeEnum.CLIENT_TO_SERVER_SEND,
                userGuid = "dasd",
            };

            byte[] handShakeBytes = handShakeDto.BytesFromParams();
            SendWebSocketMessageBytes(handShakeBytes);
            connected = true;
            StartHeartbeat();
        };

        websocket.OnError += (e) => {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) => {
            serverCloseCanvas.SetActive(true);
            Debug.Log("Connection closed! " + e);
            connected = false;
            EndHeartbeat();
        };

        websocket.OnMessage += (bytes) => {
            if (bytes.Length > 0)
            {
                try
                {
                    if (runningCoroutine == null)
                    {
                        runningCoroutine = ProcessMessage(bytes);
                        StartCoroutine(runningCoroutine);
                    }
                    else
                    {
                        coroutineQueue.Enqueue(ProcessMessage(bytes));
                    }
                }
                catch (Exception e)
                {
                    Debug.Log(e.ToString());
                }
            }
        };

        // Connect the WebSocket
        await websocket.Connect();
    }


    IEnumerator CoroutineCoordinator()
    {
        while (true)
        {
            if (coroutineQueue.Count > 0)
            {
                runningCoroutine = coroutineQueue.Dequeue();
                yield return runningCoroutine;
                runningCoroutine = null;
            }
            yield return null;
        }
    }


    private Coroutine _heartbeat;

    private void StartHeartbeat()
    {
        if (_heartbeat != null)
            return;

        _heartbeat = StartCoroutine(Heartbeat());
    }

    private void EndHeartbeat()
    {
        if (_heartbeat == null)
            return;

        StopCoroutine(_heartbeat);
        _heartbeat = null;
    }

    private IEnumerator Heartbeat()
    {
        while (connected)
        {
            yield return new WaitForSeconds(10f);
            ConnectionPingDto pingMsg = new()
            {
                userGuid = _guidString,
                pingMessage = "Client Ping Request SEND...",
            };
            SendWebSocketMessageBytes(pingMsg.BytesFromParams());
        }

        _heartbeat = null;
    }

    private IEnumerator ProcessNetworkActivity (Action lambda)
    {
        yield return null;
        lambda();
    }

    private IEnumerator ProcessMessage(byte[] bytes)
    {
        var networkActivityList = NetworkActivityFactory.ProcessBytesToNetworkActivities(bytes);

        foreach (var networkActivity in networkActivityList)
        {
            switch (networkActivity.NetworkActivityType)
            {
                case NetworkActivityTypeEnum.SOCKET_PING:
                    //ProcessPingMessage((ConnectionPingDto)networkActivity);
                    break;
                case NetworkActivityTypeEnum.SOCKET_HANDSHAKE:
                    ProcessHandshakeMessage((ConnectionHandshakeDto)networkActivity);
                    break;
                case NetworkActivityTypeEnum.MARBLE_GAME_EVENT:
                    var ge = (GameEventDto)networkActivity;
                    //Debug.Log("GE TYPE: " + ge.GameEventType);
                    yield return ProcessNetworkActivity(() => {
                        ProcessGameEvent?.Invoke((GameEventDto)networkActivity);
                    });
                    break;
                case NetworkActivityTypeEnum.MARBLE_GAME_INIT:
                    GameMatchInitDto gameMatchDTO = (GameMatchInitDto)networkActivity;
                    InitializeMatchData(gameMatchDTO.matchRuleData, gameMatchDTO.matchStateData);
                    //Debug.Log("Game Match ID " + matchClient.GameMatchId);
                    selfPlayerId = gameMatchDTO.selfPlayerId;
                    yield return LoadGameScene("GameBoardGameplay");
                    //Pasok sa scene bago initialize 
                    //Initialize Match State
                    break;
                case NetworkActivityTypeEnum.MARBLE_GAME_CHECK:
                    // Game State Checker For Events Huli 
                    break;
                case NetworkActivityTypeEnum.MAIN_MENU_EVENT:
                    ProcessMainMenuEvent?.Invoke((MainMenuEventDto)networkActivity);
                 
                    break;
                case NetworkActivityTypeEnum.FEEDBACK_MESSAGE_REJECTED:
                    ProcessFeedbackMessage?.Invoke((FeedbackMessageDto)networkActivity);
                    break;
            }
            //packetSize.text = bytes.Length.ToString();
            activityType = networkActivity.NetworkActivityType;
        }
    }

    void InitializeMatchData(MatchGameRuleData rule, MatchGameStateData state)
    {
        matchClient = new MatchGameModelClient(rule);
        matchClient.UpdateWithStateData(state);

    }

    private IEnumerator LoadGameScene(string sceneName)
    {
        transitionCanvas.SetActive(true);
        transitionAnim.Play("Fade_In");
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene(sceneName);
        yield return new WaitForSeconds(2);
        transitionAnim.Play("Fade_Out");
        yield return new WaitForSeconds(1);
        transitionCanvas.SetActive(false);
    }

    public void CallFadeOut()
    {
        StartCoroutine(LoadGameSceneFadeOut());
    }
    private IEnumerator LoadGameSceneFadeOut()
    {
        transitionAnim.Play("Fade_Out");
        yield return new WaitForSeconds(1);
        transitionCanvas.SetActive(false);

    }


    private IEnumerator DelayRollPrompt(float duration, GameEventDto networkActivity)
    {
        yield return new WaitForSeconds(duration);
        ProcessGameEvent?.Invoke(networkActivity);
    }


    public void LoadMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
        serverCloseCanvas.SetActive(false);
    }

    #region For Testing
    private void ProcessHandshakeMessage(ConnectionHandshakeDto hsMessage)
    {
        Debug.Log(hsMessage.handshakeType + "(from Server) " + hsMessage.userGuid);

        if (hsMessage.handshakeType == ConnectionHandshakeTypeEnum.SERVER_TO_CLIENT_SEND)
        {
            // Cache Web Socket GUID.
            _guidString = hsMessage.userGuid;
            // Respond from Handshake Message.
            ConnectionHandshakeDto hsResponse = new()
            {
                handshakeType = ConnectionHandshakeTypeEnum.SERVER_TO_CLIENT_RECEIVE,
                userGuid = _guidString,
            };
            byte[] handshakeBytes = hsResponse.BytesFromParams();
            SendWebSocketMessageBytes(handshakeBytes);
        }
    }

#endregion

/*    private void ProcessPingMessage(ConnectionPingDto pingMessage)
    {
        Debug.Log("(from Server) " + pingMessage.pingMessage);
    }
*/
    void Update()
    {

/*        if (_uuid == "" || _token == "")
        {
            _uuid = Guid.NewGuid().ToString();
            _token = Guid.NewGuid().ToString();

        }
*/
#if !UNITY_WEBGL || UNITY_EDITOR


        if (connected)
        {
            websocket.DispatchMessageQueue();
        }

#endif
    }

    #region Sending Messages

    public async void SendWebSocketMessage(string msg)
    {
        if (websocket.State == WebSocketState.Open)
        {
            await websocket.SendText(msg);
        }
    }
    public void SendWebSocketMessageBytes(byte[] bytes)
    {
        if (websocket.State == WebSocketState.Open)
        {
            websocket.Send(bytes);
        }
    }

    async void SendWebSocketMessageTest()
    {
        if (websocket.State == WebSocketState.Open)
        {
            // Sending bytes
            byte[] bytes = { 01, 01, 01 };
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            //await websocket.Send(new byte[] {0});

            // Sending plain text
            await websocket.SendText(message);

        }
    }

    #endregion 
    public async void DisconnectConnection()
    {
        await websocket.Close();
    }

    private async void OnApplicationQuit()
    {
        if (connected)
        {
            await websocket.Close();
        }
    }
}