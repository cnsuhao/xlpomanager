using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Streamlet.POManager.POUtility
{
    using StringPair = KeyValuePair<string, string>;
    using StringCollection = Dictionary<string, string>;

    public static class POWriter
    {
        public static void WriteToFile(POData poData, string poFile, bool writeBom = false, string lineEnding = "\r\n")
        {
            string poText = WriteToString(poData, lineEnding);
            byte[] bytes = Encoding.UTF8.GetBytes(poText);

            FileStream fs = new FileStream(poFile, FileMode.Create, FileAccess.Write, FileShare.None);

            if (writeBom)
            {
                byte[] bom = { 0xef, 0xbb, 0xbf };
                fs.Write(bom, 0, bom.Length);
            }

            fs.Write(bytes, 0, bytes.Length);
            fs.Close();
        }

        public static string WriteToString(POData poData, string lineEnding = "\r\n")
        {
            string poText = "";

            poText += GetHeaderString(poData.Header, lineEnding) + lineEnding;

            foreach (var pair in poData.Content)
            {
                poText += GetItemString(pair.Key, pair.Value, lineEnding) + lineEnding;
            }

            return poText;
        }

        private static string GetHeaderString(StringCollection header, string lineEnding = "\r\n")
        {
            string headerString = "msgid \"\"" + lineEnding +
                            "msgstr \"\"" + lineEnding;

            foreach (var pair in header)
            {
                headerString += "\"" + Escape(pair.Key) + ": " + Escape(pair.Value) + "\\n\"" + lineEnding;
            }

            return headerString;
        }

        private static string GetItemString(string original, string translated, string lineEnding = "\r\n")
        {
            return "msgid \"" + Escape(original) + "\"" + lineEnding +
                   "msgstr \"" + Escape(translated) + "\"" + lineEnding;
        }

        private static string Escape(string plain)
        { 
            return plain.Replace("\\", "\\\\")
                        .Replace("\"", "\\\"")
                        .Replace("\r", "\\r")
                        .Replace("\n", "\\n");
        }
    }
}
