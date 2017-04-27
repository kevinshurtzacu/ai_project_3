using System.Collections.Generic;
using System;
using Learn.TicTacToe;

namespace Learn
{
    public class Discovered
    {
        private Stack<IState> moves;
        private IDictionary<IState, double> discovered;

        public double ExploreRate { get; set; }

        // Create a default Discovered bank
        public Discovered()
        {
            moves = new Stack<IState>();
            discovered = new Dictionary<IState, double>();
            ExploreRate = 0.0;
        }

        // Create a Discovered bank with a custom ExploreRate
        public Discovered(double rate)
        {
            moves = new Stack<IState>();
            discovered = new Dictionary<IState, double>();
            ExploreRate = rate;
        }

        public IState ChooseSuccessor(List<IState> states)
        {
            IState bestState = null;
            double bestValue = 0.0;

            foreach (IState state in states)
            {
                // Add any new states to the discovered list
                if (!discovered.ContainsKey(state))
                    discovered.Add(state, 0.5);

                // Update best state most of the time
                Random rand = new Random();

                if (rand.NextDouble() >= ExploreRate || bestState == null)
                    bestState = (discovered[state] > bestValue) ? state : bestState;
                else
                    bestState = (states[rand.Next() % states.Count]);
            }

            // Add chosen state to moves
            moves.Push(bestState);

            return bestState;
        }

        public void Reward()
        {
            // Improve all state scores on a graduated scale from 80% to 20%
            int numMoves = moves.Count;
            double percentBonus = .80;

            while (moves.Count > 0)
            {
                IState state = moves.Pop();
                
                // Reward favorable states
                double diff = 1.0 - discovered[state];
                double reward = diff * percentBonus;
                discovered[state] += reward;

                // Decrement reward percentage
                percentBonus -= (.80 - .20) / numMoves; 
            }
        }

        public void Penalize()
        {
            // Penalize all state scores on a graduated scale from 80% to 20%
            int numMoves = moves.Count;
            double percentPenalty = .80;

            while (moves.Count > 0)
            {
                IState state = moves.Pop();
                
                // Reward favorable states
                double diff = discovered[state];
                double penalty = diff * percentPenalty;
                discovered[state] -= penalty;

                // Decrement reward percentage
                percentPenalty -= (.80 - .20) / numMoves; 
            }
        }

        // Clear out past moves, no rewards
        public void Reset() => moves.Clear();
        
        public override string ToString()
        {
            string output = "";

            // Summarize information
            output += String.Format($"Keys: {discovered.Keys.Count}");
            output += String.Format($" Values: {discovered.Values.Count}");
            output += String.Format($" Moves: {moves.Count}\n");

            return output;
        }
    }

}