using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace Streamlet.POManager.LanguageUtility
{
    public static class LangUtility
    {
        public static IEnumerable<CultureInfo> EnumAllLanguages()
        {
            return CultureInfo.GetCultures(CultureTypes.AllCultures);
        }

        public static IEnumerable<CultureInfo> EnumSubLanguages(int lcid)
        {
            return from ci in CultureInfo.GetCultures(CultureTypes.AllCultures)
                   where ci.Parent.LCID == lcid
                   select ci;
        }

        public static IEnumerable<CultureInfo> EnumSubLanguages(string lcname)
        {
            return from ci in CultureInfo.GetCultures(CultureTypes.AllCultures)
                   where ci.Parent.Name == lcname
                   select ci;
        }
    }
}
