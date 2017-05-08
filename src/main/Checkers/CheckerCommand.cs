using Microsoft.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.IO;
using System;
using Learn;
namespace Learn.Checkers
{
    public class CheckerCommand : ICommand
    {
        // Runtime settings
        private long training;
        private bool useInFile;
        private bool useOutFile;
        private string inFile;
        private string outFile;
        private bool quiet;

        public CheckerCommand(string inFileName, string outFileName, long numGames, bool isQuiet)
        {
            // Set input file settings
            useInFile = (inFileName.Length == 0) ? false : true;
            if (useInFile) inFile = inFileName;

            // Set output file settings
            useOutFile = (outFileName.Length == 0) ? false : true;
            if (useOutFile) outFile = outFileName;

            // Set training value
            training = numGames;

            // Set output verbosity
            quiet = isQuiet;
        }

        public void Run()
        {
            if (!quiet)
                Console.WriteLine("RL Checkers");

            IState state = new CheckerState();
            CheckerState checkerState = state as CheckerState;
            
            Agent playerOne = new Agent();
            Agent playerTwo = new Agent();

            // Wire player behaviors to state events
            CheckerState.WireEvents(checkerState, playerOne, playerTwo);

            // Handle Arguments
            if (useInFile)  Utilities.ReadInFile(inFile, playerOne, playerTwo);
            if (useOutFile) Utilities.WriteOutFile(outFile, playerOne);

            // Train agents
            if (!useInFile) CheckerState.TrainZeroSum(training, !quiet, playerOne, playerTwo);
            Console.WriteLine(playerTwo.Discovered);
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
        }
        
        public static void Configure(CommandLineApplication app)
        {
            // Set description and help option
            app.Description = "Train an agent to play Checkers";
            app.HelpOption("-h | --help");

            // Handle serialized input
            CommandOption inFileName = app.Option(
                "-i | --inFile <filename>",
                "Enter the filename to load to restore learning data",
                CommandOptionType.SingleValue
            );

            // Produce serialized output
            CommandOption outFileName = app.Option(
                "-o | --outFile <filename>",
                "Enter the filename to target when saving the learning data",
                CommandOptionType.SingleValue
            );

            CommandOption numGames = app.Option(
                "-t | --training <games>",
                "Enter the number of games to run during training before playing",
                CommandOptionType.SingleValue
            );

            CommandOption isQuiet = app.Option(
                "-q | --quiet",
                "Silences output when training agents, including progress bar",
                CommandOptionType.NoValue
            );

            // Runtime function
            app.OnExecute(() =>
            {
                // Run the CheckerCommand
                (new CheckerCommand(
                    inFileName.HasValue()   ? inFileName.Value()            : "",
                    outFileName.HasValue()  ? outFileName.Value()           : "",
                    numGames.HasValue()     ? Int32.Parse(numGames.Value()) : 10000,
                    isQuiet.HasValue())
                ).Run();
                
                // Confirm success
                return 0;
            });
        }
    }
}