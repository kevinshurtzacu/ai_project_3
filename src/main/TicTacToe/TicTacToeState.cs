using System.Collections.Generic;
using System;

namespace Learn.TicTacToe
{
    public class TicTacToeState : IState
    {
        // Grid states
        public enum Move : byte { X, O, Blank };

        // Game events
        public static event Action<object, EventArgs> PlayerWinsX;
        public static event Action<object, EventArgs> PlayerWinsO;
        public static event Action<object, EventArgs> CatsGame;

        // Board representation
        private Move[,] board;

        // Create a default board
        public TicTacToeState() { resetBoard(); }

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

        // Check for X victory, return next moves
        public List<IState> GoalTestX()
        {
            List<IState> successors = getSuccessors(Move.X);
            bool gameOver = false;

            // Player X wins
            if (playerWin(Move.X))
            {
                PlayerWinsX?.Invoke(this, EventArgs.Empty);
                gameOver = true;
            }

            // Player O wins
            if (playerWin(Move.O))
            {
                PlayerWinsO?.Invoke(this, EventArgs.Empty);
                gameOver = true;
            }

            // No viable options remain
            if (successors.Count == 0)
            {
                CatsGame?.Invoke(this, EventArgs.Empty);
                gameOver = true;
            }
            
            // If game over, start over
            if (gameOver)
                return new TicTacToeState().getSuccessors(Move.X);
            
            // Goal not met, but options remain
            return successors;
        }
        
        // Check for O victory, return next moves
        public List<IState> GoalTestO()
        {
            List<IState> successors = getSuccessors(Move.O);
            bool gameOver = false;

            // Player X wins
            if (playerWin(Move.X))
            {
                PlayerWinsX?.Invoke(this, EventArgs.Empty);
                gameOver = true;
            }

            // Player O wins
            if (playerWin(Move.O))
            {
                PlayerWinsO?.Invoke(this, EventArgs.Empty);
                gameOver = true;
            }
            
            // No viable options remain
            if (successors.Count == 0)
            {
                CatsGame?.Invoke(this, EventArgs.Empty);
                gameOver = true;
            }

            // If game over, start over
            if (gameOver)
                return new TicTacToeState().getSuccessors(Move.O);

            // Goal not met, but options remain
            return successors;
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
            int finalHash = 0x0;
            
            /* Breaks every space on the board into two-bit partitions of an integer value, thereby
             * representing the board with the first 18 bits of the 32-bit integer.
             * 
             * The function below identifies the proper bit-offset for each space's 2-bit partition, 
             * and then applies bit configurations 0 (X), 1 (O), or 2 (Blank) to the 2-bit partition
             * starting at the offset.
             *
             * This ensures a unique bit-string for every permutation in the board, theoretically
             * allowing for a maximum of 19,683 (3^9) possible permutations thereof.
             */
            for (int rowIndex = 0; rowIndex < 3; ++rowIndex)
            {
                for (int colIndex = 0; colIndex < 3; ++colIndex)
                {
                    // Represents the low-order bit of the correct bit-partition
                    int offset = (int)(Math.Pow(2, ((rowIndex * 3) + colIndex) * 2));

                    switch (board[rowIndex, colIndex])
                    {
                        // Assign bit-partition 0b00 (0)
                        case Move.X:
                            // This position should be marked 0b00, which it already is.
                            break;
                        
                        // Assign bit-partition 0b01 (1)
                        case Move.O:
                            finalHash |= offset;
                            break;

                        // Assign bit-partitino 0b10 (2)
                        case Move.Blank:
                            finalHash |= (offset * 2);
                            break;
                    }
                }
            }

            return finalHash;
        }

        public override string ToString()
        {
            String output = "";

            for (int rowIndex = 0; rowIndex < 3; ++rowIndex)
                output += $"[{board[rowIndex, 0]}]\t[{board[rowIndex, 1]}]\t[{board[rowIndex, 2]}]\n";
            
            return output;
        }

        // Reset the game board
        private void resetBoard()
        {
            board = new Move[3, 3] {
                { Move.Blank, Move.Blank, Move.Blank },
                { Move.Blank, Move.Blank, Move.Blank },
                { Move.Blank, Move.Blank, Move.Blank }
            };
        }

        // Check for player victory
        private bool playerWin(Move move)
        {
            // Check rows
            if (board[0,0] == move
                && board[0,1] == move
                && board[0,2] == move)
                return true;
            
            if (board[1,0] == move
                && board[1,1] == move
                && board[1,2] == move)
                return true;

            if (board[2,0] == move
                && board[2,1] == move
                && board[2,2] == move)
                return true;

            // Check columns
            if (board[0,0] == move
                && board[1,0] == move
                && board[2,0] == move)
                return true;

            if (board[0,1] == move
                && board[1,1] == move
                && board[2,1] == move)
                return true;

            if (board[0,2] == move
                && board[1,2] == move
                && board[2,2] == move)
                return true;

            // Check diagonals
            if (board[0,0] == move
                && board[1,1] == move
                && board[2,2] == move)
                return true;

            if (board[0,2] == move
                && board[1,1] == move
                && board[2,0] == move)
                return true;

            // No matches
            return false;
        }

        // Generate successors
        private List<IState> getSuccessors(Move move)
        {
            List<IState> successors = new List<IState>();

            // Generate all viable successors
            for (int rowIndex = 0; rowIndex < 3; ++rowIndex)
            {
                for (int colIndex = 0; colIndex < 3; ++colIndex)
                {
                    // If there is an open grid space, add it for the player
                    if (board[rowIndex, colIndex] == Move.Blank)
                    {
                        board[rowIndex, colIndex] = move;
                        successors.Add(new TicTacToeState(this));
                        board[rowIndex, colIndex] = Move.Blank;
                    }
                }
            }

            return successors;
        }
    }
}