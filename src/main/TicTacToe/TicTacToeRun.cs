using System.Collections.Generic;
using System;

namespace Learn.TicTacToe
{
    class TicTacToeRun
    {
        private static void wireEvents(TicTacToeState tttState, Agent playerOne, Agent playerTwo)
        {
            // Wire up player behaviors to state events
            TicTacToeState.PlayerWinsX += playerOne.Victory;
            TicTacToeState.PlayerWinsX += playerTwo.Defeat;
            TicTacToeState.PlayerWinsO += playerOne.Defeat;
            TicTacToeState.PlayerWinsO += playerTwo.Victory;
            TicTacToeState.CatsGame    += playerOne.Draw;
            TicTacToeState.CatsGame    += playerTwo.Draw;
        }
        
        private static void trainZeroSum(long practiceGames, params Agent[] agents)
        {
            // Create starting state
            IState state = new TicTacToeState();
            TicTacToeState tttState = state as TicTacToeState;
            
            // Put agents in training mode
            foreach (Agent agent in agents)
                agent.TrainingMode();

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
            Console.Write("[ ");

            // Watch each agent evolve
            while (!enoughTraining)
            {
                // Train the first agent
                tttState = agents[0].Act(tttState.GoalTestX) as TicTacToeState;

                games = agents[0].Victories + agents[1].Victories + agents[0].Draws;
                
                if (games % (practiceGames / 100) == 0 && !dotPrinted)
                {
                    // Update competitiveness at quarters
                    if (ticks % 25 == 0)
                    {
                        // Display quarter in progress bar
                        Console.Write($"[{ticks}]");

                        // Make each agent more competitive
                        foreach (Agent agent in agents)
                            agent.TrainingMode((ticks / 100) * 0.5);
                    }

                    // Update ticks
                    ticks += 1;

                    // Don't include hundredth tick
                    if (ticks < 100)
                        Console.Write(".");
                    
                    dotPrinted = true;
                }

                // Check if total number of practices have been met
                if (games > practiceGames)
                    enoughTraining = true;
                
                // Train the second agent, assuming they are not sufficiently trained
                if (!enoughTraining)
                {
                    tttState = agents[1].Act(tttState.GoalTestO) as TicTacToeState;

                    games = agents[0].Victories + agents[1].Victories + agents[0].Draws;
                    
                    if (games % (practiceGames / 100) == 0 && !dotPrinted)
                    {
                        // Update competitiveness at quarters
                        if (ticks % 25 == 0)
                        {
                            // Display quarter in progress bar
                            Console.Write($"[{ticks}]");

                            // Make each agent more competitive
                            foreach (Agent agent in agents)
                                agent.TrainingMode((ticks / 100) * 0.5);
                        }

                        // Update ticks
                        ticks += 1;

                        // Don't include hundredth tick
                        if (ticks < 100)
                            Console.Write(".");
                        
                        dotPrinted = true;
                    }

                    // Check if total number of practices have been met
                    if (games > practiceGames)
                        enoughTraining = true;
                }
            }

            
            // End progress bar
            Console.WriteLine(" ]");
            Console.WriteLine("\n");

            // Put agents in competitive mode
            foreach (Agent agent in agents)
                agent.CompeteMode();
        }

        private static void printOptions(List<IState> successors)
        {
            for (int successorIndex = 0; successorIndex < successors.Count; ++successorIndex)
            {
                Console.WriteLine($"--- Option {successorIndex + 1} [{successorIndex + 1}] ---", successorIndex);
                Console.WriteLine(successors[successorIndex]);
            }
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Tic Tac Toe Machine Learning Demonstration");

            // Create starting state
            IState state = new TicTacToeState();
            TicTacToeState tttState = state as TicTacToeState;

            // Create players one and two
            Agent playerOne = new Agent();
            Agent playerTwo = new Agent();

            // Wire player behaviors to state events and train agents
            wireEvents(tttState, playerOne, playerTwo);
            trainZeroSum(100000, playerOne, playerTwo);

            // Determine victory, defeat, and cat's game events
            bool playerTwoVictory = false;
            Action<object, EventArgs> declareVictory = (object sender, EventArgs e) => playerTwoVictory = true;
            TicTacToeState.PlayerWinsO += declareVictory;

            bool playerTwoDefeat = false;
            Action<object, EventArgs> declareDefeat = (object sender, EventArgs e) => playerTwoDefeat = true;
            TicTacToeState.PlayerWinsX += declareDefeat;

            bool playerTwoDraw = false;
            Action<object, EventArgs> declareDraw = (object sender, EventArgs e) => playerTwoDraw = true;
            TicTacToeState.CatsGame += declareDraw;

            // Debug
            Console.WriteLine(playerOne);

            // Player competes with computer
            while (!playerTwoVictory && !playerTwoDraw && !playerTwoDefeat)
            {
                Console.WriteLine(tttState);
                tttState = playerOne.Act(tttState.GoalTestX) as TicTacToeState;

                if (!playerTwoVictory && !playerTwoDraw && !playerTwoDefeat)
                {
                    // Display move options
                    List<IState> options = tttState.GoalTestO();
                    printOptions(options);

                    // Prompt the user
                    Console.Write("Please select a move: ");
                    int moveIndex = Int32.Parse(Console.ReadLine()) - 1;

                    // Assign the state
                    tttState = options[moveIndex] as TicTacToeState;
                }

                // Select outcome
                if (playerTwoVictory)
                    Console.WriteLine("Human player wins!");
                
                if (playerTwoDefeat)
                    Console.WriteLine("Computer wins!");
                
                if (playerTwoDraw)
                    Console.WriteLine("Cat's game!");
            }
        }
    }
}
