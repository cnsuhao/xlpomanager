using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Streamlet.POManager.POUtility;
using Streamlet.POManager.LanguageUtility;

namespace Streamlet.POManager.POCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            var lang = LangUtility.EnumAllLanguages();

            var poData = POReader.ParseFile(@"..\..\..\TestFiles\en.po");
            poData = POReader.ParseFile(@"..\..\..\TestFiles\zh.po");
            poData = POReader.ParseFile(@"..\..\..\TestFiles\zh-CN.po");

            POWriter.WriteToFile(poData, "zh-CN.po");
        }
    }
}
