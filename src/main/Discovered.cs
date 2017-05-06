using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.IO;
using System;
using Learn.TicTacToe;
using Learn.Checkers;

namespace Learn
{
    [DataContract]
    [KnownType(typeof(TicTacToeState))]
    [KnownType(typeof(CheckerState))]
    [KnownType(typeof(Dictionary<IState, double>))]
    public sealed class Discovered
    {
        [DataMember]
        private IDictionary<IState, double> stateValues;
        private Stack<IState> moves;

        public double ExploreRate { get; set; }

        // Create a default Discovered bank
        public Discovered()
        {
            moves = new Stack<IState>();
            stateValues = new Dictionary<IState, double>();
            ExploreRate = 0.0;
        }

        // Create a Discovered bank with a custom ExploreRate
        public Discovered(double rate)
        {
            moves = new Stack<IState>();
            stateValues = new Dictionary<IState, double>();
            ExploreRate = rate;
        }

        public IState ChooseSuccessor(List<IState> states)
        {
            IState bestState = null;
            double bestValue = 0.0;

            foreach (IState state in states)
            {
                // Add any new states to the stateValues list
                if (!stateValues.ContainsKey(state))
                    stateValues.Add(state, 0.5);
                
                // Update best state
                bestState = (stateValues[state] > bestValue) ? state : bestState;
                bestValue = stateValues[bestState];
            }
            
            // Allow for exploration
            Random rand = new Random();

            if (rand.NextDouble() < ExploreRate)
                bestState = (states[rand.Next() % states.Count]);

            // Add chosen state to moves
            moves.Push(bestState);

            return bestState;
        }

        public void Reward()
        {
            // Improve all state scores on a graduated scale from 5% to 2%
            int numMoves = moves.Count;
            double percentBonus = .05;
            
            while (moves.Count > 0)
            {
                IState state = moves.Pop();

                // Reward favorable states
                double diff = 1.0 - stateValues[state];
                double reward = diff * percentBonus;
                stateValues[state] += reward;

                // Decrement reward percentage
                percentBonus -= (.05 - .02) / numMoves; 
            }
        }

        public void Penalize()
        {
            // Penalize all state scores on a graduated scale from 5% to 2%
            int numMoves = moves.Count;
            double percentPenalty = .05;
            
            while (moves.Count > 0)
            {
                IState state = moves.Pop();
                
                // Reward favorable states
                double diff = stateValues[state];
                double penalty = diff * percentPenalty;
                stateValues[state] -= penalty;

                // Decrement reward percentage
                percentPenalty -= (.05 - .02) / numMoves; 
            }
        }

        // Clear out past moves, no rewards
        public void Reset() { moves.Clear(); }
        
        // Export to XML with DataContract
        public void ExportData(FileStream outStream)
        {
            // Use a DataContract serializer to export to XML
            var dcs = new DataContractSerializer(typeof(Discovered));

            using (XmlWriter xmlWriter = XmlWriter.Create(outStream))
                dcs.WriteObject(xmlWriter, this);
        }

        // Import from XML with DataContract
        public void ImportData(FileStream inStream)
        {
            // Deserialize fs into disc
            Discovered disc = null;
            var dcs = new DataContractSerializer(typeof(Discovered));

            using (var reader = XmlDictionaryReader.CreateTextReader(inStream, new XmlDictionaryReaderQuotas()))
                disc = (Discovered)dcs.ReadObject(reader, true);
            
            // Copy relevant members
            stateValues = disc.stateValues;
        }

        public override string ToString()
        {
            string output = "";

            // Summarize information
            output += String.Format($"Keys: {stateValues.Keys.Count}");
            output += String.Format($" Values: {stateValues.Values.Count}");
            output += String.Format($" Moves: {moves.Count}\n");

            foreach (IState key in stateValues.Keys)
                output += String.Format($"State: {key.GetHashCode()}\t\tValue: {stateValues[key]}\n");

            return output;
        }
    }
}