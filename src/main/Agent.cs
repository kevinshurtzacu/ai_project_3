using System.Collections.Generic;

namespace Learn
{
    public class Agent
    {
        // Possible Agent conditions
        public enum Condition : byte { Success, Failure, Uncertain };

        // Starting state for problem domain
        public IState State { get; set; }

        // Define a delegate for testing goals
        public delegate bool GoalTest(IState state);
        public GoalTest Test { get; set; }

        // Discovery bank to store and reward discovered states
        private Discovered discovered;

        // Create a default Agent with no starting state or GoalTest
        public Agent()
        {
            State = null;
            Test = null;
            
            discovered = new Discovered();
        }

        // Create an agent with a starting state but no GoalTest
        public Agent(IState startingState)
        {
            State = startingState;
            Test = null;

            discovered = new Discovered();

            /* Place the initial state into the Discovery bank
             * in case the initial state happens to be the solution.
             */
            List<IState> starter = new List<IState>();
            starter.Add(State);
            discovered.ChooseSuccessor(starter);
        }

        // Create an agent with a starting state and GoalTest
        public Agent(IState startingState, GoalTest test)
        {
            State = startingState;
            Test = test;
            
            discovered = new Discovered();
            List<IState> starter = new List<IState>();
            starter.Add(State);
            discovered.ChooseSuccessor(starter);
        }

        public Condition Act()
        {
            if (Test(State))
            {
                discovered.Update();
                discovered.Clear();
                return Condition.Success;
            }

            List<IState> successorStates = State.getSuccessors();
            
            return Condition.Uncertain;
        }

        public void Reset()
        {

        }
    }
}