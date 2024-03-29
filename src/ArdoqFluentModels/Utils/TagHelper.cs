﻿using System.Collections.Generic;
using System.Linq;

namespace ArdoqFluentModels.Utils
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