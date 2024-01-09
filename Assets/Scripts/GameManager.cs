using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// Follow the coding standard defined here
    /// https://stax3.slab.com/posts/engineering-practices-unity-4b8xeh31
    /// </summary>
    
    public enum GameMode
    {
        twoPlayer,
        playerVsAI,
        AIvsAI
    }

    public static GameManager instance;

    [HideInInspector] public GameMode gameMode;
    public Piece.PieceType turn = Piece.PieceType.black;
    [HideInInspector] public bool moved = false;

    [SerializeField] private GameObject moveEndPanel;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private Text winnerText;
    [SerializeField] private Text turnText;
    [SerializeField] private Text blackScoreText;
    [SerializeField] private Text whiteScoreText;
    [SerializeField] private Board board;

    private int whiteScore = 0;
    private int blackScore = 0;
    private bool gameOver = false;

    [HideInInspector] public Piece.PieceType playerTurn;

    private void Awake()
    {
        //singleton
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        int mode = PlayerPrefs.GetInt("gameMode", 2);
        if (mode == 0)
            gameMode = GameMode.playerVsAI;
        else if (mode == 1)
            gameMode = GameMode.twoPlayer;
        else
            gameMode = GameMode.AIvsAI;

        playerTurn = turn;
        moveEndPanel.SetActive(false);
        UpdateTurnText();

        if (gameMode == GameMode.AIvsAI)
            board.AITurn();
    }

    void UpdateTurnText()
    {
        if (turn == Piece.PieceType.black)
        {
            turnText.text = "Black's turn";
            turnText.color = Color.black;
        }
        else
        {
            turnText.text = "White's turn";
            turnText.color = Color.white;
        }
    }

    public void PerformedMove()
    {
        moved = true;
        if (gameMode == GameMode.AIvsAI)
        {
            moveEndPanel.SetActive(false);
        }
        else if (!(gameMode == GameMode.playerVsAI && turn != playerTurn))
        {
            moveEndPanel.SetActive(true);
        }
        CheckWin();
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

    public void EndTurn()
    {
        moved = false;
        if (turn == Piece.PieceType.black)
        {
            turn = Piece.PieceType.white;
        }
        else
            turn = Piece.PieceType.black;

        moveEndPanel.SetActive(false);
        UpdateTurnText();

        if (!gameOver)
        {
            if (gameMode == GameMode.playerVsAI && turn != playerTurn)
                board.AITurn();
            else if (gameMode == GameMode.AIvsAI)
                board.AITurn();
        }
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

    void Win(Piece.PieceType pieceType)
    {
        gameOver = true;
        turnText.gameObject.SetActive(false);
        if (pieceType == Piece.PieceType.black)
        {
            winnerText.color = Color.black;
            winnerText.text = "Black WON!";
        }
        else
        {
            winnerText.color = Color.white;
            winnerText.text = "White WON!";
        }
        gameOverPanel.SetActive(true);
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene(0);
    }
}