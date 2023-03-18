using UnityEngine;
using System;
using System.Collections.Generic;

/*
 * This code was modified from Simon Lucas' implementation of MCTS
 * https://web.archive.org/web/20151213012727/http://mcts.ai/code/java.html
 */
public class TreeNode
{
    static System.Random r = new System.Random();
    static double epsilon = 1e-6;

    public List<TreeNode> children;
    double nVisits, totValue;
    public double uctValue;

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
            double value = newNode.simulate(); //3. SIMULATION

            foreach (TreeNode node in visited)
            {
                node.updateStats(value); //4. BACKPROPAGATION
            }
        }
    }

    public void expand()
    {
        List<Point> childrenMoves = listPossibleMoves(state.boardState);
        //Apply one move for each expansion child
        foreach (Point move in childrenMoves)
        {
            TreeNode childNode = new TreeNode(new State(state.boardState, state.currentTurn, state.lastPos, state.lastOPos, state.pieceNumber));
            if (state.currentTurn == Board.TURN_X)
            {
                childNode.state.boardState[move.x][move.y] = Square.SQUARE_X;
                childNode.state.lastPos = new Point(move.x, move.y);
                childNode.state.pieceNumber++;
                childNode.state.stateResult = checkWin(childNode.state, Square.SQUARE_X);
                childNode.state.currentTurn = Board.TURN_O; //apply new turn
            }
            else //state.currentTurn == Board.TURN_O
            {
                childNode.state.boardState[move.x][move.y] = Square.SQUARE_O;
                childNode.state.lastOPos = new Point(move.x, move.y);
                childNode.state.pieceNumber++;
                childNode.state.stateResult = checkWin(childNode.state, Square.SQUARE_O);
                childNode.state.currentTurn = Board.TURN_X; //apply new turn
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
            //UCT value calculation
            double uctValue =
                    c.totValue / (c.nVisits + epsilon) +
                            Math.Sqrt(Math.Log(nVisits + 1) / (c.nVisits + epsilon)) +
                            r.NextDouble() * epsilon; // small random number to break ties randomly in unexpanded nodes
            c.uctValue = uctValue;
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
        State simState = new State(Util.deepcloneArray(state.boardState), state.currentTurn, state.lastPos, state.lastOPos, state.pieceNumber);
        simState.stateResult = state.stateResult;

        char simCurrentTurn = state.currentTurn;
        char simOppTurn = (state.currentTurn == Board.TURN_O ? Board.TURN_X : Board.TURN_O);

        int simValue = int.MinValue;

        //simulate semi-randomly (for both players) until a terminal result is achieved
        while (simState.stateResult == Board.RESULT_NONE)
        {
            Point chosenMove = null;
            Point candidateMove1 = checkTwoPieces(simState, simCurrentTurn); //my two pieces
            Point candidateMove2 = checkTwoPieces(simState, simOppTurn); //opponent's two pieces
            Point candidateMove3 = null;
            if ((simState.lastPos != null) && (simState.lastOPos != null))
            {
                candidateMove3 = placeAdjacent(simState, simCurrentTurn); //my last one piece
            }

            if (candidateMove1 != null) //check my "two pieces"
            {
                chosenMove = candidateMove1;
                //Debug.Log("myTwoPieces heuristic at " + chosenMove.x + "," + chosenMove.y);
            }
            else if (candidateMove2 != null) ///check opponent's "two pieces"
            {
                chosenMove = candidateMove2;
                //Debug.Log("oppTwoPieces heuristic at " + chosenMove.x + "," + chosenMove.y);
            }
            else if(candidateMove3 != null)
            {
                chosenMove = candidateMove3;
                //Debug.Log(simCurrentTurn + " onepiece heuristic at " + chosenMove.x + "," + chosenMove.y);
            }
            else
            {
                chosenMove = doRandomMove();
            }

            if (simState.boardState[chosenMove.x][chosenMove.y] == Square.SQUARE_EMPTY)
            {
                simState.boardState[chosenMove.x][chosenMove.y] = (simCurrentTurn == Board.TURN_X ? Square.SQUARE_X : Square.SQUARE_O);

                //printState(simState.boardState); //debug

                if (simCurrentTurn == Board.TURN_X)
                {
                    simState.lastPos = new Point(chosenMove.x, chosenMove.y);
                }
                else //simCurrentTurn == Board.TURN_O
                {
                    simState.lastOPos = new Point(chosenMove.x, chosenMove.y);
                }
                simState.pieceNumber++;
                simState.stateResult= checkWin(simState, simCurrentTurn); //check terminal condition
                //Debug.Log("result: " + simState.stateResult);
                simCurrentTurn = (simCurrentTurn == Board.TURN_X ? Board.TURN_O : Board.TURN_X); //switch turn
                simOppTurn = (simCurrentTurn == Board.TURN_X ? Board.TURN_O : Board.TURN_X); //switch turn
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
    public Point placeAdjacent(State state, char currentTurn)
    {
        List<Point> moveList = new List<Point>();
        Point result = null;
        Point lastPosition = currentTurn == Board.TURN_X ? state.lastPos : state.lastOPos;

        //search for possible moves adjacent to lastPosition
        for (int y = 1; y > -2; y--)
        {
            for (int x = -1; x < 2; x++)
            {
                if ((lastPosition.x + x >= 0) && (lastPosition.x + x < Board.BOARD_SIZE)
                    && (lastPosition.y + y >= 0) && (lastPosition.y + y < Board.BOARD_SIZE))
                {
                    if (state.boardState[lastPosition.x + x][lastPosition.y + y] == Square.SQUARE_EMPTY)
                    {
                        moveList.Add(new Point(lastPosition.x + x, lastPosition.y + y));
                    }
                }
            }
        }

        switch (moveList.Count)
        {
            case 0:
                {
                    result = null;
                    break;
                }
            case 1:
                {
                    result = moveList[0];
                    break;
                }
            default:
                {
                    result = moveList[r.Next(moveList.Count)];
                    break;
                }
        }

        return result;
    }

    //A heuristic to check two pieces in a row, winning a game or preventing the opponent from winning the game
    public Point checkTwoPieces(State state, char currentTurn)
    {
        List<Point> moveList = new List<Point>();
        Point result = null;

        string pattern1 = new string(new char[Board.BOARD_SIZE] { Square.SQUARE_EMPTY, currentTurn, currentTurn });
        string pattern2 = new string(new char[Board.BOARD_SIZE] { currentTurn, Square.SQUARE_EMPTY, currentTurn });
        string pattern3 = new string(new char[Board.BOARD_SIZE] { currentTurn, currentTurn, Square.SQUARE_EMPTY });

        //check horizontally
        for (int y = 0; y < Board.BOARD_SIZE; y++)
        {
            string hor = new string(new char[Board.BOARD_SIZE] {state.boardState[0][y], state.boardState[1][y], state.boardState[2][y]});
            if (hor.Equals(pattern1))
            {
                moveList.Add(new Point(0, y));
            }
            else if (hor.Equals(pattern2))
            {
                moveList.Add(new Point(1, y));
            }
            else if (hor.Equals(pattern3))
            {
                moveList.Add(new Point(2, y));
            }
        }

        //check vertical
        for (int x = 0; x < Board.BOARD_SIZE; x++)
        {
            string ver = new string(new char[Board.BOARD_SIZE] { state.boardState[x][0], state.boardState[x][1], state.boardState[x][2] });
            if (ver.Equals(pattern1))
            {
                moveList.Add(new Point(x, 0));
            }
            else if (ver.Equals(pattern2))
            {
                moveList.Add(new Point(x, 1));
            }
            else if (ver.Equals(pattern3))
            {
                moveList.Add(new Point(x, 2));
            }
        }

        //check diag1
        string diag1 = new string(new char[Board.BOARD_SIZE] { state.boardState[0][2], state.boardState[1][1], state.boardState[2][0] });
        if (diag1.Equals(pattern1))
        {
            moveList.Add(new Point(0, 2));
        }
        else if (diag1.Equals(pattern2))
        {
            moveList.Add(new Point(1, 1));
        }
        else if (diag1.Equals(pattern3))
        {
            moveList.Add(new Point(2, 0));
        }

        //check diag2
        string diag2 = new string(new char[Board.BOARD_SIZE] { state.boardState[2][2], state.boardState[1][1], state.boardState[0][0] });
        if (diag2.Equals(pattern1))
        {
            moveList.Add(new Point(2, 2));
        }
        else if (diag2.Equals(pattern2))
        {
            moveList.Add(new Point(1, 1));
        }
        else if (diag2.Equals(pattern3))
        {
            moveList.Add(new Point(0, 0));
        }

        switch (moveList.Count)
        {
            case 0:
            {
                result = null;
                break;
            }
            case 1:
            {
                result =  moveList[0];
                break;
            }
            default:
            {
                result = moveList[r.Next(moveList.Count)];
                break;
            }
        }
        return result;
    }


    public List<Point> listPossibleMoves(char[][] boardState)
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


    public int checkWin(State state, char currTurn)
    {
        int result = Board.RESULT_NONE;

        int lastX = currTurn == Board.TURN_X ? state.lastPos.x : state.lastOPos.x;
        int lastY = currTurn == Board.TURN_X ? state.lastPos.y : state.lastOPos.y;

        if (isHor(state, currTurn, lastY)
             || isVer(state, currTurn, lastX)
             || isDiag1(state, currTurn)
             || isDiag2(state, currTurn))
        {
            result = (currTurn == Square.SQUARE_X ? Board.RESULT_X : Board.RESULT_O);
        }
        else if (state.pieceNumber == Board.BOARD_SIZE * Board.BOARD_SIZE)
        {
            result = Board.RESULT_DRAW;
        }
        return result;
    }

    public bool isHor(State state, char currTurn, int lastY)
    {
        return (state.boardState[0][lastY] == currTurn
            && state.boardState[1][lastY] == currTurn
            && state.boardState[2][lastY] == currTurn);
    }

    public bool isVer(State state, char currTurn, int lastX)
    {
        return (state.boardState[lastX][0] == currTurn
            && state.boardState[lastX][1] == currTurn
            && state.boardState[lastX][2] == currTurn);
    }

    public bool isDiag1(State state, char currTurn)
    {
        return (state.boardState[0][2] == currTurn
            && state.boardState[1][1] == currTurn
            && state.boardState[2][0] == currTurn);
    }

    public bool isDiag2(State state, char currTurn)
    {
        return (state.boardState[2][2] == currTurn
            && state.boardState[1][1] == currTurn
            && state.boardState[0][0] == currTurn);
    }

    //debugging purpose
    public void printState(char[][] boardState)
    {
        String s = null;
        for (int y = boardState.Length - 1; y > -1; y--)
        {
            for (int x = 0; x < boardState.Length; x++)
            {
                s += boardState[x][y];
            }
            s += "\n";
        }
        Debug.Log(s);
    }


}