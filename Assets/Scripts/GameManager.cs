using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;
    public GameObject whiteKingPrefab;
    public GameObject blackKingPrefab;
    public GameObject highlightPrefab;

    private const int boardSize = 8;
    private GameObject[,] board = new GameObject[boardSize, boardSize];

    public GameObject selectedObject;
    public Transform movingObject;
    private Vector3 movingTarget;

    private List<Vector2Int> validMoves = new List<Vector2Int>();
    private List<GameObject> highlights = new List<GameObject>();

    //debugging
    private void OnGUI()
    {
        int cellSize = 50;
        int fontSize = 24;
        int startX = 50;
        int startY = 400;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;

        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                int value = 0;
                if (board[j, i] != null)
                {
                    if (board[j, i].CompareTag("Black"))
                    {
                        value = 1;
                    }
                    else if (board[j, i].CompareTag("White"))
                    {
                        value = -1;
                    }
                    else if (board[j, i].CompareTag("BlackKing"))
                    {
                        value = 2;
                    }
                    else if (board[j, i].CompareTag("WhiteKing"))
                    {
                        value = -2;
                    }
                }
                
                string text = value.ToString();
                int xPos = startX + j * cellSize;
                int yPos = startY - i * cellSize;
                GUI.Label(new Rect(xPos, yPos, cellSize, cellSize), text, style);
            }
        }
    }

    private void Start()
    {
        InitializeBoard();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //getting mouse position
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = Camera.main.nearClipPlane;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);

            //getting grid position
            int roundedX = Mathf.RoundToInt(worldPosition.x);
            int roundedY = Mathf.RoundToInt(worldPosition.y);

            Clicked(roundedX, roundedY);
        }

        //lerping piece
        if (movingObject != null)
        {
            movingObject.transform.position = Vector3.Lerp(movingObject.transform.position, movingTarget, Time.deltaTime * 8);
        }
    }

    void InitializeBoard()
    {
        //create alternating white and black pieces
        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                if ((row + col) % 2 == 0)
                {
                    if (row < 3)
                    {
                        CreatePiece(whitePiecePrefab, row, col);
                    }
                    else if (row > 4)
                    {
                        CreatePiece(blackPiecePrefab, row, col);
                    }
                }
            }
        }
    }

    GameObject CreatePiece(GameObject prefab, int row, int col)
    {
        GameObject piece = Instantiate(prefab, new Vector3(col, row), Quaternion.identity);
        board[col, row] = piece;
        return piece;
    }

    void Clicked(int x, int y)
    {
        //clearing any previous highlights
        foreach (GameObject highlight in highlights)
        {
            Destroy(highlight);
        }
        highlights.Clear();

        //if invalid position
        if (x < 0 || x > 7 || y < 0 || y > 7)
        return;

        print(y + ", " + x);

        //performing action on selected object
        if (selectedObject != null)
        {
            if (IsValidMove(y, x) > 0)
            {
                //capturing
                if (IsValidMove(y, x) == 2)
                {
                    CapturePiece(y, x);
                }
                //promoting
                if (IsValidMove(y, x) == 3)
                {
                    PromoteToKing(y, x);
                }

                board[Mathf.RoundToInt(selectedObject.transform.position.x), Mathf.RoundToInt(selectedObject.transform.position.y)] = null;
                board[x, y] = selectedObject;
                movingObject = selectedObject.transform;
                movingTarget = new Vector3(x, y, 0);
                print("Valid move");
            }
            else
            {
                print("Invalid move");
            }
            selectedObject.GetComponent<SpriteRenderer>().color = Color.white;
            selectedObject = null;

            return;
        }

        //selecting object
        selectedObject = board[x, y];
        if (selectedObject != null)
        {
            print("Clicked " + selectedObject.tag);
            selectedObject.GetComponent<SpriteRenderer>().color = Color.gray;
            CalculateValidMoves();
            ShowValidMoves();
        }          
        else
        {
            print("Clicked empty");
        }
    }

    void CalculateValidMoves()
    {
        validMoves.Clear();

        if (selectedObject == null)
            return;

        int currentRow = Mathf.RoundToInt(selectedObject.transform.position.y);
        int currentCol = Mathf.RoundToInt(selectedObject.transform.position.x);

        //check for regular diagonal moves
        CheckValidMove(Mathf.Clamp(currentRow + 1, 0, boardSize-1), Mathf.Clamp(currentCol + 1, 0, boardSize-1));
        CheckValidMove(Mathf.Clamp(currentRow + 1, 0, boardSize-1), Mathf.Clamp(currentCol - 1, 0, boardSize-1));
        CheckValidMove(Mathf.Clamp(currentRow - 1, 0, boardSize-1), Mathf.Clamp(currentCol + 1, 0, boardSize-1));
        CheckValidMove(Mathf.Clamp(currentRow - 1, 0, boardSize-1), Mathf.Clamp(currentCol - 1, 0, boardSize-1));

        //check for capturing moves...
        CheckValidMove(Mathf.Clamp(currentRow + 2, 0, boardSize - 1), Mathf.Clamp(currentCol + 2, 0, boardSize - 1));
        CheckValidMove(Mathf.Clamp(currentRow + 2, 0, boardSize - 1), Mathf.Clamp(currentCol - 2, 0, boardSize - 1));
        CheckValidMove(Mathf.Clamp(currentRow - 2, 0, boardSize - 1), Mathf.Clamp(currentCol + 2, 0, boardSize - 1));
        CheckValidMove(Mathf.Clamp(currentRow - 2, 0, boardSize - 1), Mathf.Clamp(currentCol - 2, 0, boardSize - 1));
    }

    void ShowValidMoves()
    {
        foreach (Vector2Int move in validMoves)
        {
            int row = move.x;
            int col = move.y;
            print("Valid: " + row + ", " + col);
            GameObject newHighlight = Instantiate(highlightPrefab, new Vector3(col, row, 0), Quaternion.identity);
            highlights.Add(newHighlight);
        }
    }

    void CheckValidMove(int row, int col)
    {
        if (IsValidMove(row, col) > 0)
        {
            validMoves.Add(new Vector2Int(row, col));
        }
    }

    int IsValidMove(int targetRow, int targetCol)
    {
        if (board[targetCol, targetRow] != null)
            return 0;

        //allowed row distance based on the piece color
        int allowedRowDistance = selectedObject.CompareTag("White") ? 1 : -1;

        int currentRow = Mathf.RoundToInt(selectedObject.transform.position.y);
        int currentCol = Mathf.RoundToInt(selectedObject.transform.position.x);

        //check for regular diagonal move
        if (selectedObject.CompareTag("White") || selectedObject.CompareTag("Black"))
        {
            if (targetRow - currentRow == allowedRowDistance && Mathf.Abs(targetCol - currentCol) == 1)
            {
                //check for checker promotion
                if (targetRow == 0 || targetRow == boardSize - 1)
                {
                    return 3;
                }

                return 1;
            }
        }
        else if (selectedObject.CompareTag("WhiteKing") || selectedObject.CompareTag("BlackKing"))
        {
            if (Mathf.Abs(targetRow - currentRow) == 1 && Mathf.Abs(targetCol - currentCol) == 1)
            {
                //check for checker promotion
                if (targetRow == 0 || targetRow == boardSize - 1)
                {
                    return 3;
                }

                return 1;
            }
        }

        //check for capturing move (jump over opponent's piece)
        if (selectedObject.CompareTag("White") || selectedObject.CompareTag("Black"))
        {
            if (targetRow - currentRow == (allowedRowDistance * 2) && Mathf.Abs(targetCol - currentCol) == 2)
            {
                int middleRow = (targetRow + currentRow) / 2;
                int middleCol = (targetCol + currentCol) / 2;
                GameObject middlePiece = board[middleCol, middleRow];

                //check if there's an opponent's piece in the middle
                if (middlePiece != null)
                {
                    if (IsOpponent(selectedObject, middlePiece))
                    {
                        //check for checker promotion
                        if (targetRow == 0 || targetRow == boardSize - 1)
                        {
                            return 3;
                        }

                        return 2;
                    }
                }
                
            }
        }
        else if (selectedObject.CompareTag("WhiteKing") || selectedObject.CompareTag("BlackKing"))
        {
            if (Mathf.Abs(targetRow - currentRow) == 2 && Mathf.Abs(targetCol - currentCol) == 2)
            {
                int middleRow = (targetRow + currentRow) / 2;
                int middleCol = (targetCol + currentCol) / 2;
                GameObject middlePiece = board[middleCol, middleRow];

                //check if there's an opponent's piece in the middle
                if (middlePiece != null)
                {
                    if (IsOpponent(selectedObject, middlePiece))
                    {
                        //check for checker promotion
                        if (targetRow == 0 || targetRow == boardSize - 1)
                        {
                            return 3;
                        }

                        return 2;
                    }
                }
            }
        }

        return 0;
    }

    bool IsOpponent(GameObject selectedPiece, GameObject middlePiece)
    {
        if ((selectedPiece.CompareTag("White") && middlePiece.CompareTag("Black")) ||
            (selectedPiece.CompareTag("White") && middlePiece.CompareTag("BlackKing")) ||
            (selectedPiece.CompareTag("Black") && middlePiece.CompareTag("White")) ||
            (selectedPiece.CompareTag("Black") && middlePiece.CompareTag("WhiteKing")) ||
            (selectedPiece.CompareTag("WhiteKing") && middlePiece.CompareTag("Black")) ||
            (selectedPiece.CompareTag("WhiteKing") && middlePiece.CompareTag("BlackKing")) ||
            (selectedPiece.CompareTag("BlackKing") && middlePiece.CompareTag("White")) ||
            (selectedPiece.CompareTag("BlackKing") && middlePiece.CompareTag("WhiteKing")))
        {
            return true;
        }

        return false;
    }

    void CapturePiece(int targetRow, int targetCol)
    {
        int currentRow = Mathf.RoundToInt(selectedObject.transform.position.y);
        int currentCol = Mathf.RoundToInt(selectedObject.transform.position.x);

        int middleRow = (targetRow + currentRow) / 2;
        int middleCol = (targetCol + currentCol) / 2;
        GameObject middlePiece = board[middleCol, middleRow];

        //check if there's an opponent's piece in the middle
        if (middlePiece != null)
        {
            if (IsOpponent(selectedObject, middlePiece))
            {
                //capture is valid
                Destroy(middlePiece);
                board[middleCol, middleRow] = null;
            }
        }
    }

    void PromoteToKing(int row, int col)
    {
        GameObject newKing = null;
        if (row == boardSize - 1 && selectedObject.CompareTag("White"))
        {
            //white piece to white king
            newKing = CreatePiece(whiteKingPrefab, row, col);
            print("White promoted to king");
        }
        else if (row == 0 && selectedObject.CompareTag("Black"))
        {
            //black piece to black king
            newKing = CreatePiece(blackKingPrefab, row, col);
            print("Black promoted to king");
        }

        Destroy(selectedObject);
        selectedObject = newKing;
    }
}
