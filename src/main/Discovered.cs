using System.Collections.Generic;
using System;

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
            ExploreRate = .025;
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

                if (rand.NextDouble() >= ExploreRate)
                    bestState = discovered[state] > bestValue ? state : bestState;
            }

            // Add chosen state to moves
            moves.Push(bestState);

            return bestState;
        }

        public void Update()
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

        public void Clear()
        {
            // Clear out past moves, no rewards
            moves.Clear();
        }
    }
}