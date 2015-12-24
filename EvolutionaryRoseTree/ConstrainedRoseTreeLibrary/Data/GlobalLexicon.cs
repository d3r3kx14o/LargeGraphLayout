using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConstrainedRoseTreeLibrary.Data
{
    public class GlobalLexicon
    {
        public Dictionary<string, int> Lexicon;
        public Dictionary<int, string> InvertedLexicon;

        public GlobalLexicon(Dictionary<string, int> lexicon)
        {
            Lexicon = new Dictionary<string, int>(lexicon);
            InvertedLexicon = new Dictionary<int, string>();
            foreach (var kvp in Lexicon)
            {
                InvertedLexicon.Add(kvp.Value, kvp.Key);
            }
        }

        public GlobalLexicon(Dictionary<int, string> invertedLexicon)
        {
            InvertedLexicon = new Dictionary<int, string>(invertedLexicon);
            Lexicon = new Dictionary<string, int>();
            foreach (var kvp in InvertedLexicon)
            {
                Lexicon.Add(kvp.Value, kvp.Key);
            }
        }
    }
}
