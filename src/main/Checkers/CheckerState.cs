using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System;

namespace Learn.Checkers
{
    [DataContract]
    public class CheckerState : IState
    {
        // Grid states
        [DataContract]
        public enum Piece : byte
        {
            [EnumMember] White,
            [EnumMember] Black,
            [EnumMember] WhiteKing,
            [EnumMember] BlackKing,
            [EnumMember] Blank
        }

        // Current move
        [DataContract]
        public enum Move : byte { [EnumMember] White, [EnumMember] Black }

        // Endgame events
        public static event Action<object, EventArgs> WhiteWins;
        public static event Action<object, EventArgs> BlackWins;

        // Board representation
        [DataMember]
        private Piece[][] board;

        // Default constructor
        public CheckerState() { resetBoard(); }

        // Copy constructor
        public CheckerState(CheckerState src)
        {
            board = new Piece[8][];

            for (int rowIndex = 0; rowIndex < 8; ++rowIndex)
            {
                // Create a new row
                board[rowIndex] = new Piece[8];

                // Copy src members into new row
                for (int colIndex = 0; colIndex < 8; ++colIndex)
                    board[rowIndex][colIndex] = src.board[rowIndex][colIndex];
            }
        }

        // Successors for white
        public List<IState> SuccessorsWhite
        {
            get
            {
                List<IState> successors = getSuccessors(Move.White);

                if (successors.Count == 0)
                {
                    // White cannot move, so it must forfeit
                    BlackWins(this, EventArgs.Empty);
                    
                    // Black always has the first move
                    return new CheckerState().SuccessorsBlack;
                }

                return successors;
            }
        }

        // Successors for black
        public List<IState> SuccessorsBlack
        {
            get
            {
                List<IState> successors = getSuccessors(Move.Black);

                // Black always has the first move
                if (successors.Count == 0)
                {
                    // Black cannot win, so it must forfeit
                    WhiteWins(this, EventArgs.Empty);

                    // Black always has the first move
                    return new CheckerState().SuccessorsBlack;
                }

                return successors;
            }
        }

        // Goal Test
        public void GoalTest()
        {
            bool whitesRemain = false;
            bool blacksRemain = false;

            // Search each cell
            foreach (Piece[] row in board)
            {
                foreach (Piece cell in row)
                {
                    // Check if whites and blacks remain
                    if (cell == Piece.White || cell == Piece.WhiteKing)
                        whitesRemain = true;
                    else if (cell == Piece.Black || cell == Piece.BlackKing)
                        blacksRemain = true;
                    
                    // If both remain, there is no need to search further
                    if (whitesRemain && blacksRemain)
                        return;
                }
            }

            // If no whites remain, black wins
            if (!whitesRemain)
                BlackWins(this, EventArgs.Empty);
            
            // If no blacks remain, white wins
            if (!blacksRemain)
                WhiteWins(this, EventArgs.Empty);
        }

        // Check for equality
        public override bool Equals(object other)
        {
            CheckerState otherCheckerState = other as CheckerState;

            if (other == null)
                return false;
            
            if (other == this)
                return true;
            
            // If there are any points of difference, return false
            for (int rowIndex = 0; rowIndex < board.Length; ++rowIndex)
            {
                for (int colIndex = 0; colIndex < board[rowIndex].Length; ++colIndex)
                {
                    if (board[rowIndex][colIndex] != otherCheckerState.board[rowIndex][colIndex])
                        return false;
                }
            }

            // Otherwise, return true
            return true;
        }

        /* Generate hash
         *
         * Because there is not a technique for creating a strictly unique hash with a 32 bit integer
         * with a bit less than 5^32 possible board states, I have opted for a more traditional
         * hashing algorithm.  The following is based on the hashing solution found in 
         * Josh Bloch's "Effective Java".
         * 
         * Its implementation is described by StackOverflow user Jon Skeet at the following address:
         * http://stackoverflow.com/questions/263400/what-is-the-best-algorithm-for-an-overridden-system-object-gethashcode
         */
        public override int GetHashCode()
        {
            // Arbitrarily chosen primes
            const int primeOne = 486187739;
            const int primeTwo = 715225739;
            
            // Allow overflow
            unchecked
            {
                // Initialize hash to a large prime
                int hash = primeOne;

                // Apply the hashing procedure
                foreach (Piece[] row in board)
                    foreach (Piece cell in row)
                        hash = (hash * primeTwo) + (byte)cell;

                // Return new hash
                return hash;
            }
        }

        /* Print a representation of the board
         * 
         * B : BlackKing
         * b : Black
         * W : WhiteKing
         * w : White
         * - : Blank
         */
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            byte whiteKings = 0;
            byte whites = 0;
            byte blackKings = 0;
            byte blacks = 0;

            // Use sb to build a representation of each cell
            foreach (Piece[] row in board)
            {
                foreach (Piece cell in row)
                {
                    // Mark each cell with the proper Piece
                    switch (cell)
                    {
                        case Piece.BlackKing:
                            sb.Append("[ B ]");
                            blackKings += 1;
                            break;
                        
                        case Piece.Black:
                            sb.Append("[ b ]");
                            blacks += 1;
                            break;
                        
                        case Piece.WhiteKing:
                            sb.Append("[ W ]");
                            whiteKings += 1;
                            break;
                        
                        case Piece.White:
                            sb.Append("[ w ]");
                            whites += 1;
                            break;
                        
                        case Piece.Blank:
                            sb.Append("[ - ]");
                            break;
                        
                        default:
                            sb.Append("[ ? ]");
                            break;
                    }
                }

                // Add newline
                sb.AppendLine();
            }

            return String.Format($"              (Black Side)\n" +
                                 $"{sb.ToString()}" +
                                 $"              (White Side)\n\n" +
                                 $"W: {whites}\tWK: {whiteKings}\t" +
                                 $"B: {blacks}\tBK: {blackKings}\n");
        }

        // Connect playerOne and playerTwo the the event signals from CheckerState
        public static void WireEvents(CheckerState checkerState, Agent playerOne, Agent playerTwo)
        {
            // Wire up player behaviors to state events
            CheckerState.WhiteWins += playerOne.Victory;
            CheckerState.WhiteWins += playerTwo.Defeat;
            CheckerState.BlackWins += playerOne.Defeat;
            CheckerState.BlackWins += playerTwo.Victory;
        }
        
        // Train two agents to compete in an adversarial game
        public static void TrainZeroSum(long practiceGames, bool showOutput, Agent agentOne, Agent agentTwo)
        {
            // Report the number of practice games
            if (showOutput)
                Console.WriteLine($"Number of Practice Games: {practiceGames}");
   
            // Create starting state
            IState state = new CheckerState();
            CheckerState checkerState = state as CheckerState;
            
            // Put agents in training mode
            agentOne.TrainingMode(1.0);
            agentTwo.TrainingMode(1.0);

            // Use repeated wins as a benchmark
            bool enoughTraining = false;
            long games = 0;
            int ticks = 0;
            bool dotPrinted = false;

            // Reset dotPrinted after it has moved on
            Action<object, EventArgs> resetDotPrinted = (object sender, EventArgs e) => dotPrinted = false;
            
            CheckerState.WhiteWins += resetDotPrinted;
            CheckerState.BlackWins += resetDotPrinted;

            // Begin training progress bar
            if (showOutput)
                Console.Write("[ ");

            // Watch each agent evolve
            while (!enoughTraining)
            {
                // Train the first agent
                checkerState = agentOne.Act(checkerState.SuccessorsBlack) as CheckerState;
                checkerState.GoalTest();

                games = agentOne.Victories + agentTwo.Victories + agentOne.Draws;
                
                // Account for every 1% of progress
                if (games % (practiceGames / 100) == 0 && !dotPrinted)
                {
                    // Update competitiveness at quarters
                    if (ticks % 25 == 0)
                    {
                        // Display quarter in progress bar
                        if (showOutput)
                            Console.Write($"[{ticks}]");

                        // Make each agent more competitive
                        agentOne.TrainingMode((100.0 - ticks) / 100.0);
                        agentTwo.TrainingMode((100.0 - ticks) / 100.0);
                    }

                    // Update ticks
                    ticks += 1;

                    // Don't include hundredth tick
                    if (ticks < 100 && showOutput)
                        Console.Write(".");
                    
                    dotPrinted = true;
                }

                // Check if total number of practices have been met
                if (games >= practiceGames)
                    enoughTraining = true;
                
                // Train the second agent, assuming they are not sufficiently trained
                if (!enoughTraining)
                {
                    checkerState = agentTwo.Act(checkerState.SuccessorsWhite) as CheckerState;
                    checkerState.GoalTest();

                    games = agentOne.Victories + agentTwo.Victories + agentOne.Draws;
                    
                    // Account for every 1% of progress
                    if (games % (practiceGames / 100) == 0 && !dotPrinted)
                    {
                        // Update competitiveness at quarters
                        if (ticks % 25 == 0)
                        {
                            // Display quarter in progress bar
                            if (showOutput)
                                Console.Write($"[{ticks}]");

                            // Make each agent more competitive
                            agentOne.TrainingMode((100.0 - ticks) / 100.0);
                            agentTwo.TrainingMode((100.0 - ticks) / 100.0);
                        }

                        // Update ticks
                        ticks += 1;

                        // Don't include hundredth tick
                        if (ticks < 100 && showOutput)
                            Console.Write(".");
                        
                        dotPrinted = true;
                    }

                    // Check if total number of practices have been met
                    if (games >= practiceGames)
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
            agentOne.CompeteMode();
            agentTwo.CompeteMode();
        }

        // Print a series of successors using their default ToString methods
        public static void PrintOptions(List<IState> successors)
        {
            for (int successorIndex = 0; successorIndex < successors.Count; ++successorIndex)
            {
                Console.WriteLine($"--- Option {successorIndex + 1} [{successorIndex + 1}] ---", successorIndex);
                Console.WriteLine(successors[successorIndex]);
            }
        }

        // Reset game board to starting state
        private void resetBoard()
        {
            board = new Piece[8][];

            // Set Blacks
            board[0] = new Piece[] { Piece.Blank, Piece.Black, Piece.Blank, Piece.Black,
                                     Piece.Blank, Piece.Black, Piece.Blank, Piece.Black };

            board[1] = new Piece[] { Piece.Black, Piece.Blank, Piece.Black, Piece.Blank,
                                     Piece.Black, Piece.Blank, Piece.Black, Piece.Blank };
            
            board[2] = new Piece[] { Piece.Blank, Piece.Black, Piece.Blank, Piece.Black,
                                     Piece.Blank, Piece.Black, Piece.Blank, Piece.Black };

            // Set middle
            for (int rowIndex = 3; rowIndex < 5; ++rowIndex)
            {
                board[rowIndex] = new Piece[8] { Piece.Blank, Piece.Blank, Piece.Blank, Piece.Blank,
                                                 Piece.Blank, Piece.Blank, Piece.Blank, Piece.Blank };
            }

            // Set White
            board[5] = new Piece[] { Piece.White, Piece.Blank, Piece.White, Piece.Blank,
                                     Piece.White, Piece.Blank, Piece.White, Piece.Blank };

            board[6] = new Piece[] { Piece.Blank, Piece.White, Piece.Blank, Piece.White, 
                                     Piece.Blank, Piece.White, Piece.Blank, Piece.White };

            board[7] = new Piece[] { Piece.White, Piece.Blank, Piece.White, Piece.Blank,
                                     Piece.White, Piece.Blank, Piece.White, Piece.Blank };
        }

        // Get successors, given a white or black move
        private List<IState> getSuccessors(Move move)
        {
            // List of successor states
            List<IState> successorsNormal = new List<IState>();
            List<IState> successorsJump = new List<IState>();
            
            // Apply behavior to each piece on the board
            for (int rowIndex = 0; rowIndex < board.Length; ++rowIndex)
            {
                for (int colIndex = 0; colIndex < board[rowIndex].Length; ++colIndex)
                {
                    if (correctPiece(move, board[rowIndex][colIndex]))
                    {
                        // See if any jumps are possible
                        List<IState> potentialJumps = getJumps(rowIndex, colIndex, move, this);
                        
                        // When no jumps are found, the current state is returned 
                        if (!potentialJumps.Contains(this))
                            successorsJump.AddRange(potentialJumps);

                        // See what normal moves are possible
                        successorsNormal.AddRange(getMoves(rowIndex, colIndex, move));
                    }
                }
            }

            // Determine which successor list to return
            if (successorsJump.Count != 0)
            {
                // If a jump is possible, it must be taken
                return successorsJump;
            }
            else
            {
                // Otherwise, normal moves may be considered
                return successorsNormal;
            }
        }

        // Generate all the "normal" moves that do not involve jumps
        private List<IState> getMoves(int row, int col, Move move)
        {
            // Ending states
            List<IState> successors = new List<IState>();

            // Save the state of the current cell on the board
            Piece curPiece = board[row][col];
            bool isKing = (curPiece == Piece.WhiteKing || curPiece == Piece.BlackKing) ? true : false;

            // Direction of advancement, either up or down the board
            int advance = (move == Move.White) ? -1 : 1;

            // Forward-left
            if (inBounds(row + advance, col - 1)
                && board[row + advance][col - 1] == Piece.Blank)
            {
                // Add a modified version of the current state to successors
                successors.Add(makeMove(row, col, row + advance, col - 1, false));
            }

            // Forward-right
            if (inBounds(row + advance, col + 1)
                && board[row + advance][col + 1] == Piece.Blank)
            {
                // Add a modified version of the current state to successors
                successors.Add(makeMove(row, col, row + advance, col + 1, false));
            }

            // If the current cell has a king
            if (isKing)
            {
                // Back-left
                if (inBounds(row - advance, col - 1)
                    && board[row - advance][col - 1] == Piece.Blank)
                {
                    // Add a modified version of the current state to successors
                    successors.Add(makeMove(row, col, row - advance, col - 1, false));
                }

                // Back-right
                if (inBounds(row - advance, col + 1)
                    && board[row - advance][col + 1] == Piece.Blank)
                {
                    // Add a modified version of the current state to successors
                    successors.Add(makeMove(row, col, row - advance, col + 1, false));
                }
            }

            // Return all viable, normal move states found
            return successors;
        }

        // Recursively generate all states that could result from a jump
        private List<IState> getJumps(int row, int col, Move move, CheckerState state)
        {
            // Ending states
            List<IState> successors =  new List<IState>();
            
            // Current player's piece
            Piece curPiece = board[row][col];
            bool isKing = (curPiece == Piece.WhiteKing || curPiece == Piece.BlackKing) ? true : false;
            
            // Enemy piece types
            Piece otherPiece;
            Piece otherPieceKing;
            
            // What direction in which to move
            int advance;

            if (move == Move.White)
            {
                otherPiece = Piece.Black;
                otherPieceKing = Piece.BlackKing;
                advance = -1;
            }
            else
            {
                otherPiece = Piece.White;
                otherPieceKing = Piece.WhiteKing;
                advance = 1;
            }

            // If there is a forward-left enemy piece
            if (inBounds(row + advance, col - 1)
                && (board[row + advance][col - 1] == otherPiece
                || board[row + advance][col - 1] == otherPieceKing))
            {
                // If there is a blank cell to bridge to
                if (inBounds(row + (advance * 2), col - 2)
                    && (board[row + (advance * 2)][col - 2] == Piece.Blank))
                {
                    // Add any possible successors
                    CheckerState modState = makeMove(row, col, row + (advance * 2), col - 2, true);

                    // If current piece becomes a king
                    if (isKing == false
                        && (modState.board[row + (advance * 2)][col - 2] == Piece.WhiteKing
                        || modState.board[row + (advance * 2)][col - 2] == Piece.BlackKing))
                    {
                        // End all successive jumps
                        successors.Add(modState);
                    }
                    else
                    {
                        // Otherwise, find all possible branching possibilities
                        successors.AddRange(getJumps(row + (advance * 2), col - 2, move, modState));
                    }
                }
            }

            // If there is a forward-right enemy piece
            if (inBounds(row + advance, col + 1)
                && (board[row + advance][col + 1] == otherPiece
                || board[row + advance][col + 1] == otherPieceKing))
            {
                // If there is a blank cell to bridge to
                if (inBounds(row + (advance * 2), col + 2)
                    && (board[row + (advance * 2)][col + 2] == Piece.Blank))
                {
                    // Add any possible successors
                    CheckerState modState = makeMove(row, col, row + (advance * 2), col + 2, true);

                    // If current piece becomes a king
                    if (isKing == false
                        && (modState.board[row + (advance * 2)][col + 2] == Piece.WhiteKing
                        || modState.board[row + (advance * 2)][col + 2] == Piece.BlackKing))
                    {
                        // End all successive jumps
                        successors.Add(modState);
                    }
                    else
                    {
                        // Otherwise, find all possible branching possibilities
                        successors.AddRange(getJumps(row + (advance * 2), col + 2, move, modState));
                    }
                }
            }

            // If the current piece is already a king
            if (isKing)
            {
                // If there is a back-left enemy piece
                if (inBounds(row - advance, col - 1)
                    && (board[row - advance][col - 1] == otherPiece
                    || board[row - advance][col - 1] == otherPieceKing))
                {
                    // If there is a blank cell to bridge to
                    if (inBounds(row - (advance * 2), col - 2)
                        && (board[row - (advance * 2)][col - 2] == Piece.Blank))
                    {
                        // Add any possible successors
                        CheckerState modState = makeMove(row, col, row - (advance * 2), col - 2, true);
                        successors.AddRange(getJumps(row - (advance * 2), col - 2, move, modState));
                    }
                }

                // If there is a back-right enemy piece
                if (inBounds(row - advance, col + 1)
                    && (board[row - advance][col + 1] == otherPiece
                    || board[row - advance][col + 1] == otherPieceKing))
                {
                    // If there is a blank cell to bridge to
                    if (inBounds(row - (advance * 2), col + 2)
                        && (board[row - (advance * 2)][col + 2] == Piece.Blank))
                    {
                        // Add any possible successors
                        CheckerState modState = makeMove(row, col, row - (advance * 2), col + 2, true);
                        successors.AddRange(getJumps(row - (advance * 2), col + 2, move, modState));
                    }
                }
            }

            // If no further jumps were found, this is a terminal state (base case)
            if (successors.Count == 0)
                successors.Add(state);
                        
            // Return all viable successor jump states found
            return successors;
        }

        // Modify, clone, and restore the current state to produce a successor state 
        private CheckerState makeMove(int rowSrc, int colSrc, int rowDest, int colDest, bool isJump)
        {
            // The piece to move
            Piece curPiece = board[rowSrc][colSrc];
            Piece midPiece = board[(rowSrc + rowDest) / 2][(colSrc + colDest) / 2];

            // Modify
            board[rowSrc][colSrc] = Piece.Blank;
            board[rowDest][colDest] = curPiece;

            if (isJump)
                board[(rowSrc + rowDest) / 2][(colSrc + colDest) / 2] = Piece.Blank;

            // Convert any men to kings
            if (board[rowDest][colDest] == Piece.White && rowDest == 0)
                board[rowDest][colDest] = Piece.WhiteKing;
            
            if (board[rowDest][colDest] == Piece.Black && rowDest == 7)
                board[rowDest][colDest] = Piece.BlackKing;
            
            // Clone
            CheckerState modState = new CheckerState(this);
            
            // Restore
            if (isJump)
                board[(rowSrc + rowDest) / 2][(colSrc + colDest) / 2] = midPiece;
            
            board[rowDest][colDest] = Piece.Blank;
            board[rowSrc][colSrc] = curPiece;

            // Return new state
            return modState;
        }

        // Determine if the piece matches the move
        private bool correctPiece(Move move, Piece piece)
        {
            // Return true if the piece matches the move
            if (move == Move.White)
                return (piece == Piece.White || piece == Piece.WhiteKing) ? true : false;
            
            if (move == Move.Black)
                return (piece == Piece.Black || piece == Piece.BlackKing) ? true : false;
            
            // Return false if the piece is blank
            return false;
        }

        // Determine if a board location is in bounds
        private bool inBounds(int row, int col)
        {
            return !(row < 0 || row >= board.Length) && !(col < 0 || col >= board[0].Length);
        }
    }
}