using UnityEngine;
using System;
using System.Collections.Generic;

public class TreeNode
{
    static System.Random r = new System.Random();
    static double epsilon = 1e-6;

    public List<TreeNode> children;
    double nVisits, totValue;

    public State state;

    public TreeNode(State state)
    {
        children = new List<TreeNode>();
        nVisits = 0;
        totValue = 0;

        this.state = state;
    }

    public void iterateMCTS()
    {
        LinkedList<TreeNode> visited = new LinkedList<TreeNode>();
        TreeNode cur = this;
        visited.AddLast(this);
        while (!cur.isLeaf()) //1. SELECTION
        {
            cur = cur.select();

            visited.AddLast(cur);
        }
        if (cur.state.stateResult == Board.RESULT_NONE)
        {
            cur.expand(); //2. EXPANSION
            TreeNode newNode = cur.select();
            visited.AddLast(newNode);
            double value = newNode.simulate(); //3. SIMULATION //TODO heuristic policy?

            foreach (TreeNode node in visited)
            {
                node.updateStats(value); //4. BACKPROPAGATION
            }
        }
    }

    public void expand()
    {
        List<Point> childrenMoves = listPossibleMoves(state.boardState, state.lastPos);

        //Apply one move for each expansion child
        foreach (Point move in childrenMoves)
        {
            TreeNode childNode = new TreeNode(new State(state.boardState, state.lastPos, state.pieceNumber));
            if (state.lastPos == null || childNode.state.boardState[state.lastPos.x][state.lastPos.y] == Square.SQUARE_O)
            {
                childNode.state.boardState[move.x][move.y] = Square.SQUARE_X;
                childNode.state.lastPos = new Point(move.x, move.y);
                childNode.state.pieceNumber++;
                childNode.state.stateResult = checkWin(childNode.state, Square.SQUARE_X);
            }
            else
            {
                childNode.state.boardState[move.x][move.y] = Square.SQUARE_O;
                childNode.state.lastPos = new Point(move.x, move.y);
                childNode.state.pieceNumber++;
                childNode.state.stateResult = checkWin(childNode.state, Square.SQUARE_O);
            }

            children.Add(childNode);
        }
    }

    public TreeNode select()
    {
        TreeNode selected = null;
        double bestValue = Double.MinValue;
        foreach (TreeNode c in children)
        {
            double uctValue =
                    c.totValue / (c.nVisits + epsilon) +
                            Math.Sqrt(Math.Log(nVisits + 1) / (c.nVisits + epsilon)) +
                            r.NextDouble() * epsilon; // small random number to break ties randomly in unexpanded nodes
            if (uctValue > bestValue)
            {
                selected = c;
                bestValue = uctValue;
            }
        }
        return selected;
    }

    public bool isLeaf()
    {
        return children.Count == 0;
    }

    public double simulate()
    {
        State simState = new State(Util.deepcloneArray(state.boardState), state.lastPos, state.pieceNumber);
        simState.stateResult = state.stateResult;

        int simCurrentTurn = (state.boardState[state.lastPos.x][state.lastPos.y] == Square.SQUARE_O ? Board.TURN_X : Board.TURN_O);
        int simOppTurn = (state.boardState[state.lastPos.x][state.lastPos.y] == Square.SQUARE_O ? Board.TURN_O : Board.TURN_X);

        //printState(simBoardState);

        int simValue = int.MinValue;

        //simulate semi-randomly (for both players) until a terminal result is achieved
        while (simState.stateResult == Board.RESULT_NONE)
        {
            Point chosenMove = null;
            Point candidateMove1 = checkTwoPieces(simState, simCurrentTurn);
            Point candidateMove2 = checkTwoPieces(simState, simOppTurn);

            if (candidateMove1 != null) //check my "two pieces"
            {
                chosenMove = candidateMove1;
            }
            else if (candidateMove2 != null) //check opponent's "two pieces"
            {
                chosenMove = candidateMove2;
            }
            else
            {
                chosenMove = doRandomMove();
            }

            if (simState.boardState[chosenMove.x][chosenMove.y] == Square.SQUARE_EMPTY)
            {

                simState.boardState[chosenMove.x][chosenMove.y] = (simCurrentTurn == Board.TURN_X ? Square.SQUARE_X : Square.SQUARE_O);
                //printState(simBoardState);
                simState.lastPos = new Point(chosenMove.x, chosenMove.y);
                simState.pieceNumber++;
                simState.stateResult= checkWin(simState, simCurrentTurn); //check terminal condition
                simCurrentTurn = 3 - simCurrentTurn; //switch turn
                simOppTurn = 3 - simOppTurn; //switch turn
            }
        }

        switch (simState.stateResult)
        {
            case Board.RESULT_DRAW:
            {
                simValue = 0;
                break;
            }
            case Board.RESULT_X:
            {
                simValue = MCTSAI.myTurn == Board.TURN_X ? 1 : -1; //1 means victory, -1 means defeat
                break;
            }
            case Board.RESULT_O:
            {
                simValue = MCTSAI.myTurn == Board.TURN_O ? 1 : -1;
                break;
            }
            default:
            {
                Debug.LogError("illegal simStateResult value");
                break;
            }
        }
        return simValue;
    }

    public void updateStats(double value)
    {
        nVisits++;
        totValue += value;
    }

    //A heuristic to check one piece in a row, placing one piece adjacent to previous own piece
    public Point placeAdjacent(int[][] boardState, int currentTurn, Point lastPos)
    {
        List<Point> moveList = new List<Point>();

        switch (moveList.Count)
        {
            case 0:
                {
                    return null;
                }
            case 1:
                {
                    return moveList[0];
                }
            default:
                {
                    return moveList[r.Next(moveList.Count)];
                }
        }
    }

    //A heuristic to check two pieces in a row, winning a game or preventing the opponent from winning the game
    public Point checkTwoPieces(State state, int currentTurn)
    {
        List<Point> moveList = new List<Point>();

        //check horizontally
        for (int y = 0; y < Board.BOARD_SIZE; y++)
        {
            if (checkHor(state, currentTurn, 1, y) == Board.INROW - 1)
            {
                Point p = getFirstEmptyPos(state.boardState, 0, y, 1, 0);
                if (p != null)
                {
                    moveList.Add(p);
                }
            }
        }

        //check vertical
        for (int x = 0; x < Board.BOARD_SIZE; x++)
        {
            if (checkVer(state, currentTurn, x, 1) == Board.INROW - 1)
            {
                Point p = getFirstEmptyPos(state.boardState, x, Board.BOARD_SIZE - 1, 0, -1);
                if (p != null)
                {
                    moveList.Add(p);
                }
            }
        }

        //check diag1
        if (checkDiag1(state, currentTurn, 1, 1) == Board.INROW - 1)
        {
            Point p = getFirstEmptyPos(state.boardState, 0, Board.BOARD_SIZE - 1, 1, -1);
            if (p != null)
            {
                moveList.Add(p);
            }
        }

        //check diag2
        if (checkDiag2(state, currentTurn, 1, 1) == Board.INROW - 1)
        {
            Point p = getFirstEmptyPos(state.boardState, Board.BOARD_SIZE - 1, Board.BOARD_SIZE - 1, -1, -1);
            if (p != null)
            {
                moveList.Add(p);
            }
        }

        switch (moveList.Count)
        {
            case 0:
            {
                return null;
            }
            case 1:
            {
                return moveList[0];
            }
            default:
            {
                return moveList[r.Next(moveList.Count)];
            }
        }
    }

    public Point getFirstEmptyPos(int[][] boardState, int initX, int initY, int offsetX, int offsetY)
    {
        Point result = null;
        int currX = initX;
        int currY = initY;
        bool found = false;

        while ((currX >= 0) && (currX < Board.BOARD_SIZE) && (currY >= 0) && (currY < Board.BOARD_SIZE) && (!found))
        {
            if (boardState[currX][currY] == Square.SQUARE_EMPTY)
            {
                found = true;
            }
            else
            {
                currX += offsetX;
                currY += offsetY;
            }
        }
        if (found)
        {
            result = new Point(currX, currY);
        }

        return result;
    }

    public List<Point> listPossibleMoves(int[][] boardState, Point lastPos)
    {   
        List<Point> possibleMoves = new List<Point>();
        //list all 9 possible moves
        for (int i = 0; i < Board.BOARD_SIZE; i++)
        {
            for (int j = 0; j < Board.BOARD_SIZE; j++)
            {
                //check for legal board coordinate
                if (boardState[i][j] == Square.SQUARE_EMPTY)
                {
                    possibleMoves.Add(new Point(i, j));
                }
            }
        }
        return possibleMoves;
    }

    public Point doRandomMove()
    {
        return new Point(r.Next(Board.BOARD_SIZE), r.Next(Board.BOARD_SIZE));
    }


    public int checkWin(State state, int currTurn)
    {
        int result = Board.RESULT_NONE;

        if (checkHor(state, currTurn, state.lastPos.x, state.lastPos.y) >= Board.INROW
            || checkVer(state, currTurn, state.lastPos.x, state.lastPos.y) >= Board.INROW
            || checkDiag1(state, currTurn, state.lastPos.x, state.lastPos.y) >= Board.INROW
            || checkDiag2(state, currTurn, state.lastPos.x, state.lastPos.y) >= Board.INROW)
        {
            result = (currTurn == Square.SQUARE_X ? Board.RESULT_X : Board.RESULT_O);
        }
        else if (state.pieceNumber == Board.BOARD_SIZE * Board.BOARD_SIZE)
        {
            result = Board.RESULT_DRAW;
        }
        return result;
    }

    public int checkHor(State state, int currTurn, int lastPosX, int lastPosY)
    {
        return countRow(state, currTurn, -1, 0) + countRow(state, currTurn, 1, 0) - 1; //left + right
    }

    public int checkVer(State state, int currTurn, int lastPosX, int lastPosY)
    {
        return countRow(state, currTurn, 0, 1) + countRow(state, currTurn, 0, -1) - 1; //up + down;
    }

    public int checkDiag1(State state, int currTurn, int lastPosX, int lastPosY)
    {
        return countRow(state, currTurn, -1, 1) + countRow(state, currTurn, 1, -1) - 1; //up-left + down-right;
    }

    public int checkDiag2(State state, int currTurn, int lastPosX, int lastPosY)
    {
        return countRow(state, currTurn, 1, 1) + countRow(state, currTurn, -1, -1) - 1; //up-right + down-left;
    }

    //counts the number of pieces in a row, given offsetX and offsetY as the counting direction
    public int countRow(State state, int currTurn, int offsetX, int offsetY)
    {
        int result = 0;
        int currX = state.lastPos.x;
        int currY = state.lastPos.x;
        bool rowEnded = false;

        while ((currX >= 0) && (currX < Board.BOARD_SIZE) && (currY >= 0) && (currY < Board.BOARD_SIZE) && (!rowEnded))
        {
            if (state.boardState[currX][currY] == currTurn)
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

    //debugging purpose
    public void printState(int[][] boardState)
    {
        String s = null;
        for (int i = 0; i < boardState.Length; i++)
        {
            for (int j = 0; j < boardState[i].Length; j++)
            {
                s += boardState[i][j];
            }
            s += "\n";
        }
        Debug.Log(s);
        //Debug.Log("finished printState()");
    }


}