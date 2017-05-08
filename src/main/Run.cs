using Microsoft.Extensions.CommandLineUtils;

public class Run
{
    public static void Main(string[] args)
    {
        // Create and run the application
        var app = new CommandLineApplication();
        RootCommand.Configure(app);
        app.Execute(args);
    }
}