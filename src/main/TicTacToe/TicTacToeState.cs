using System.Collections.Generic;
using System.Runtime.Serialization;
using System;

namespace Learn.TicTacToe
{
    [DataContract]
    [KnownType(typeof(Move[][]))]
    public class TicTacToeState : IState
    {
        // Grid states
        [DataContract]
        public enum Move : byte { [EnumMember] X, [EnumMember] O, [EnumMember] Blank };

        // Game events
        public static event Action<object, EventArgs> PlayerWinsX;
        public static event Action<object, EventArgs> PlayerWinsO;
        public static event Action<object, EventArgs> CatsGame;

        // Board representation
        [DataMember]
        private Move[][] board;

        // Create a default board
        public TicTacToeState() { resetBoard(); }

        // Create a copy of src
        public TicTacToeState(TicTacToeState src)
        {
            board = new Move[3][] { new Move[3], new Move[3], new Move[3] };
            
            for (int rowIndex = 0; rowIndex < 3; ++rowIndex)
            {
                for (int colIndex = 0; colIndex < 3; ++colIndex)
                    board[rowIndex][colIndex] = src.board[rowIndex][colIndex];
            }
        }

        public List<IState> SuccessorsX
        {
            get
            {
                List<IState> successors = getSuccessors(Move.X);

                if (successors.Count == 0)
                {
                    // No viable options remain
                    CatsGame?.Invoke(this, EventArgs.Empty);
                    return new TicTacToeState().SuccessorsX;
                }

                return successors;
            }
        }

        public List<IState> SuccessorsO
        {
            get
            {
                List<IState> successors = getSuccessors(Move.O);

                if (successors.Count == 0)
                {
                    // No viable options remain
                    CatsGame?.Invoke(this, EventArgs.Empty);
                    return new TicTacToeState().SuccessorsO;
                }

                return successors;
            }
        }

        // Check for end game
        public void GoalTest()
        {
            // Player X wins
            if (playerWin(Move.X))
                PlayerWinsX?.Invoke(this, EventArgs.Empty);

            // Player O wins
            if (playerWin(Move.O))
                PlayerWinsO?.Invoke(this, EventArgs.Empty);
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
                    if (board[rowIndex][colIndex] != otherBoard.board[rowIndex][colIndex])
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

                    switch (board[rowIndex][colIndex])
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
                output += $"[{board[rowIndex][0]}]\t[{board[rowIndex][1]}]\t[{board[rowIndex][2]}]\n";
            
            return output;
        }

        public static void WireEvents(TicTacToeState tttState, Agent playerOne, Agent playerTwo)
        {
            // Wire up player behaviors to state events
            TicTacToeState.PlayerWinsX += playerOne.Victory;
            TicTacToeState.PlayerWinsX += playerTwo.Defeat;
            TicTacToeState.PlayerWinsO += playerOne.Defeat;
            TicTacToeState.PlayerWinsO += playerTwo.Victory;
            TicTacToeState.CatsGame    += playerOne.Draw;
            TicTacToeState.CatsGame    += playerTwo.Draw;
        }
        
        public static void TrainZeroSum(long practiceGames, bool showOutput, params Agent[] agents)
        {
            // Create starting state
            IState state = new TicTacToeState();
            TicTacToeState tttState = state as TicTacToeState;
            
            // Put agents in training mode
            foreach (Agent agent in agents)
                agent.TrainingMode(1.0);

            // Use repeated wins as a benchmark
            bool enoughTraining = false;
            long games = 0;
            int ticks = 0;
            bool dotPrinted = false;

            // Reset dotPrinted after it has moved on
            Action<object, EventArgs> resetDotPrinted = (object sender, EventArgs e) => dotPrinted = false;
            
            TicTacToeState.PlayerWinsX += resetDotPrinted;
            TicTacToeState.PlayerWinsO += resetDotPrinted;
            TicTacToeState.CatsGame    += resetDotPrinted;

            // Begin training progress bar
            if (showOutput)
                Console.Write("[ ");

            // Watch each agent evolve
            while (!enoughTraining)
            {
                // Train the first agent
                tttState = agents[0].Act(tttState.SuccessorsX) as TicTacToeState;
                tttState.GoalTest();

                games = agents[0].Victories + agents[1].Victories + agents[0].Draws;
                
                if (games % (practiceGames / 100) == 0 && !dotPrinted)
                {
                    // Update competitiveness at quarters
                    if (ticks % 25 == 0)
                    {
                        // Display quarter in progress bar
                        if (showOutput)
                            Console.Write($"[{ticks}]");

                        // Make each agent more competitive
                        foreach (Agent agent in agents)
                            agent.TrainingMode((100 - ticks) / 100);
                    }

                    // Update ticks
                    ticks += 1;

                    // Don't include hundredth tick
                    if (ticks < 100 && showOutput)
                        Console.Write(".");
                    
                    dotPrinted = true;
                }

                // Check if total number of practices have been met
                if (games > practiceGames)
                    enoughTraining = true;
                
                // Train the second agent, assuming they are not sufficiently trained
                if (!enoughTraining)
                {
                    tttState = agents[1].Act(tttState.SuccessorsO) as TicTacToeState;
                    tttState.GoalTest();

                    games = agents[0].Victories + agents[1].Victories + agents[0].Draws;
                    
                    if (games % (practiceGames / 100) == 0 && !dotPrinted)
                    {
                        // Update competitiveness at quarters
                        if (ticks % 25 == 0)
                        {
                            // Display quarter in progress bar
                            if (showOutput)
                                Console.Write($"[{ticks}]");

                            // Make each agent more competitive
                            foreach (Agent agent in agents)
                                agent.TrainingMode((100 - ticks) / 100);
                        }

                        // Update ticks
                        ticks += 1;

                        // Don't include hundredth tick
                        if (ticks < 100 && showOutput)
                            Console.Write(".");
                        
                        dotPrinted = true;
                    }

                    // Check if total number of practices have been met
                    if (games > practiceGames)
                        enoughTraining = true;
                }
            }
            
            // End progress bar
            if (showOutput)
            {
                Console.WriteLine(" ]");
                Console.WriteLine("\n");
            }

            // Put agents in competitive mode
            foreach (Agent agent in agents)
                agent.CompeteMode();
        }

        public static void PrintOptions(List<IState> successors)
        {
            for (int successorIndex = 0; successorIndex < successors.Count; ++successorIndex)
            {
                Console.WriteLine($"--- Option {successorIndex + 1} [{successorIndex + 1}] ---", successorIndex);
                Console.WriteLine(successors[successorIndex]);
            }
        }

        // Reset the game board
        private void resetBoard()
        {
            board = new Move[3][]
            {
                new Move[3] { Move.Blank, Move.Blank, Move.Blank },
                new Move[3] { Move.Blank, Move.Blank, Move.Blank },
                new Move[3] { Move.Blank, Move.Blank, Move.Blank }
            };
        }

        // Check for player victory
        private bool playerWin(Move move)
        {
            // Check rows
            if (board[0][0] == move
                && board[0][1] == move
                && board[0][2] == move)
                return true;
            
            if (board[1][0] == move
                && board[1][1] == move
                && board[1][2] == move)
                return true;

            if (board[2][0] == move
                && board[2][1] == move
                && board[2][2] == move)
                return true;

            // Check columns
            if (board[0][0] == move
                && board[1][0] == move
                && board[2][0] == move)
                return true;

            if (board[0][1] == move
                && board[1][1] == move
                && board[2][1] == move)
                return true;

            if (board[0][2] == move
                && board[1][2] == move
                && board[2][2] == move)
                return true;

            // Check diagonals
            if (board[0][0] == move
                && board[1][1] == move
                && board[2][2] == move)
                return true;

            if (board[0][2] == move
                && board[1][1] == move
                && board[2][0] == move)
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
                    if (board[rowIndex][colIndex] == Move.Blank)
                    {
                        board[rowIndex][colIndex] = move;
                        successors.Add(new TicTacToeState(this));
                        board[rowIndex][colIndex] = Move.Blank;
                    }
                }
            }

            return successors;
        }
    }
}