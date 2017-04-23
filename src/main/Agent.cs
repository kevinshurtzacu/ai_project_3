using System.Collections.Generic;
using System;

namespace Learn
{
    public class Agent
    {
        // Discovery bank to store and reward discovered states
        private Discovered discovered;

        // Wins, Losses, Draws
        public long Victories { get; set; }
        public long Defeats { get; set; }
        public long Draws { get; set; }

        // Create a default Agent with no GoalTest
        public Agent()
        {
            discovered = new Discovered();
        }

        // Respond to victory and defeat events
        public void Victory(object sender, EventArgs e)
        {
            discovered.Reward();
            Victories += 1;
        }

        public void Defeat(object sender, EventArgs e)
        {
            discovered.Penalize();
            Defeats += 1;
        }

        public void Draw(object sender, EventArgs e)
        {
            discovered.Reset();
            Draws += 1;
        }

        // Use GoalTest to invoke appropriate behaviors
        public IState Act(Func<List<IState>> goalTest)
        {
            // Generate successor states
            List<IState> successorStates = goalTest();

            // Return the next best option
            return discovered.ChooseSuccessor(successorStates);
        }
    }
}