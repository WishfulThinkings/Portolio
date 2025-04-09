using ModooMarbleGameClientServerData.Src.Dtos;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WebsocketFeedbackMessageHandler : MonoBehaviour
{
    public GameObject rejectedGameInputGO;
    public TextMeshProUGUI rejectedGameInputText;

    private void OnEnable()
    {
        WebsocketHandler.ProcessFeedbackMessage += ParseFeedbackMessage;
    }

    private void OnDisable()
    {
        WebsocketHandler.ProcessFeedbackMessage -= ParseFeedbackMessage;
    }


    private void ParseFeedbackMessage(FeedbackMessageDto feedback)
    {
        StopCoroutine(FeedbackMessageEnabler());
        StartCoroutine(FeedbackMessageEnabler());

        switch (feedback.messageKey)
        {
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_UNSUPPORTED_ACTION:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Unsupported Action", "게임 입력 거부됨: 지원되지 않는 동작");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_WRONG_MATCH_ID:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Wrong Match ID", "게임 입력 거부됨: 잘못된 매치 ID");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_ITEM_NOT_EXIST:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Item does not exist", "게임 입력 거부됨: 아이템이 존재하지 않음");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_TILE_NOT_EXIST:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Tile does not exist", "게임 입력 거부됨: 타일이 존재하지 않음");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_CARD_EFFECT_NOT_EXIST:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Card effect does not exist", "게임 입력 거부됨: 카드 효과가 존재하지 않음");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_PLAYER_NOT_JOINED:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Player has not joined yet", "게임 입력 거부됨: 플레이어가 아직 참가하지 않음");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_INVALID_TARGET:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Invalid Target", "게임 입력 거부됨: 잘못된 대상");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_WRONG_PLAYER_TURN:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Wrong player turn", "게임 입력 거부됨: 잘못된 플레이어 턴");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_WRONG_ACTION_PROMPT:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Wrong Action Prompt", "게임 입력 거부됨: 잘못된 행동 프롬프트");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_ACTION_RESTRICTED:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Action Restricted", "게임 입력 거부됨: 제한된 행동");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_WRONG_ACTION_PROMPT_SOURCE:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Wrong Action Prompt Source", "게임 입력 거부됨: 잘못된 행동 프롬프트 소스");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_WRONG_ITEM_OWNERSHIP:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Wrong Item Ownership", "게임 입력 거부됨: 잘못된 아이템 소유");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_INSUFFICIENT_CHIPS:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Insufficient Chips", "게임 입력 거부됨: 칩 부족");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_WRONG_TILE_TYPE:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Wrong Tile Type", "게임 입력 거부됨: 잘못된 타일 유형");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_WRONG_TILE_OWNERSHIP:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Wrong Tile Ownership", "게임 입력 거부됨: 잘못된 타일 소유");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_WRONG_TILE_CONDITION:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Wrong Tile Condition", "게임 입력 거부됨: 잘못된 타일 상태");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_OPTION_UNAVAILABLE:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Option Unavailable", "게임 입력 거부됨: 선택 불가능");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_EMPTY_UUID:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Empty UUID", "게임 입력 거부됨: 빈 UUID");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_GAME_ALREADY_ENDED:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Game Already Ended", "게임 입력 거부됨: 게임이 이미 종료됨");
                break;
            case ModooMarbleGameClientServerData.Src.Types.FeedbackMessageKeyEnum.REJECTED_GAME_INPUT_EXCESS_INPUT:
                LocalizationFormatter.FormatText(rejectedGameInputText, "Game Input Rejected: Excess Input", "게임 입력 거부됨: 과잉 입력");
                break;
            default:
                break;
        }
    }

    private IEnumerator FeedbackMessageEnabler()
    {
        rejectedGameInputGO.SetActive(false);
        rejectedGameInputGO.SetActive(true);
        yield return new WaitForSeconds(1f);
        rejectedGameInputGO.SetActive(false);
    }
}
