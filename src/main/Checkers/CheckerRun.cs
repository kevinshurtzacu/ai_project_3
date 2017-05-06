using Microsoft.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.IO;
using System;

namespace Learn.Checkers
{
    public class CheckerRun
    {
        public static void Main(String[] args)
        {
            // Create a CLI Application
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

            // Runtime function
            cla.OnExecute(() =>
            {
                Console.WriteLine("RL Checkers");

                IState state = new CheckerState();
                CheckerState checkerState = state as CheckerState;
                
                Agent playerOne = new Agent();
                Agent playerTwo = new Agent();

                // Wire player behaviors to state events
                CheckerState.WireEvents(checkerState, playerOne, playerTwo);
                
                // Set number of games to train for, default is 10,000
                int numGames = training.HasValue() ? Int32.Parse(training.Value()) : 10000;

                // Handle Arguments
                Utilities.HandleInFile(inFile, playerOne, playerTwo, numGames, CheckerState.TrainZeroSum);
                Utilities.HandleOutFile(outFile, playerOne);

                // Determine victory, defeat, and cat's game events
                bool playerTwoVictory = false;
                bool playerTwoDefeat = false;
                
                Action<object, EventArgs> declareVictory = (object sender, EventArgs e) => playerTwoVictory = true;
                Action<object, EventArgs> declareDefeat = (object sender, EventArgs e) => playerTwoDefeat = true;
                
                CheckerState.WhiteWins += declareVictory;
                CheckerState.BlackWins += declareDefeat;

                // Player competes with computer
                while (!playerTwoVictory && !playerTwoDefeat)
                {
                    checkerState = playerOne.Act(checkerState.SuccessorsBlack) as CheckerState;
                    checkerState.GoalTest();

                    if (!playerTwoVictory && !playerTwoDefeat)
                    {
                        // Display move options
                        List<IState> options = checkerState.SuccessorsWhite;
                        int moveIndex = 0;
                        bool validMove = false;

                        // Prompt the user
                        while (!validMove)
                        {
                            try
                            {
                                // Receive user input
                                CheckerState.PrintOptions(options);
                                Console.Write("Please select a move: ");
                                moveIndex = Int32.Parse(Console.ReadLine()) - 1;

                                // If move is not possible, throw exception
                                if (moveIndex < 0 || moveIndex >= options.Count)
                                    throw new InvalidMoveException(moveIndex + 1);
                                
                                // If no error was raised, mark the move as valid
                                validMove = true;
                            }
                            catch (InvalidMoveException e)
                            {
                                Console.Write(e.Message);
                                Console.WriteLine("; please select from the options listed.");
                            }
                            catch (Exception e)
                            {
                                // Print error, exit application
                                Console.Error.WriteLine($"Error: {e}");
                                return 1;
                            }
                        }

                        // Assign the state
                        checkerState = options[moveIndex] as CheckerState;
                        checkerState.GoalTest();
                    }

                    // Select outcome
                    if (playerTwoVictory)
                        Console.WriteLine("Human player wins!");
                    
                    if (playerTwoDefeat)
                        Console.WriteLine("Computer wins!");                    
                }

                return 0;
            });

            // Run the application
            cla.Execute(args);
        }
    }
}