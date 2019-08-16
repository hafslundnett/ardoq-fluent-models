using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ModelMaintainer.Utils
{
    public class CsvCrawlingParser
    {
        private readonly char _delimiter;

        public CsvCrawlingParser(char delimiter)
        {
            _delimiter = delimiter;
        }

        public IEnumerable<string[]> Parse(string inp)
        {
            var input = inp.Replace("\n\r", "\n");
            input = input.Replace("\r\n", "\n");

            var result = new List<string[]>();

            var elementAccum = new StringBuilder();
            var lineAccum = new List<string>();
            var inString = false;
            for (int i = 0; i < input.Length; i++)
            {
                var c = input[i];
                if (c == '"')
                {
                    inString = !inString;
                }
                else if (inString)
                {
                    elementAccum.Append(c);
                }
                else if (c == _delimiter)
                {
                    lineAccum.Add(elementAccum.ToString());
                    elementAccum = new StringBuilder();
                }
                else if (c == '\n')
                {
                    if (elementAccum.Length > 0)
                    {
                        lineAccum.Add(elementAccum.ToString());
                    }

                    result.Add(lineAccum.ToArray());
                    lineAccum = new List<string>();
                    elementAccum = new StringBuilder();
                }
                else
                {
                    elementAccum.Append(c);
                }
            }

            if (elementAccum.Length > 0)
            {
                lineAccum.Add(elementAccum.ToString());
            }

            if (lineAccum.Any())
            {
                result.Add(lineAccum.ToArray());
            }

            return result;
        }
    }
}
