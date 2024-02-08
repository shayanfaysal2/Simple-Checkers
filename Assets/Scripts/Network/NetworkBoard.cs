using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System.Linq;

public class NetworkBoard : NetworkBehaviour
{
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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if not game started
        if (!NetworkGameManager.instance.gameStarted)
            return;

        //left mouse click
        if (Input.GetMouseButtonDown(0))
        {
            //if not own turn
            if (NetworkGameManager.instance.turn != NetworkGameManager.instance.playerTurn)
                return;

            //if already moved
            if (NetworkGameManager.instance.moved)
                return;

            //getting mouse to grid coordinates
            int x = -1, y = -1;
            if (MouseToGrid(ref x, ref y))
            {
                ClearHighlights();

                //select piece if none selected
                if (selectedPiece == null)
                {
                    print("selecting piece");
                    print("turn = " + NetworkGameManager.instance.turn);
                    print("me = " + NetworkGameManager.instance.playerTurn);
                    SelectPieceServerRpc(x, y);
                }
                //perform move on selected piece
                else
                {
                    print("performing move on piece");
                    PerformMoveServerRpc(x, y);
                }
            }
        }

        //lerping piece
        if (movingPiece != null)
            movingPiece.transform.position = Vector3.Lerp(movingPiece.transform.position, movingTarget, Time.deltaTime * 8);
    }

    public void InitializeBoard()
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

    [ServerRpc(RequireOwnership = false)]
    void SelectPieceServerRpc(int x, int y)
    {
        SelectPieceClientRpc(x, y);
    }

    [ClientRpc]
    void SelectPieceClientRpc(int x, int y)
    {
        //if empty space
        if (pieces[x, y] == null)
            return;

        //selecting piece
        selectedPiece = pieces[x, y];
        CalculateValidMoves();
        if (NetworkGameManager.instance.turn == NetworkGameManager.instance.playerTurn)
        {
            if (pieces[x, y].pieceType == NetworkGameManager.instance.turn)
            {
                selectedPiece.GetSelected();
                ShowValidMoves();
            }   
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void PerformMoveServerRpc(int x, int y)
    {
        PerformMoveClientRpc(x, y);
    }

    [ClientRpc]
    void PerformMoveClientRpc(int x, int y)
    {
        print("performing move");

        Piece middlePiece = null;
        bool promoted = false;

        //check if performed move is valid
        int result = IsValidMove(selectedPiece, x, y);

        bool condition = result > 0 && validMoves.Any(move => move.endX == x && move.endY == y);

        if (condition) //&& validMoves.Any(move => move.endX == x && move.endY == y))
        {
            //capturing
            if (result == 2)
            {
                middlePiece = CapturePiece(x, y);
            }
            //promoting
            else if (result == 3)
            {
                promoted = PromoteToKing(y);
            }
            //capturing and promoting
            else if (result == 4)
            {
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
                if (NetworkGameManager.instance.turn == NetworkGameManager.instance.playerTurn)
                    NetworkGameManager.instance.PerformedMoveServerRpc();
                    //NetworkGameManager.instance.PerformedMove();
            }
        }

        selectedPiece.GetDeselected();
        selectedPiece = null;
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
                NetworkGameManager.instance.UpdateScore(lastMove.piece.pieceType, -1);
            }

            //if last move had promotion
            if (lastMove.piecePromoted == true && selectedPiece.isKing)
                selectedPiece.Demote();

            selectedPiece = null;
            NetworkGameManager.instance.PerformedUndo();
        }
    }

    bool CalculateValidMoves()
    {
        validMoves.Clear();
        List<Piece> allPieces = new List<Piece>();
        if (NetworkGameManager.instance.turn == Piece.PieceType.black)
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
            NetworkGameManager.instance.UpdateScore(selectedPiece.pieceType, 1);

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
}
