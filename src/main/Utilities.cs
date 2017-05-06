using Microsoft.Extensions.CommandLineUtils;
using System.IO;
using System;

namespace Learn
{
    public static class Utilities
    {
        // Deserialize data
        public static void HandleInFile(CommandOption inFile, Agent agentOne, Agent agentTwo, 
                                         long training, Action<long, bool, Agent, Agent> train)
        {
            // Deserialize data or train fresh agents
            if (inFile.HasValue())
            {
                try
                {
                    // Use previously constructed data
                    using (FileStream rofStream = File.OpenRead(inFile.Value()))
                        agentOne.Discovered.ImportData(rofStream);
                }
                catch (Exception e)
                {
                    // Report failure to open or read serialized data
                    Console.Error.WriteLine($"Error: {e}; Failed to read serialized data");
                }
            }
            else
            {
                // Train fresh agents
                train(training, true, agentOne, agentTwo);                
            }
        }
        
        // Save serialized data
        public static void HandleOutFile(CommandOption outFile, Agent agent)
        {
            if (outFile.HasValue())
            {
                try
                {
                    // Use previously constructed data
                    using (FileStream wfStream = File.OpenWrite(outFile.Value()))
                        agent.Discovered.ExportData(wfStream);
                }
                catch (Exception e)
                {
                    // Report failure to open or write serialized data
                    Console.Error.WriteLine($"Error: {e}; Failed to write serialized data");
                }
            }
        }
    }
}