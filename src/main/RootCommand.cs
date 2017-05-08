using Microsoft.Extensions.CommandLineUtils;
using Learn.TicTacToe;
using Learn.Checkers;

public class RootCommand : ICommand
{
    private readonly CommandLineApplication rootApp;

    public RootCommand(CommandLineApplication app)
    {
        rootApp = app;
    }

    public void Run()
    {
        rootApp.ShowHelp();
    }

    public static void Configure(CommandLineApplication app)
    {
        // app.Name = "learn";
        app.Description = "Train an agent to play games using reinforcement learning";
        app.HelpOption("-h | --help");

        // Register commands
        app.Command("tictactoe", TicTacToeCommand.Configure);
        app.Command("checkers", CheckerCommand.Configure);

        // Run command
        app.OnExecute(() =>
        {
            (new RootCommand(app)).Run();
            return 0;
        });
    }
}