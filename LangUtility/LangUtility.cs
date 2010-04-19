using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Streamlet.POManager.LanguageUtility
{
    public static class LangUtility
    {
        public const int TopLCID = 0x7f;

        public static IEnumerable<CultureInfo> EnumAllLanguages()
        {
            return from ci in CultureInfo.GetCultures(CultureTypes.AllCultures)
                   where ci.LCID != TopLCID && ci.LCID != ci.Parent.LCID
                   select ci;
        }

        public static IEnumerable<CultureInfo> EnumSubLanguages(int lcid)
        {
            return from ci in EnumAllLanguages()
                   where ci.Parent.LCID == lcid
                   select ci;
        }

        public static IEnumerable<CultureInfo> EnumSubLanguages(string lcname)
        {
            return from ci in EnumAllLanguages()
                   where ci.Parent.Name == lcname
                   select ci;
        }

        public static IEnumerable<CultureInfo> EnumTopLevelLanguages()
        {
            return EnumSubLanguages(TopLCID);
        }
    }
}
