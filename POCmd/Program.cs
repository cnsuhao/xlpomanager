using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Streamlet.POManager.POUtility;

namespace Streamlet.POManager.POCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            POData poData = POReader.ParseFile(@"..\..\..\TestFiles\en.po");
            poData = POReader.ParseFile(@"..\..\..\TestFiles\zh.po");
            poData = POReader.ParseFile(@"..\..\..\TestFiles\zh-CN.po");
        }
    }
}
