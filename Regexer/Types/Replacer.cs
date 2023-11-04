using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Regexer
{
    internal class Replacer
    {
        private static readonly string Separator = @"\,";
        private static readonly Regex UnicodeEscapeRegex = new Regex(@"[\dabcdef]{4}", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public Regex[] SearchRegices { get; private set; } = new Regex[0];
        public Replacement[][] Replacements { get; private set; } = new Replacement[0][];

        public Replacer() { }
        public Replacer(PatternSet patternSet)
        {
            SetSearchRegices(patternSet.SearchPattern, patternSet.IsIgnoreCase);
            SetReplacements(patternSet.ReplacePattern, patternSet.IsEscape);
        }

        public void SetSearchRegices(string patterns, bool isIgnoreCase)
        {
            if (patterns.Length == 0)
            {
                SearchRegices = new Regex[0];
                return;
            }

            SearchRegices = patterns.Split(Separator)
                    .Select(pattern =>
                    {
                        return new Regex(pattern, RegexOptions.Singleline | RegexOptions.Compiled | (isIgnoreCase ? RegexOptions.IgnoreCase : 0));
                    })
                    .ToArray();
        }

        public void SetReplacements(string patterns, bool isEscape)
        {
            if (patterns.Length == 0)
            {
                Replacements = new Replacement[0][];
                return;
            }

            Replacements = (isEscape ? Escape(patterns) : Unescape(patterns)).Split(Separator)
                .Select(pattern =>
                {
                    int length = pattern.Length;
                    char[] chars = pattern.Concat(Enumerable.Repeat('\0', 2)).ToArray();

                    List<Replacement> result = new List<Replacement>();
                    ReplacementType type = ReplacementType.Normal;
                    List<char> values = new List<char>();

                    for (int i = 0; i < length; i++)
                    {
                        if (chars[i] == '$')
                        {
                            switch (chars[i + 1])
                            {
                                case 'l': switchType(ReplacementType.LowerFirst); i++; continue;
                                case 'L': switchType(ReplacementType.Lower); i++; continue;
                                case 'u': switchType(ReplacementType.UpperFirst); i++; continue;
                                case 'U': switchType(ReplacementType.Upper); i++; continue;
                                case 'E': switchType(ReplacementType.Normal); i++; continue;
                                case '$': values.Add('$'); switchType(type); i++; continue;
                            }

                            values.Add('$');
                            i++;
                        }

                        values.Add(chars[i]);
                    }

                    addReplacement();

                    return result.ToArray();

                    void switchType(ReplacementType newType)
                    {
                        addReplacement();
                        type = newType;
                        values = new List<char>();
                    }

                    void addReplacement()
                    {
                        string value = new string(values.ToArray()).TrimEnd('\0');
                        if (!value.Any())
                        {
                            return;
                        }

                        result.Add(new Replacement
                        {
                            Type = type,
                            Value = value,
                        });
                    }
                })
                .ToArray();
        }

        private static string Escape(string text)
        {
            return text.Replace("$", "$$");
        }

        private static string Unescape(string text)
        {
            return string.Create(text.Length, text.Concat(Enumerable.Repeat('\0', 5)).ToArray(), (result, original) =>
              {
                  int j = 0;
                  for (int i = 0; i < result.Length; i++)
                  {
                      if (original[i] == '\\')
                      {
                          switch (original[i + 1])
                          {
                              case 'a': result[j] = '\a'; j++; i++; continue;
                              case 'b': result[j] = '\b'; j++; i++; continue;
                              case 't': result[j] = '\t'; j++; i++; continue;
                              case 'r': result[j] = '\r'; j++; i++; continue;
                              case 'v': result[j] = '\v'; j++; i++; continue;
                              case 'f': result[j] = '\f'; j++; i++; continue;
                              case 'n': result[j] = '\n'; j++; i++; continue;
                              case 'e': result[j] = '\u001B'; j++; i++; continue;
                              case 'u':
                                  string code = string.Create(4, i + 2, (r, s) =>
                                     {
                                         for (int k = 0; k < 4; k++)
                                         {
                                             r[k] = original[s + k];
                                         }
                                     });
                                  if (UnicodeEscapeRegex.IsMatch(code))
                                  {
                                      result[j] = Convert.ToChar(Convert.ToInt32(code, 16));
                                      j++;
                                      i += 5;
                                      continue;
                                  }
                                  break;
                              case '\\': result[j] = '\\'; j++; i++; continue;
                          }
                      }

                      result[j] = original[i];
                      j++;
                  }
              }).TrimEnd('\0');
        }

        public string Replace(string text)
        {
            text = text.RemoveAll('\r');

            for (int i = 0; i < SearchRegices.Length; i++)
            {
                Replacement[] replacements = new Replacement[0];
                if (i < Replacements.Length)
                {
                    replacements = Replacements[i];
                }

                int index = 0;
                Match match = SearchRegices[i].Match(text);
                StringBuilder stringBuilder = new StringBuilder();

                while (match.Success)
                {
                    stringBuilder.Append(text.Substring(index, match.Index - index));

                    foreach (Replacement replacement in replacements)
                    {
                        string result = match.Result(replacement.Value);
                        if (!result.Any())
                        {
                            continue;
                        }

                        switch (replacement.Type)
                        {
                            case ReplacementType.LowerFirst:
                                result = result.ToLowerFirstInvariant();
                                break;
                            case ReplacementType.Lower:
                                result = result.ToLowerInvariant();
                                break;
                            case ReplacementType.UpperFirst:
                                result = result.ToUpperFirstInvariant();
                                break;
                            case ReplacementType.Upper:
                                result = result.ToUpperInvariant();
                                break;
                        }
                        stringBuilder.Append(result);
                    }

                    index = match.Index + match.Length;
                    match = match.NextMatch();
                }

                stringBuilder.Append(text.Substring(index));

                text = stringBuilder.ToString();
            }

            return text;
        }
    }
}
