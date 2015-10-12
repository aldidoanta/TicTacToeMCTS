using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour
{
    public const int BOARD_SIZE = 3;
    public const int INROW = 3;
    public const char TURN_X = '1';
    public const char TURN_O = '2';

    public const int RESULT_NONE = -1;
    public const int RESULT_DRAW = 0;
    public const int RESULT_X = 1;
    public const int RESULT_O = 2;

    //public float startX, startY;
    [HideInInspector] public char[][] boardState;
    [HideInInspector] public int pieceNumber;
    [HideInInspector] public char currentTurn;
    [HideInInspector] public Point lastPos, lastOPos;
    [HideInInspector] public int result;

    [HideInInspector] public bool isStarted;

    //public Transform squarePrefab;

    // Use this for initialization
    void Start()
    {
        isStarted = false;
        //initBoard();
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
        lastOPos = null;
        result = RESULT_NONE;

        //init boardState
        boardState = new char[BOARD_SIZE][];
        for (int i = 0; i < boardState.Length; i++)
        {
            boardState[i] = new char[BOARD_SIZE];
            for (int j = 0; j < boardState[i].Length; j++)
            {
                boardState[i][j] = Square.SQUARE_EMPTY;
            }
        }

        foreach(Square square in GetComponentsInChildren<Square>())
        {
            square.status = Square.SQUARE_EMPTY;
        }

        /* Board rendering */
        //float squareSize = squarePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        ////Render the board
        //for (int i = 0; i < BOARD_SIZE; i++)
        //{
        //    for (int j = 0; j < BOARD_SIZE; j++)
        //    {
        //        Transform squareObj = Instantiate(squarePrefab, new Vector3(startX + (i * squareSize), startY + ((j - (BOARD_SIZE - 1)) * squareSize), 0), Quaternion.identity) as Transform;
        //        squareObj.SetParent(this.transform);
        //        Square squareScript = squareObj.GetComponent<Square>();
        //        squareScript.posX = i;
        //        squareScript.posY = j;
        //    }
        //}
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
                    square.status = currentTurn;
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
        if (currentTurn == Board.TURN_X)
        {
            currentTurn = Board.TURN_O;
        }
        else //currentTurn == Board.TURN_O
        {
            currentTurn = Board.TURN_X;
        }
    }

    public void updateSquare(int x, int y, char currTurn)
    {
        boardState[x][y] = currTurn;
        pieceNumber++;
        if (currTurn == Board.TURN_X)
        {
            lastPos = new Point(x, y);
        }
        else //currTurn == Board.TURN_O
        {
            lastOPos = new Point(x, y);
        }

        switch (checkWin(currTurn, x, y))
        {
            case RESULT_X:
            {
                result = RESULT_X;
                Debug.Log("X wins");
                StartCoroutine(showGameResult());
                break;
            }
            case RESULT_O:
            {
                result = RESULT_O;
                Debug.Log("O wins");
                StartCoroutine(showGameResult());
                break;
            }
            case RESULT_DRAW:
            {
                result = RESULT_DRAW;
                Debug.Log("Draw");
                StartCoroutine(showGameResult());
                break;
            }
        }
    }

    public int checkWin(char currTurn, int lastX, int lastY)
    {
        int result = RESULT_NONE;

        if (isHor(currTurn, lastY)
            || isVer(currTurn, lastX)
            || isDiag1(currTurn)
            || isDiag2(currTurn))
        {
            result = (currTurn == Square.SQUARE_X ? RESULT_X : RESULT_O);
        }
        else if (pieceNumber == BOARD_SIZE * BOARD_SIZE)
        {
            result = RESULT_DRAW;
        }
        return result;
    }

    public bool isHor(char currTurn, int lastY)
    {
        return (boardState[0][lastY] == currTurn
            && boardState[1][lastY] == currTurn
            && boardState[2][lastY] == currTurn);
    }

    public bool isVer(char currTurn, int lastX)
    {
        return (boardState[lastX][0] == currTurn
            && boardState[lastX][1] == currTurn
            && boardState[lastX][2] == currTurn);
    }

    public bool isDiag1(char currTurn)
    {
        return (boardState[0][2] == currTurn
            && boardState[1][1] == currTurn
            && boardState[2][0] == currTurn);
    }

    public bool isDiag2(char currTurn)
    {
        return (boardState[2][2] == currTurn
            && boardState[1][1] == currTurn
            && boardState[0][0] == currTurn);
    }

    public IEnumerator showGameResult() //TODO  ?
    {
        yield return new WaitForSeconds(3);
        isStarted = false;
    }
}
