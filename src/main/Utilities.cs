using Microsoft.Extensions.CommandLineUtils;
using System.IO;
using System;

namespace Learn
{
    public static class Utilities
    {
        // Deserialize data
        public static void ReadInFile(string inFile, Agent agentOne, Agent agentTwo)
        {
            // Deserialize data or train fresh agents
            try
            {
                // Use previously constructed data
                using (FileStream rofStream = File.OpenRead(inFile))
                    agentOne.Discovered.ImportData(rofStream);
            }
            catch (Exception e)
            {
                // Report failure to open or read serialized data
                Console.Error.WriteLine($"Error: {e}; Failed to read serialized data");
            }
        }
        
        // Save serialized data
        public static void WriteOutFile(string outFile, Agent agent)
        {
            try
            {
                // Use previously constructed data
                using (FileStream wfStream = File.OpenWrite(outFile))
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