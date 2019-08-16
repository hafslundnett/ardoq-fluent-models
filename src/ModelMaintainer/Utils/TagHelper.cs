using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelMaintainer.Utils
{
    public static class TagHelper
    {
        public static IEnumerable<string> ToArdoqTags(this IEnumerable<KeyValuePair<string, string>> map)
        {
            if (map == null)
            {
                return new List<string>();
            }

            return map.Select(p => $"{p.Key.ToLower()}-{p.Value.ToLower()}");
        }
    }
}
