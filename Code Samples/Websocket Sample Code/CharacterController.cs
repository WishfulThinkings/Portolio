using ModooMarbleGameClientServerData.Src.Types;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CharacterController : MonoBehaviour
{
    public UInt64 playerId;
    public TextMeshProUGUI playerUIText;
    public TextMeshProUGUI buildPoints;
    public int chips;

    [Header("Name Component")]
    public string playerName;
    public GameObject turnUI;
    public TextMeshProUGUI turnUIText;
    
    [HideInInspector]public Animator animator;
    public Sprite playerImage;

    [Header("Player Actions")]
    public GameObject playerActionTextParent;
    public TextMeshProUGUI playerActionText;

    public PlayerColorEnum playerColor;

    public Animator playerUIAnim;

    public int colorMonopolyScore;
    public int holidayScore;

    [Header("Player Effects")]
    public GameObject playerHappy;
    public GameObject playerSad;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
}
