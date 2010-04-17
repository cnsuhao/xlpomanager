using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Streamlet.POManager.POUtility
{
    using StringCollection = Dictionary<string, string>;

    public class POData
    {
        public StringCollection Header
        {
            get;
            set;
        }

        public StringCollection Content
        {
            get;
            set;
        }

        public POData()
        {
            Header = new StringCollection();
            Content = new StringCollection();
        }

        public void Clear()
        {
            Header.Clear();
            Content.Clear();
        }
    }
}
