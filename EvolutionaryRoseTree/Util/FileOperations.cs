using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace EvolutionaryRoseTree.Util
{
    class FileOperations
    {

        internal static List<string> LoadWordList(string keywordPath)
        {
            StreamReader sr = new StreamReader(keywordPath);

            List<string> words = new List<string>();
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.Length > 0)
                    words.Add(line);
            }

            return words;
        }


        public static Dictionary<string, List<string>> LoadConfigFile(string filename)
        {
            var config = new Dictionary<string, List<string>>();
            StreamReader sr = new StreamReader(filename);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                if (line.StartsWith("//") || line.Length == 0)
                    continue;
                var paraName = line;
                line = sr.ReadLine();
                List<string> paraVals = new List<string>();
                while (line != null && line.Length > 0)
                {
                    paraVals.Add(line);
                    line = sr.ReadLine();
                }
                config.Add(paraName, paraVals);
            }

            sr.Close();
            return config;
        }
    }
}
