using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Regexer
{
    internal static class ReplacerExtension
    {
        public static async Task ReplaceFile(this Replacer replacer, string inputPath, string outputPath)
        {
            try
            {
                string text = await File.ReadAllTextAsync(inputPath);
                text = replacer.Replace(text);
                await File.WriteAllTextAsync(outputPath, text);
            }
            catch (Exception ex)
            {
                ex.Log();
            }
        }
    }
}
