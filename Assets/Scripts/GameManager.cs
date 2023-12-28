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
    public bool displayGrid;

    //TODO: dont use tags and your base for comparisons 
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;
    public GameObject highlightPrefab;
    public GameObject undoEndPanel;
    public GameObject undoButton;
    public Text turnText;

    private const int boardSize = 8;
    private GameObject[,] board = new GameObject[boardSize, boardSize];

    private GameObject selectedObject;
    private Transform movingObject;
    private Vector3 movingTarget;

    private List<Vector2Int> validMoves = new List<Vector2Int>();
    private List<GameObject> highlights = new List<GameObject>();
    private Stack<MoveInfo> moveStack = new Stack<MoveInfo>();

    bool moved = false;
    bool turn = false;
    public int whiteScore = 0;
    public int blackScore = 0;

    //TODO: move classes like these into their own file, or a single shared file separate from your mono scripts
    private class MoveInfo
    {
        public GameObject Piece;
        public int StartRow;
        public int StartCol;
        public int EndRow;
        public int EndCol;
        public GameObject midPiece;
        public bool piecePromoted;
    }


    //debugging
    private void OnGUI()
    {
        if (!displayGrid)
            return;

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
        undoEndPanel.SetActive(false);
        UpdateTurnText();

        InitializeBoard();
    }

    private void Update()
    {
        //TODO: move this into its own class
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

    //TODO: move into a grid/board class 
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

    //TODO: remove redundant return 
    GameObject CreatePiece(GameObject prefab, int row, int col)
    {
        GameObject piece = Instantiate(prefab, new Vector3(col, row), Quaternion.identity);
        board[col, row] = piece;
        return piece;
    }

    void UpdateTurnText()
    {
        if (!turn)
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

    //TODO: rename function, this is now a super function that is doing too many things 
    //TODO: break the functionality of this function into separate functions 
    void Clicked(int x, int y)
    {
        //clearing any previous highlights
        //TODO: if your going to use a separate object for highlighting, use object pooling
        //TODO: highlight can just be a function of the checker that enables or changes sprite 
        foreach (GameObject highlight in highlights)
        {
            Destroy(highlight);
        }
        highlights.Clear();

        //if invalid position
        //TODO: user the board size variable you declared here
        if (x < 0 || x > 7 || y < 0 || y > 7) return;

        //if already moved
        if (moved)
            return;

        print(y + ", " + x);

        //performing action on selected object
        if (selectedObject != null)
        {
            GameObject middlePiece = null;
            bool promoted = false;
            if (IsValidMove(y, x) > 0)
            {
                //capturing
                if (IsValidMove(y, x) == 2)
                {
                    middlePiece = CapturePiece(y, x);
                }
                //promoting
                if (IsValidMove(y, x) == 3)
                {
                    PromoteToKing(y, x);
                    promoted = true;
                }
                //capturing and promoting
                if (IsValidMove(y, x) == 4)
                {
                    middlePiece = CapturePiece(y, x);
                    PromoteToKing(y, x);
                    promoted = true;
                }

                //store the move information in the stack
                MoveInfo moveInfo = new MoveInfo
                {
                    Piece = selectedObject,
                    StartRow = Mathf.RoundToInt(selectedObject.transform.position.y),
                    StartCol = Mathf.RoundToInt(selectedObject.transform.position.x),
                    EndRow = y,
                    EndCol = x,
                    midPiece = middlePiece,
                    piecePromoted = promoted
                };
                moveStack.Push(moveInfo);

                moved = true;
                undoButton.SetActive(true);
                undoEndPanel.SetActive(true);
                CheckWin();

                board[Mathf.RoundToInt(selectedObject.transform.position.x), Mathf.RoundToInt(selectedObject.transform.position.y)] = null;
                board[x, y] = selectedObject;
                movingObject = selectedObject.transform;
                movingTarget = new Vector3(x, y, 0);
                //turn = !turn;
                print("Valid move");
            }
            else
            {
                print("Invalid move");
            }
            selectedObject.transform.localScale = Vector3.one;
            selectedObject.GetComponent<SpriteRenderer>().color = Color.white;
            selectedObject = null;

            return;
        }

        //selecting object
        if (board[x, y] == null)
            return;

        //turn
        if (!turn)
        {
            if (board[x, y].CompareTag("Black") || board[x, y].CompareTag("BlackKing"))
            {
                selectedObject = board[x, y];
            }
            else
            {
                selectedObject = null;
                print("Black's turn!");
            }
        }
        else
        {
            if (board[x, y].CompareTag("White") || board[x, y].CompareTag("WhiteKing"))
            {
                selectedObject = board[x, y]; 
            }
            else
            {
                selectedObject = null;
                print("White's turn!");
            }
        }

        if (selectedObject != null)
        {
            print("Clicked " + selectedObject.tag);
            selectedObject.transform.localScale = new Vector3(1.1f, 1.1f, 1f);
            selectedObject.GetComponent<SpriteRenderer>().color = Color.gray;
            CalculateValidMoves();
            ShowValidMoves();
        }          
    }

    public void Undo()
    {
        if (moveStack.Count > 0)
        {
            MoveInfo moveInfo = moveStack.Pop();
            
            print("Undo: " + moveInfo.StartRow + ", " + moveInfo.StartCol);
            
            selectedObject = moveInfo.Piece;
            
            board[Mathf.RoundToInt(selectedObject.transform.position.x), Mathf.RoundToInt(selectedObject.transform.position.y)] = null;
            
            //TODO: keep the x,y notation, x = rows, y = col
            board[moveInfo.StartCol, moveInfo.StartRow] = selectedObject;
            
            movingObject = selectedObject.transform;
            movingTarget = new Vector3(moveInfo.StartCol, moveInfo.StartRow, 0);
            
            undoButton.SetActive(false);
            undoEndPanel.SetActive(false);
            moved = false;
            //turn = !turn;

            //if capture
            if (moveInfo.midPiece != null)
            {
                moveInfo.midPiece.SetActive(true);
                
                board[Mathf.RoundToInt(moveInfo.midPiece.transform.position.x), Mathf.RoundToInt(moveInfo.midPiece.transform.position.y)] = moveInfo.midPiece;

                //undoing score
                if (moveInfo.Piece.CompareTag("Black") || moveInfo.Piece.CompareTag("BlackKing"))
                {
                    blackScore--;
                }
                else if (moveInfo.Piece.CompareTag("White") || moveInfo.Piece.CompareTag("WhiteKing"))
                {
                    whiteScore--;
                }
            }

            //if promotion
            if (moveInfo.piecePromoted == true)
            {
                if (selectedObject.CompareTag("BlackKing"))
                {
                    selectedObject.tag = "Black";
                    selectedObject.transform.GetChild(0).gameObject.SetActive(false);
                }
                else if (selectedObject.CompareTag("WhiteKing"))
                {
                    selectedObject.tag = "White";
                    selectedObject.transform.GetChild(0).gameObject.SetActive(false);
                }
            }

            selectedObject = null;
        }
    }

    public void EndTurn()
    {
        moved = false;
        turn = !turn;
        undoEndPanel.SetActive(false);
        UpdateTurnText();
    }

    void CalculateValidMoves()
    {
        validMoves.Clear();

        if (selectedObject == null)
            return;

        //TODO: just use a grid index or keep the position of the piece stored in a variable rather then using transform 
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

    //TODO: can be made much more simple, just have to check the diagonals of the selected piece 
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
                            return 4;
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
                        /*if (targetRow == 0 || targetRow == boardSize - 1)
                        {
                            return 4;
                        }*/

                        return 2;
                    }
                }
            }
        }

        return 0;
    }

    //TODO: Can be a simple id check
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

    GameObject CapturePiece(int targetRow, int targetCol)
    {
        //TODO: logic that is being reused constantly should be its own function 
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
                middlePiece.SetActive(false);
                //Destroy(middlePiece);
                board[middleCol, middleRow] = null;

                //add score
                if (selectedObject.CompareTag("Black") || selectedObject.CompareTag("BlackKing"))
                {
                    blackScore++;
                }
                else if (selectedObject.CompareTag("White") || selectedObject.CompareTag("WhiteKing"))
                {
                    whiteScore++;
                }

                return middlePiece;
            }

            return null;
        }

        return null;
    }

    void CheckWin()
    {
        if (blackScore >= 12)
        {
            print("Black won!");
            RestartScene();
        }
        else if (whiteScore >= 12)
        {
            print("White won!");
            RestartScene();
        }
    }

    void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    //TODO: logic is fine, remove tag comparisons 
    void PromoteToKing(int row, int col)
    {
        GameObject newKing = null;
        if (row == boardSize - 1 && selectedObject.CompareTag("White"))
        {
            //white piece to white king
            //newKing = CreatePiece(whiteKingPrefab, row, col);
            selectedObject.tag = "WhiteKing";
            selectedObject.transform.GetChild(0).gameObject.SetActive(true);
            print("White promoted to king");
        }
        else if (row == 0 && selectedObject.CompareTag("Black"))
        {
            //black piece to black king
            //newKing = CreatePiece(blackKingPrefab, row, col);
            selectedObject.tag = "BlackKing";
            selectedObject.transform.GetChild(0).gameObject.SetActive(true);
            print("Black promoted to king");
        }

        //Destroy(selectedObject);
        //selectedObject.SetActive(false);
        //board[Mathf.RoundToInt(selectedObject.transform.position.x), Mathf.RoundToInt(selectedObject.transform.position.y)] = null;
        //selectedObject = newKing;
    }
}
