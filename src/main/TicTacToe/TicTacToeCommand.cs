using Microsoft.Extensions.CommandLineUtils;
using System.Collections.Generic;
using System.IO;
using System;

namespace Learn.TicTacToe
{
    class TicTacToeCommand : ICommand
    {
        // Runtime settings
        private long training;
        private bool useInFile;
        private bool useOutFile;
        private string inFile;
        private string outFile;
        private bool quiet;

        public TicTacToeCommand(string inFileName, string outFileName, long numGames, bool isQuiet)
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
                Console.WriteLine("Tic Tac Toe Machine Learning Demonstration");

            // Create starting state
            IState state = new TicTacToeState();
            TicTacToeState tttState = state as TicTacToeState;

            // Create players one and two
            Agent playerOne = new Agent();
            Agent playerTwo = new Agent();

            // Wire player behaviors to state events
            TicTacToeState.WireEvents(tttState, playerOne, playerTwo);

            // Handle serialization
            if (useInFile)  Utilities.ReadInFile(inFile, playerOne, playerTwo);
            if (useOutFile) Utilities.WriteOutFile(outFile, playerOne);

            // Conduct training
            if (!useInFile) TicTacToeState.TrainZeroSum(training, !quiet, playerOne, playerTwo);

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
        }

        public static void Configure(CommandLineApplication app)
        {
            // Set app description and help
            app.Description = "Train an agent to play Tic Tac Toe";
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

            // Run the application
            app.OnExecute(() =>
            {
                // Run the TicTacToeCommand
                (new TicTacToeCommand(
                    inFileName.HasValue()   ? inFileName.Value()            : "",
                    outFileName.HasValue()  ? outFileName.Value()           : "",
                    numGames.HasValue()     ? Int32.Parse(numGames.Value()) : 100000,
                    isQuiet.HasValue())
                ).Run();

                // Confirm success
                return 0;
            });
        }
    }
}
