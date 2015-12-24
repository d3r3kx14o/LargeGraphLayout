using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConstrainedRoseTreeLibrary.Data
{
    public class RawDocument
    {
        public RawDocument(Dictionary<string, int> docVector, int documentId)
        {
            DocumentId = documentId;
            DocumentContentVector = docVector;
            DocumentComplementVector = new Dictionary<string, int>();
        }

        public RawDocument(
            int documentID, 
            Dictionary<string, int> documentContentVector, 
            Dictionary<string, int> documentComplementVector)
        {
            DocumentId = documentID;
            DocumentContentVector = documentContentVector;
            DocumentComplementVector = documentComplementVector;
        }

        public int DocumentId { get; protected set; }
        public Dictionary<string, int> DocumentContentVector { get; protected set; }
        public Dictionary<string, int> DocumentComplementVector { get; protected set; }
    }
}
