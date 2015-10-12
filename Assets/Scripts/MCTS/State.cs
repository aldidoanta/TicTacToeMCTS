using System;
using System.Collections.Generic;

public class State
{
    public char[][] boardState;
    public char currentTurn;
    public Point lastPos, lastOPos;
    public int stateResult;
    public int pieceNumber; //number of pieces placed on the board -- removes the need to count manually

    public State(char[][] prevBoardState, char currentTurn, Point lastPos, Point lastOPos, int pieceNumber)
    {
        this.currentTurn = currentTurn;
        this.lastPos = lastPos;
        this.lastOPos = lastOPos;
        stateResult = Board.RESULT_NONE;
        this.pieceNumber = pieceNumber;

        if (prevBoardState != null) //no previous moves
        {
            boardState = Util.deepcloneArray(prevBoardState);
        }
        else
        {
            boardState = new char[Board.BOARD_SIZE][];
            for (int i = 0; i < boardState.Length; i++)
            {
                boardState[i] = new char[Board.BOARD_SIZE];
                for (int j = 0; j < boardState[i].Length; j++)
                {
                    boardState[i][j] = Square.SQUARE_EMPTY;
                }
            }
        }
    }
}
