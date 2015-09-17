using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour
{
    public static int BOARD_SIZE = 3;
    public const int INROW = 3;
    public const int TURN_X = 1;
    public const int TURN_O = 2;

    public const int RESULT_NONE = -1;
    public const int RESULT_DRAW = 0;
    public const int RESULT_X = 1;
    public const int RESULT_O = 2;

    public float startX, startY;
    [HideInInspector] public int[][] boardState;
    [HideInInspector] public int pieceNumber;
    [HideInInspector] public int currentTurn;
    [HideInInspector] public Point lastPos;
    [HideInInspector] public int result;

    public Transform squarePrefab;

    // Use this for initialization
    void Start()
    {
        initBoard();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void initBoard()
    {
        currentTurn = Board.TURN_X;
        pieceNumber = 0;
        lastPos = null;
        result = RESULT_NONE;

        //init boardState
        boardState = new int[BOARD_SIZE][];
        for (int i = 0; i < boardState.Length; i++)
        {
            boardState[i] = new int[BOARD_SIZE];
            for (int j = 0; j < boardState[i].Length; j++)
            {
                boardState[i][j] = Square.SQUARE_EMPTY;
            }
        }

        float squareSize = squarePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        //Render the board
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                Transform squareObj = Instantiate(squarePrefab, new Vector3(startX + (i * squareSize), startY + ((j - (BOARD_SIZE - 1)) * squareSize), 0), Quaternion.identity) as Transform;
                squareObj.SetParent(this.transform);
                Square squareScript = squareObj.GetComponent<Square>();
                squareScript.posX = i;
                squareScript.posY = j;
            }
        }
    }

    public void selectSquare(int posX, int posY)
    {
        //TODO optimize?
        foreach (Square square in this.GetComponentsInChildren<Square>())
        {
            if (square.posX == posX && square.posY == posY)
            {
                if (square.status == Square.SQUARE_EMPTY)
                {
                    square.status = (currentTurn == TURN_X ? Square.SQUARE_X : Square.SQUARE_O);
                    updateSquare(posX, posY, square.status);
                    switchTurn();
                }
                else
                {
                    Debug.Log("selected square is not empty");
                }
                break;
            }
        }
    }

    public void switchTurn()
    {
        currentTurn = 3 - currentTurn;
    }

    public void updateSquare(int x, int y, int currTurn)
    {
        boardState[x][y] = currTurn;
        pieceNumber++;
        lastPos = new Point(x, y);

        //Debug.Log("updated at " + x + "," + y + "with " + boardState[x][y]);

        switch (checkWin(currTurn, x, y))
        {
            case RESULT_X:
            {
                result = RESULT_X;
                Debug.Log("X wins");
                break;
            }
            case RESULT_O:
            {
                result = RESULT_O;
                Debug.Log("O wins");
                break;
            }
            case RESULT_DRAW:
            {
                result = RESULT_DRAW;
                Debug.Log("Draw");
                break;
            }
        }
    }

    public int checkWin(int currTurn, int lastX, int lastY)
    {
        int result = RESULT_NONE;

        if (checkHor(currTurn, lastX, lastY) >= INROW
            || checkVer(currTurn, lastX, lastY) >= INROW
            || checkDiag1(currTurn, lastX, lastY) >= INROW
            || checkDiag2(currTurn, lastX, lastY) >= INROW)
        {
            result = (currTurn == Square.SQUARE_X ? RESULT_X : RESULT_O);
        }
        else if (pieceNumber == BOARD_SIZE * BOARD_SIZE)
        {
            result = RESULT_DRAW;
        }
        return result;
    }

    public int checkHor(int currTurn, int lastX, int lastY)
    {
        return countRow(currTurn, lastX, lastY, -1, 0) + countRow(currTurn, lastX, lastY, 1, 0) - 1; //left + right
    }

    public int checkVer(int currTurn, int lastX, int lastY)
    {
        return countRow(currTurn, lastX, lastY, 0, 1) + countRow(currTurn, lastX, lastY, 0, -1) - 1; //up + down;
    }

    public int checkDiag1(int currTurn, int lastX, int lastY)
    {
        return countRow(currTurn, lastX, lastY, -1, 1) + countRow(currTurn, lastX, lastY, 1, -1) - 1; //up-left + down-right;
    }

    public int checkDiag2(int currTurn, int lastX, int lastY)
    {
        return countRow(currTurn, lastX, lastY, 1, 1) + countRow(currTurn, lastX, lastY, -1, -1) - 1; //up-right + down-left;
    }

    //counts the number of pieces in a row, given offsetX and offsetY as the counting direction
    public int countRow(int currTurn, int initX, int initY, int offsetX, int offsetY)
    {
        int result = 0;
        int currX = initX;
        int currY = initY;
        bool rowEnded = false;

        while ((currX >= 0) && (currX < BOARD_SIZE) && (currY >= 0) && (currY < BOARD_SIZE) && (!rowEnded))
        {
            if (boardState[currX][currY] == currTurn)
            {
                result++;
            }
            else
            {
                rowEnded = true;
            }
            currX += offsetX;
            currY += offsetY;
        }

        return result;
    }
}
