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
    
    public static GameManager instance;

    public Piece.PieceType turn = Piece.PieceType.black;
    [HideInInspector] public bool moved = false;

    [SerializeField] private GameObject moveEndPanel;
    [SerializeField] private Text turnText;
    [SerializeField] private Text blackScoreText;
    [SerializeField] private Text whiteScoreText;
    [SerializeField] private Board board;

    private int whiteScore = 0;
    private  int blackScore = 0;

    private void Awake()
    {
        //singleton
        if (instance == null)
            instance = this;
    }

    private void Start()
    {
        moveEndPanel.SetActive(false);
        UpdateTurnText();  
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
        moveEndPanel.SetActive(true);
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
            turn = Piece.PieceType.white;
        else
            turn = Piece.PieceType.black;
        moveEndPanel.SetActive(false);
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

    void CheckWin()
    {
        if (blackScore >= 12)
        {
            print("Black won!");
            Invoke("RestartScene", 2);
        }
        else if (whiteScore >= 12)
        {
            print("White won!");
            Invoke("RestartScene", 2);
        }
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}