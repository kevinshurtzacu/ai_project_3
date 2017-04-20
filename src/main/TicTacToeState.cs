using System.Collections.Generic;

namespace Learn
{
    public class TicTacToeState : IState
    {
        // Grid states, turns
        public enum Move : byte { X, O, Blank };
        public enum Turn : byte { X, O };

        // Board representation
        private Move[,] board;

        // Turn
        private Turn turn;

        // Create a default board
        public TicTacToeState()
        {
            board = new Move[3, 3] {
                { Move.Blank, Move.Blank, Move.Blank },
                { Move.Blank, Move.Blank, Move.Blank },
                { Move.Blank, Move.Blank, Move.Blank }
            };

            turn = Turn.X;
        }

        // Create a board with different starting turn
        public TicTacToeState(Turn startTurn)
        {
            board = new Move[3, 3] {
                { Move.Blank, Move.Blank, Move.Blank },
                { Move.Blank, Move.Blank, Move.Blank },
                { Move.Blank, Move.Blank, Move.Blank }
            };

            turn = startTurn;
        }

        // Create a copy of src
        public TicTacToeState(TicTacToeState src)
        {
            board = new Move[3, 3];
            
            for (int rowIndex = 0; rowIndex < 3; ++rowIndex)
            {
                for (int colIndex = 0; colIndex < 3; ++colIndex)
                    board[rowIndex, colIndex] = src.board[rowIndex, colIndex];
            }
        }

        public override bool Equals(object other)
        {
            TicTacToeState otherBoard = other as TicTacToeState;

            if (otherBoard == null)
                return false;
            
            if (otherBoard == this)
                return true;
            
            // Compare the cells of both arrays
            for (int rowIndex = 0; rowIndex < 3; ++rowIndex)
            {
                for (int colIndex = 0; colIndex < 3; ++colIndex)
                {
                    // If there is a mismatch, return false
                    if (board[rowIndex, colIndex] != otherBoard.board[rowIndex, colIndex])
                        return false;
                }
            }

            // Otherwise, return true
            return true;
        }

        public override int GetHashCode()
        {
            // Only the array must be unique
            return board.GetHashCode();
        }

        public List<IState> getSuccessors()
        {
            List<IState> successors = new List<IState>();

            // Generate all viable successors
            for (int rowIndex = 0; rowIndex < 3; ++rowIndex)
            {
                for (int colIndex = 0; colIndex < 3; ++ colIndex)
                {
                    // If there is an open grid space, add it for the player
                    if (board[rowIndex, colIndex] == Move.Blank)
                    {
                        board[rowIndex, colIndex] = (Move)turn;
                        successors.Add(new TicTacToeState(this));
                        board[rowIndex, colIndex] = Move.Blank;
                    }
                }
            }

            return successors;
        }

        // GoalTest for O Player Victory
        public static Agent.GoalTest oPlayerWin = (IState state) =>
        {
            TicTacToeState boardState = state as TicTacToeState;

            if (boardState == null)
                return false;
            
            // Check rows
            if (boardState.board[0,0] == Move.O
                && boardState.board[0,1] == Move.O
                && boardState.board[0,2] == Move.O)
                return true;
            
            if (boardState.board[1,0] == Move.O
                && boardState.board[1,1] == Move.O
                && boardState.board[1,2] == Move.O)
                return true;

            if (boardState.board[2,0] == Move.O
                && boardState.board[2,1] == Move.O
                && boardState.board[2,2] == Move.O)
                return true;

            // Check columns
            if (boardState.board[0,0] == Move.O
                && boardState.board[1,0] == Move.O
                && boardState.board[2,0] == Move.O)
                return true;

            if (boardState.board[0,1] == Move.O
                && boardState.board[1,1] == Move.O
                && boardState.board[2,1] == Move.O)
                return true;

            if (boardState.board[0,2] == Move.O
                && boardState.board[1,2] == Move.O
                && boardState.board[2,2] == Move.O)
                return true;

            // Check diagonals
            if (boardState.board[0,0] == Move.O
                && boardState.board[1,1] == Move.O
                && boardState.board[2,2] == Move.O)
                return true;

            if (boardState.board[0,2] == Move.O
                && boardState.board[1,1] == Move.O
                && boardState.board[2,0] == Move.O)
                return true;

            // No matches
            return false;
        };

        // GoalTest for X Player Victory
        public static Agent.GoalTest xPlayerWin = (IState state) =>
        {
            TicTacToeState boardState = state as TicTacToeState;

            if (boardState == null)
                return false;
            
            // Check rows
            if (boardState.board[0,0] == Move.X
                && boardState.board[0,1] == Move.X
                && boardState.board[0,2] == Move.X)
                return true;
            
            if (boardState.board[1,0] == Move.X
                && boardState.board[1,1] == Move.X
                && boardState.board[1,2] == Move.X)
                return true;

            if (boardState.board[2,0] == Move.X
                && boardState.board[2,1] == Move.X
                && boardState.board[2,2] == Move.X)
                return true;

            // Check columns
            if (boardState.board[0,0] == Move.X
                && boardState.board[1,0] == Move.X
                && boardState.board[2,0] == Move.X)
                return true;

            if (boardState.board[0,1] == Move.X
                && boardState.board[1,1] == Move.X
                && boardState.board[2,1] == Move.X)
                return true;

            if (boardState.board[0,2] == Move.X
                && boardState.board[1,2] == Move.X
                && boardState.board[2,2] == Move.X)
                return true;

            // Check diagonals
            if (boardState.board[0,0] == Move.X
                && boardState.board[1,1] == Move.X
                && boardState.board[2,2] == Move.X)
                return true;

            if (boardState.board[0,2] == Move.X
                && boardState.board[1,1] == Move.X
                && boardState.board[2,0] == Move.X)
                return true;

            // No matches
            return false;
        };
    }
}