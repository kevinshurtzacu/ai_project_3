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

        static void Main(string[] args)
        {
            Console.WriteLine("Tic Tac Toe Machine Learning Demonstration");

            // Create starting state
            IState state = new TicTacToeState();
            
            // Create players one and two
            Agent playerOne = new Agent();
            Agent playerTwo = new Agent();

            // Wire player behaviors to state events
            wireEvents((TicTacToeState)state, playerOne, playerTwo);

            // Watch each agent evolve
            while (true)
            {
                Console.WriteLine($"{playerOne.Victories}\t{playerTwo.Victories}\t{playerOne.Draws}");
                state = playerOne.Act(((TicTacToeState)state).GoalTestX);

                Console.WriteLine($"{playerOne.Victories}\t{playerTwo.Victories}\t{playerOne.Draws}");
                state = playerTwo.Act(((TicTacToeState)state).GoalTestO);
            }
        }
    }
}
