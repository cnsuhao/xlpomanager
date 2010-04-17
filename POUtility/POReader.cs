using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Streamlet.POManager.POUtility
{
    using StringPair = KeyValuePair<string, string>;
    using StringCollection = Dictionary<string, string>;
    
    public static class POReader
    {
        public static POData ParseText(string poText)
        {
            return POParser.Eval(poText);
        }

        public static POData ParseFile(string poFile)
        {
            TextReader tr = new StreamReader(poFile, Encoding.UTF8);
            string poText = tr.ReadToEnd();
            tr.Close();
            return ParseText(poText);
        }
    }
}
