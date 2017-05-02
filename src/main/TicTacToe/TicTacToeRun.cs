using Microsoft.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.IO;
using System;

namespace Learn.TicTacToe
{
    class TicTacToeRun
    {
        // Deserialize data
        private static void handleInFile(CommandOption inFile, Agent agentOne, Agent agentTwo, int training)
        {
            // Deserialize data or train fresh agents
            if (inFile.HasValue())
            {
                try
                {
                    // Use previously constructed data
                    using (FileStream rofStream = File.OpenRead(inFile.Value()))
                        agentOne.Discovered.ImportData(rofStream);
                }
                catch (Exception e)
                {
                    // Report failure to open or read serialized data
                    Console.Error.WriteLine($"Error: {e}; Failed to read serialized data");
                }
            }
            else
            {
                // Train fresh agents
                TicTacToeState.TrainZeroSum(training, true, agentOne, agentTwo);                
            }
        }
        
        // Save serialized data
        private static void handleOutFile(CommandOption outFile, Agent agent)
        {
            if (outFile.HasValue())
            {
                try
                {
                    // Use previously constructed data
                    using (FileStream wfStream = File.OpenWrite(outFile.Value()))
                        agent.Discovered.ExportData(wfStream);
                }
                catch (Exception e)
                {
                    // Report failure to open or write serialized data
                    Console.Error.WriteLine($"Error: {e}; Failed to write serialized data");
                }
            }
        }

        static void Main(string[] args)
        {
            // Process CLI arguments
            var cla = new CommandLineApplication(throwOnUnexpectedArg: false);
            
            // Handle serialized input
            CommandOption inFile = cla.Option(
                "-i | --inFile <filename>",
                "Enter the filename to load to restore learning data",
                CommandOptionType.SingleValue
            );

            // Produce serialized output
            CommandOption outFile = cla.Option(
                "-o | --outFile <filename>",
                "Enter the filename to target when saving the learning data",
                CommandOptionType.SingleValue
            );

            CommandOption training = cla.Option(
                "-t | --training <games>",
                "Enter the number of games to run during training before playing",
                CommandOptionType.SingleValue
            );

            // Configure help option
            cla.HelpOption("-h | --help");

            // Run the application
            cla.OnExecute(() =>
            {
                Console.WriteLine("Tic Tac Toe Machine Learning Demonstration");

                // Create starting state
                IState state = new TicTacToeState();
                TicTacToeState tttState = state as TicTacToeState;

                // Create players one and two
                Agent playerOne = new Agent();
                Agent playerTwo = new Agent();

                // Wire player behaviors to state events
                TicTacToeState.WireEvents(tttState, playerOne, playerTwo);
                
                // Set number of games to train for, default is 100,000
                int numGames = training.HasValue() ? Int32.Parse(training.Value()) : 100000;

                // Handle Arguments
                handleInFile(inFile, playerOne, playerTwo, numGames);
                handleOutFile(outFile, playerOne);

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

                // Player competes with computer
                while (!playerTwoVictory && !playerTwoDraw && !playerTwoDefeat)
                {
                    tttState = playerOne.Act(tttState.SuccessorsX) as TicTacToeState;

                    // No need to GoalTest if game is over
                    if (!playerTwoDraw)
                        tttState.GoalTest();

                    if (!playerTwoVictory && !playerTwoDraw && !playerTwoDefeat)
                    {
                        // Display move options
                        List<IState> options = tttState.SuccessorsO;
                        
                        if (!playerTwoDraw)
                        {
                            // Prompt the user
                            TicTacToeState.PrintOptions(options);
                            Console.Write("Please select a move: ");
                            int moveIndex = Int32.Parse(Console.ReadLine()) - 1;

                            // Assign the state
                            tttState = options[moveIndex] as TicTacToeState;
                            tttState.GoalTest();
                        }
                    }

                    // Select outcome
                    if (playerTwoVictory)
                        Console.WriteLine("Human player wins!");
                    
                    if (playerTwoDefeat)
                        Console.WriteLine("Computer wins!");
                    
                    if (playerTwoDraw)
                        Console.WriteLine("Cat's game!");
                }

                return 0;
            });

            // Run the application
            cla.Execute(args);
        }
    }
}
