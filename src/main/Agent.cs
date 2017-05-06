using System.Collections.Generic;
using System;

namespace Learn
{
    public sealed class Agent
    {
        // Discovery bank to store and reward discovered states
        public Discovered Discovered {get; set; }

        // Wins, Losses, Draws
        public long Victories { get; set; }
        public long Defeats { get; set; }
        public long Draws { get; set; }

        // Create a default Agent with no GoalTest
        public Agent()
        {
            Discovered = new Discovered();
        }

        // Respond to victory, defeat, and draw events
        public void Victory(object sender, EventArgs e)
        {
            Discovered.Reward();
            Victories += 1;
        }

        public void Defeat(object sender, EventArgs e)
        {
            Discovered.Penalize();
            Defeats += 1;
        }

        public void Draw(object sender, EventArgs e)
        {
            Discovered.Reset();
            Draws += 1;
        }

        // Toggle between play behaviors
        public void TrainingMode(double exploreRate = .5) { Discovered.ExploreRate = exploreRate; }
        public void CompeteMode() { Discovered.ExploreRate = 0; }

        // Use GoalTest to invoke appropriate behaviors
        public IState Act(List<IState> successorStates)
        {
            // Return the next best option
            return Discovered.ChooseSuccessor(successorStates);
        }

        public override string ToString() => Discovered.ToString();
    }
}