using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;
using System;
using UnityEngine.SceneManagement;

public class NetworkGameManager : NetworkBehaviour
{
    //Client RPC is called by server and executed on all clients
    //Server RPC is called by clients and executed on server

    public static NetworkGameManager instance;

    [SerializeField] private NetworkBoard board;

    public Piece.PieceType turn = Piece.PieceType.black;

    [HideInInspector] public Piece.PieceType playerTurn;

    [HideInInspector] public bool moved = false;
    [HideInInspector] public bool gameStarted = false;
    [HideInInspector] public bool gameOver = false;

    private int whiteScore = 0;
    private int blackScore = 0;

    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject moveEndPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] TextMeshProUGUI winnerText;
    [SerializeField] TextMeshProUGUI turnText;
    [SerializeField] TextMeshProUGUI blackScoreText;
    [SerializeField] TextMeshProUGUI whiteScoreText;

    private void Awake()
    {
        if (instance == null)
            instance = this;      
    }

    // Start is called before the first frame update
    void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

        //UpdateTurnText();
    }

    void UpdateTurnText()
    {
        if (turn == Piece.PieceType.black)
        {
            print("Black's turn");
            turnText.text = "Black's turn";
            //turnText.color = Color.black;
        }
        else
        {
            print("White's turn");
            turnText.text = "White's turn";
            //turnText.color = Color.white;
        }
    }

    public void PerformedMove()
    {
        moved = true;
        moveEndPanel.SetActive(true);
        //CheckWin();
    }

    [ServerRpc(RequireOwnership = false)]
    public void PerformedMoveServerRpc()
    {
        EndTurnClientRpc();
    }

    [ClientRpc]
    public void EndTurnClientRpc()
    {
        //moved = true;

        CheckWin();

        //moved = false;

        if (turn == Piece.PieceType.black)
            turn = Piece.PieceType.white;
        else
            turn = Piece.PieceType.black;

        UpdateTurnText();
    }

    public void UpdateScore(Piece.PieceType piece, int s)
    {
        if (piece == Piece.PieceType.black)
        {
            blackScore += s;
            blackScoreText.text = "Black Score: " + blackScore.ToString();
        }
        else
        {
            whiteScore += s;
            whiteScoreText.text = "White Score: " + whiteScore.ToString();
        }
    }

    public void PerformedUndo()
    {
        moveEndPanel.SetActive(false);
        moved = false;
    }

    public void UndoMove()
    {
        board.Undo();
    }

    void CheckWin()
    {
        if (blackScore >= 12)
        {
            Win(Piece.PieceType.black);
        }
        else if (whiteScore >= 12)
        {
            Win(Piece.PieceType.white);
        }
    }

    public void Win(Piece.PieceType pieceType)
    {
        gameOver = true;
        AudioManager.instance.PlayWinSound();
        turnText.gameObject.SetActive(false);
        gamePanel.SetActive(false);
        if (pieceType == Piece.PieceType.black)
        {
            //winnerText.color = Color.black;
            winnerText.text = "Black WON!";
        }
        else
        {
            //winnerText.color = Color.white;
            winnerText.text = "White WON!";
        }
        gameOverPanel.SetActive(true);

        //updating in firebase
        if (pieceType == playerTurn)
        {
            FirebaseDBManager.instance.IncreasePlayerWins();
            XPManager.instance.CalculateXP(true);
            AchievementManager.WonGame();
        }
        else
        {
            FirebaseDBManager.instance.IncreasePlayerLosses();
            XPManager.instance.CalculateXP(false);
            AchievementManager.LostGame();
        }
    }

    private void OnClientConnectedCallback(ulong obj)
    {
        print("Client connected");

        //assigning turn
        if (IsServer)
        {
            playerTurn = Piece.PieceType.black;
            print("You are black");
        }
        else
        {
            playerTurn = Piece.PieceType.white;
            print("You are white");
        }

        //if both players joined
        if (IsServer && NetworkManager.Singleton.ConnectedClients.Count == 2)
        {
            print("Start game");
            StartGameClientRpc();
        }
    }

    private void OnClientDisconnectCallback(ulong obj)
    {
        //end match
        Win(playerTurn);
    }

    [ClientRpc]
    void StartGameClientRpc()
    {
        gamePanel.SetActive(true);
        board.gameObject.SetActive(true);
        board.InitializeBoard();
        UpdateTurnText();
        gameStarted = true;
    }

    public void StartHost()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback += ConnectionApprovalCallback;
        NetworkManager.Singleton.StartHost();;
    }

    private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
    {
        //only allow max 2 players
        if (NetworkManager.Singleton.ConnectedClients.Count < 2)
        {
            connectionApprovalResponse.Approved = true;
            connectionApprovalResponse.CreatePlayerObject = true;
        }
        else
        {
            connectionApprovalResponse.Approved = false;
        }
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    public void GoToMenu()
    {
        NetworkManager.Shutdown();
        SceneManager.LoadScene(0);
    }
}