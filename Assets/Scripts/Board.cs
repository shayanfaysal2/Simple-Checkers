using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Board : MonoBehaviour
{
    public bool displayGrid;
    public GameObject piecePrefab;
    public GameObject highlightPrefab;

    private const int boardSize = 8;

    private Piece[,] pieces;
    private List<Piece> blackPieces = new List<Piece>();
    private List<Piece> whitePieces = new List<Piece>();

    private List<GameObject> highlights = new List<GameObject>();
    private List<MoveInfo> validMoves = new List<MoveInfo>();
    private Stack<MoveInfo> moveStack = new Stack<MoveInfo>();

    public Piece selectedPiece;
    private Transform movingPiece;
    private Vector3 movingTarget;

    private Coroutine aiCoroutine = null;

    private void Start()
    {
        InitializeBoard();
    }

    private void Update()
    {
        //left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            //if ai vs ai
            if (GameManager.instance.gameMode == GameManager.GameMode.AIvsAI)
                return;

            //if ai turn
            if (aiCoroutine != null)
                return;

            //if already moved
            if (GameManager.instance.moved)
                return;      

            //getting mouse to grid coordinates
            int x = -1, y = -1;
            if (MouseToGrid(ref x, ref y))
            {
                ClearHighlights();

                //select piece if none selected
                if (selectedPiece == null)
                    SelectPiece(x, y);
                //perform move on selected piece
                else
                    PerformMove(x, y);
            }
        }

        //lerping piece
        if (movingPiece != null)
            movingPiece.transform.position = Vector3.Lerp(movingPiece.transform.position, movingTarget, Time.deltaTime * 8);
    }

    public void AITurn()
    {
        aiCoroutine = StartCoroutine(AIDecision());
    }

    IEnumerator AIDecision()
    {
        yield return new WaitForSeconds(1);

        int maxScore = -1;
        List<MoveInfo> bestMoves = new List<MoveInfo>();
        List<Piece> allPieces = new List<Piece>();
        if (GameManager.instance.turn == Piece.PieceType.black)
            allPieces = blackPieces;
        else
            allPieces = whitePieces;

        //check all pieces
        for (int i = 0; i < allPieces.Count; i++)
        {
            if (!allPieces[i].gameObject.activeSelf)
                continue;

            selectedPiece = allPieces[i];

            CalculateValidMoves();

            //check all valid moves
            foreach (MoveInfo moveInfo in validMoves)
            {
                int currentScore = CalculateMoveScore(moveInfo);

                //store best moves in list
                if (currentScore > maxScore)
                {
                    maxScore = currentScore;
                    bestMoves.Clear();
                    bestMoves.Add(moveInfo);
                }
                else if (currentScore == maxScore)
                {
                    bestMoves.Add(moveInfo);
                }
            }
        }

        yield return new WaitForSeconds(Random.Range(0, 1));

        if (bestMoves.Count > 0)
        {
            //choose a random best move from the list and perform it
            MoveInfo bestMove = bestMoves[Random.Range(0, bestMoves.Count)];
            selectedPiece = bestMove.piece;
            PerformMove(bestMove.endX, bestMove.endY);
        }
        else
        {
            //checkmate
            if (GameManager.instance.turn == Piece.PieceType.black)
                GameManager.instance.Win(Piece.PieceType.white);
            else
                GameManager.instance.Win(Piece.PieceType.black);
        }

        yield return new WaitForSeconds(1);

        //GameManager.instance.EndTurn();
        aiCoroutine = null;
    }

    void InitializeBoard()
    {
        pieces = new Piece[boardSize, boardSize];
        //create alternating black and white pieces
        for (int y = 0; y < boardSize; y++)
            for (int x = 0; x < boardSize; x++)
                if ((y + x) % 2 == 0)
                    if (y < 3)
                        CreatePiece(Piece.PieceType.white, x, y);
                    else if (y > 4)
                        CreatePiece(Piece.PieceType.black, x, y);
    }

    void CreatePiece(Piece.PieceType pieceType, int x, int y)
    {
        //create piece in specified position
        Piece newPiece = Instantiate(piecePrefab, new Vector3(x, y), Quaternion.identity).GetComponent<Piece>();
        newPiece.UpdateType(pieceType);
        newPiece.UpdatePosition(x, y);

        pieces[x, y] = newPiece;

        if (pieceType == Piece.PieceType.black)
            blackPieces.Add(newPiece);
        else
            whitePieces.Add(newPiece);
    }

    void ClearHighlights()
    {
        //clear any previous highlights
        foreach (GameObject highlight in highlights)
            Destroy(highlight);
        highlights.Clear();
    }

    void SelectPiece(int x, int y)
    {
        //if empty space
        if (pieces[x, y] == null)
            return;

        //selecting piece
        if (pieces[x, y].pieceType == GameManager.instance.turn)
        {
            selectedPiece = pieces[x, y];
            selectedPiece.GetSelected();
            CalculateValidMoves();
            ShowValidMoves();
        }
    }

    void PerformMove(int x, int y)
    {
        Piece middlePiece = null;
        bool promoted = false;

        //check if performed move is valid
        int result = IsValidMove(selectedPiece, x, y);

        bool condition;
        if (GameManager.instance.gameMode == GameManager.GameMode.playerVsAI || GameManager.instance.gameMode == GameManager.GameMode.AIvsAI)
            condition = result > 0;
        else
            condition = result > 0 && validMoves.Any(move => move.endX == x && move.endY == y);

        if (condition) //&& validMoves.Any(move => move.endX == x && move.endY == y))
        {
            //normal move
            if (result == 1)
            {
                AudioManager.instance.PlayMoveSound();
            }
            //capturing
            else if (result == 2)
            {
                AudioManager.instance.PlayCaptureSound();
                middlePiece = CapturePiece(x, y);
            }
            //promoting
            else if (result == 3)
            {
                AudioManager.instance.PlayMoveSound();
                promoted = PromoteToKing(y);
            }
            //capturing and promoting
            else if (result == 4)
            {
                AudioManager.instance.PlayCaptureSound();
                middlePiece = CapturePiece(x, y);
                promoted = PromoteToKing(y);
            }

            //push the move information to the stack
            PushMoveToStack(selectedPiece, x, y, middlePiece, promoted);
            validMoves.Clear();

            //update board pieces
            pieces[selectedPiece.x, selectedPiece.y] = null;
            pieces[x, y] = selectedPiece;
            selectedPiece.UpdatePosition(x, y);
            movingPiece = selectedPiece.transform;
            movingTarget = new Vector3(x, y, 0);

            bool moreJumpsLeft = false;
            //continue if no more capture moves left
            if (result == 2 || result == 4)
            {
                moreJumpsLeft = CalculateValidMoves();
            }
            if (!moreJumpsLeft)
            {
                GameManager.instance.PerformedMove();

                if (GameManager.instance.gameMode == GameManager.GameMode.playerVsAI && GameManager.instance.turn != GameManager.instance.playerTurn)
                    StartCoroutine(EndAITurn());
                else if (GameManager.instance.gameMode == GameManager.GameMode.AIvsAI)
                    StartCoroutine(EndAITurn());
            }
            else
            {
                if (GameManager.instance.gameMode == GameManager.GameMode.playerVsAI && GameManager.instance.turn != GameManager.instance.playerTurn)
                    AITurn();
                else if (GameManager.instance.gameMode == GameManager.GameMode.AIvsAI)
                    AITurn();
            }
        }

        selectedPiece.GetDeselected();
        selectedPiece = null;
    }

    IEnumerator EndAITurn()
    {
        yield return new WaitForSeconds(1);
        GameManager.instance.EndTurn();
    }

    void PushMoveToStack(Piece piece, int x, int y, Piece middlePiece, bool promoted)
    {
        //push to stack
        MoveInfo moveInfo = new MoveInfo
        {
            piece = piece,
            startX = piece.x,
            startY = piece.y,
            endX = x,
            endY = y,
            midPiece = middlePiece,
            piecePromoted = promoted
        };
        moveStack.Push(moveInfo);
    }

    public void Undo()
    {
        if (moveStack.Count > 0)
        {
            //pop last move from stack
            MoveInfo lastMove = moveStack.Pop();

            //update board pieces
            selectedPiece = lastMove.piece;
            pieces[selectedPiece.x, selectedPiece.y] = null;
            pieces[lastMove.startX, lastMove.startY] = selectedPiece;
            selectedPiece.UpdatePosition(lastMove.startX, lastMove.startY);
            movingPiece = selectedPiece.transform;
            movingTarget = new Vector3(lastMove.startX, lastMove.startY, 0);

            //if last move had capture
            if (lastMove.midPiece != null)
            {
                lastMove.midPiece.gameObject.SetActive(true);
                pieces[lastMove.midPiece.x, lastMove.midPiece.y] = lastMove.midPiece;

                //undoing score
                GameManager.instance.UpdateScore(lastMove.piece.pieceType, -1);
            }

            //if last move had promotion
            if (lastMove.piecePromoted == true && selectedPiece.isKing)
                selectedPiece.Demote();

            selectedPiece = null;
            GameManager.instance.PerformedUndo();
        }
    }

    bool CalculateValidMoves()
    {
        validMoves.Clear();
        List<Piece> allPieces = new List<Piece>();
        if (GameManager.instance.turn == Piece.PieceType.black)
            allPieces = blackPieces;
        else
            allPieces = whitePieces;

        //check if any jump moves exist
        for (int i = 0; i < allPieces.Count; i++)
        {
            if (!allPieces[i].gameObject.activeSelf)
                continue;

            //check for capture moves
            CheckValidMove(allPieces[i], Mathf.Clamp(allPieces[i].x + 2, 0, boardSize - 1), Mathf.Clamp(allPieces[i].y + 2, 0, boardSize - 1));
            CheckValidMove(allPieces[i], Mathf.Clamp(allPieces[i].x - 2, 0, boardSize - 1), Mathf.Clamp(allPieces[i].y + 2, 0, boardSize - 1));
            CheckValidMove(allPieces[i], Mathf.Clamp(allPieces[i].x + 2, 0, boardSize - 1), Mathf.Clamp(allPieces[i].y - 2, 0, boardSize - 1));
            CheckValidMove(allPieces[i], Mathf.Clamp(allPieces[i].x - 2, 0, boardSize - 1), Mathf.Clamp(allPieces[i].y - 2, 0, boardSize - 1));
        }

        //if no jump moves
        if (validMoves.Count <= 0)
        {
            //check for regular diagonal moves
            CheckValidMove(selectedPiece, Mathf.Clamp(selectedPiece.x + 1, 0, boardSize - 1), Mathf.Clamp(selectedPiece.y + 1, 0, boardSize - 1));
            CheckValidMove(selectedPiece, Mathf.Clamp(selectedPiece.x - 1, 0, boardSize - 1), Mathf.Clamp(selectedPiece.y + 1, 0, boardSize - 1));
            CheckValidMove(selectedPiece, Mathf.Clamp(selectedPiece.x + 1, 0, boardSize - 1), Mathf.Clamp(selectedPiece.y - 1, 0, boardSize - 1));
            CheckValidMove(selectedPiece, Mathf.Clamp(selectedPiece.x - 1, 0, boardSize - 1), Mathf.Clamp(selectedPiece.y - 1, 0, boardSize - 1));
        }
        else
        {
            return true;
        }

        return false;
    }

    int CalculateMoveScore(MoveInfo move)
    {
        //capture and promotion
        if (!move.piece.isKing && move.midPiece != null && move.piecePromoted)
            return 6;
        //promotion
        else if (!move.piece.isKing && move.midPiece == null && move.piecePromoted)
            return 5;
        //capture
        else if (move.midPiece != null && !move.piecePromoted)
            return 4;
        //simple movement
        else
        {
            //if the piece is not a king, prioritize moving to the other side of the board
            if (!move.piece.isKing)
            {
                //if black
                if (move.piece.pieceType == Piece.PieceType.black)
                {
                    if (move.endY < move.startY)
                    { 
                        return 2;
                    }

                }
                else
                {
                    if (move.endY > move.startY)
                    {
                        return 2;
                    }
                }
            }
            //if the piece is a king, prioritize moving towards the opponent
            else
            {   
                //iterate through the pieces and sum up the positions
                float sumX = 0;
                float sumY = 0;

                List<Piece> allPieces = new List<Piece>();
                if (GameManager.instance.turn == Piece.PieceType.black)
                    allPieces = whitePieces;
                else
                    allPieces = blackPieces;
                foreach (Piece piece in allPieces)
                {
                    sumX += piece.x;
                    sumY += piece.y;
                }

                // Calculate the average position
                float averageX = sumX / allPieces.Count;
                float averageY = sumY / allPieces.Count;

                Vector2 pieceStartPos = new Vector2(move.startX, move.startY);
                Vector2 pieceEndPos = new Vector2(move.endX, move.endY);
                Vector2 avgPos = new Vector2(averageX, averageY);

                //check if move brings piece closer to opponent
                if (Vector2.Distance(pieceEndPos, avgPos) < Vector2.Distance(pieceStartPos, avgPos))
                    return 3;
            }

            //default score for simple movement
            return 1;
        }
    }

    void ShowValidMoves()
    {
        //highlight valid moves
        foreach (MoveInfo move in validMoves)
            if (move.piece == selectedPiece)
                highlights.Add(Instantiate(highlightPrefab, new Vector3(move.endX, move.endY, 0), Quaternion.identity));
    }

    bool CheckValidMove(Piece piece, int x, int y)
    {
        //add move to valid moves list if it's valid
        if (IsValidMove(piece, x, y) > 0)
        {
            MoveInfo move = new MoveInfo();
            move.piece = piece;
            move.startX = piece.x;
            move.startY = piece.y;
            move.endX = x;
            move.endY = y;
            validMoves.Add(move);
            return true;
        }

        return false;
    }

    int IsValidMove(Piece piece, int x, int y)
    {
        //if already occupied
        if (pieces[x, y] != null)
            return 0;

        //normal piece
        if (!piece.isKing)
        {
            //allowed row distance based on the piece color
            int allowedRowDistance = piece.pieceType == Piece.PieceType.black ? -1 : 1;

            if (y - piece.y == allowedRowDistance && Mathf.Abs(x - piece.x) == 1)
            {
                if (y == 0 || y == boardSize - 1)
                    return 3;   //promotion
                else
                    return 1;   //diagonal move
            }
            else if (y - piece.y == (allowedRowDistance * 2) && Mathf.Abs(x - piece.x) == 2)
            {
                if (GetMiddlePiece(piece, x, y) != null)
                {
                    if (y == 0 || y == boardSize - 1)
                        return 4;   //capture and promotion
                    else
                        return 2;   //capture
                }
            }
        }
        //king piece
        else
        {
            if (Mathf.Abs(y - piece.y) == 1 && Mathf.Abs(x - piece.x) == 1)
            {
                if (y == 0 || y == boardSize - 1)  
                    return 3;   //promotion
                else
                    return 1;   //diagonal move
            }
            else if (GetMiddlePiece(piece, x, y) != null)
                return 2;   //capture
        }

        //invalid move
        return 0;
    }

    bool IsOpponent(Piece piece1, Piece piece2)
    {
        //check if two pieces are opponents
        if (piece1.pieceType != piece2.pieceType)
            return true;

        return false;
    }

    Piece CapturePiece(int x, int y)
    {
        //check if there's an opponent's piece in the middle
        Piece middlePiece = GetMiddlePiece(selectedPiece, x, y);
        if (middlePiece != null)
        {
            //capture that piece
            middlePiece.gameObject.SetActive(false);
            pieces[middlePiece.x, middlePiece.y] = null;

            //add score
            GameManager.instance.UpdateScore(selectedPiece.pieceType, 1);

            return middlePiece;
        }

        return null;
    }

    bool PromoteToKing(int row)
    {
        //promote piece to king
        if ((selectedPiece.pieceType == Piece.PieceType.black && row == 0) || (selectedPiece.pieceType == Piece.PieceType.white && row == boardSize - 1))
        {
            selectedPiece.Promote();
            return true;
        }

        return false;
    }

    Piece GetMiddlePiece(Piece piece, int x, int y)
    {
        //get piece in middle
        if (Mathf.Abs(y - piece.y) == 2 && Mathf.Abs(x - piece.x) == 2)
        {
            int middleX = (x + piece.x) / 2;
            int middleY = (y + piece.y) / 2;
            Piece middlePiece = pieces[middleX, middleY];

            if (middlePiece != null && IsOpponent(piece, middlePiece))
                return middlePiece;
        }

        return null;
    }


    bool MouseToGrid(ref int x, ref int y)
    {
        //getting mouse position
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Camera.main.nearClipPlane;
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);

        //getting grid position
        x = Mathf.RoundToInt(worldPosition.x);
        y = Mathf.RoundToInt(worldPosition.y);

        //if invalid position
        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize)
            return false;

        return true;
    }

    //debugging
    private void OnGUI()
    {
        if (!displayGrid)
            return;

        int cellSize = 50;
        int fontSize = 24;
        int startX = 100;
        int startY = 700;
        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;

        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                string value = "";
                if (pieces[j, i] != null)
                {
                    if (pieces[j, i].pieceType == Piece.PieceType.black && !pieces[j, i].isKing)
                        value = "B";
                    else if (pieces[j, i].pieceType == Piece.PieceType.white && !pieces[j, i].isKing)
                        value = "W";
                    else if (pieces[j, i].pieceType == Piece.PieceType.black && pieces[j, i].isKing)
                        value = "KB";
                    else if (pieces[j, i].pieceType == Piece.PieceType.white && pieces[j, i].isKing)
                        value = "KW";
                }

                string text = value.ToString();
                int xPos = startX + j * cellSize;
                int yPos = startY - i * cellSize;
                GUI.Label(new Rect(xPos, yPos, cellSize, cellSize), text, style);
            }
        }
    }
}
