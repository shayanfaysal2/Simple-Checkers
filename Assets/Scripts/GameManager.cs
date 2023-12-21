using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject whitePiecePrefab;
    public GameObject blackPiecePrefab;

    private const int boardSize = 8;
    private GameObject[,] board = new GameObject[boardSize, boardSize];

    public GameObject selectedObject;

    private void OnGUI()
    {
        int cellSize = 50; // Adjust the cell size as needed
        int fontSize = 24; // Adjust the font size as needed
        int startX = 50;
        int startY = 400;

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = fontSize;

        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                int value = -1;
                if (board[j, i] != null)
                {
                    if (board[j, i].CompareTag("Black"))
                    {
                        value = 0;
                    }
                    else if (board[j, i].CompareTag("White"))
                    {
                        value = 1;
                    }
                }
                
                string text = value.ToString();

                // Adjust the position based on cell size and array indices
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
            int roundedX = (int)Mathf.Round(worldPosition.x);
            int roundedY = (int)Mathf.Round(worldPosition.y);

            Clicked(roundedX, roundedY);
        }
    }

    void InitializeBoard()
    {
        // Create alternating white and black pieces
        for (int row = 0; row < boardSize; row++)
        {
            for (int col = 0; col < boardSize; col++)
            {
                //board[row, col] = -1;

                if ((row + col) % 2 == 0)
                {
                    // Odd row + odd column or even row + even column
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

    void CreatePiece(GameObject prefab, int row, int col)
    {
        GameObject piece = Instantiate(prefab, new Vector3(col, row), Quaternion.identity);
        //board[row, col] = prefab == blackPiecePrefab ? 0 : 1;
        board[col, row] = piece;
    }

    void Clicked(int x, int y)
    {
        

        //if invalid position
        if (x < 0 || x > 7 || y < 0 || y > 7)
            return;

        print(y + ", " + x);

        //performing action on selected object
        if (selectedObject != null)
        {
            if (IsValidMove(y, x))
            {
                board[(int)selectedObject.transform.position.x, (int)selectedObject.transform.position.y] = null;
                selectedObject.transform.position = new Vector3(x, y, 0);
                board[x, y] = selectedObject;
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
            if (selectedObject.CompareTag("Black"))
            {
                print("Clicked black");
            }
            else if (selectedObject.CompareTag("White"))
            {
                print("Clicked white");
            }
            selectedObject.GetComponent<SpriteRenderer>().color = Color.gray;
        }          
        else
        {
            print("Clicked empty");
        }
    }

    bool IsValidMove(int targetRow, int targetCol)
    {
        if (board[targetCol, targetRow] != null)
            return false;

        //return Mathf.Abs(targetRow - (int)selectedObject.transform.position.y) == 1 && Mathf.Abs(targetCol - (int)selectedObject.transform.position.x) == 1;

        // Determine the allowed row distance based on the piece color
        int allowedRowDistance = selectedObject.CompareTag("White") ? 1 : -1;

        int currentRow = (int)selectedObject.transform.position.y;
        int currentCol = (int)selectedObject.transform.position.x;

        // Check for regular diagonal move
        if (targetRow - currentRow == allowedRowDistance && Mathf.Abs(targetCol - currentCol) == 1)
        {
            return true;
        }

        // Check for capturing move (jump over opponent's piece)
        if (Mathf.Abs(targetRow - currentRow) == 2 && Mathf.Abs(targetCol - currentCol) == 2)
        {
            int middleRow = (targetRow + currentRow) / 2;
            int middleCol = (targetCol + currentCol) / 2;

            GameObject middlePiece = board[middleCol, middleRow];

            // Check if there's an opponent's piece in the middle
            if (middlePiece != null && middlePiece.CompareTag("Black") && selectedObject.CompareTag("White") ||
                middlePiece != null && middlePiece.CompareTag("White") && selectedObject.CompareTag("Black"))
            {
                // Capture is valid
                Destroy(middlePiece);
                board[middleCol, middleRow] = null;
                return true;
            }
        }

        return false;
    }
}
