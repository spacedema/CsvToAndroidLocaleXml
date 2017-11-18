using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using CsvToAndroidLocaleXml.Model;

namespace CsvToAndroidLocaleXml
{
    class Program
    {
        public static char Separator = ',';
        static void Main(string[] args)
        {
            if (!args.Any())
                throw new ArgumentNullException(nameof(args));

            var allLines = File.ReadAllLines(args[0]);
            if (allLines.Length < 2)
            {
                Console.Write("File is empty");
                return;
            }

            InitSeparator(args);

            var locales = new Dictionary<int, Locale>();
            var firstOrDefault = allLines.FirstOrDefault();
            if (firstOrDefault == null)
            {
                Console.Write("File is empty");
                return;
            }

            allLines = allLines.Select(x => x.Replace("\"\"\"", "\"")).ToArray();

            var headers = firstOrDefault.Split(Separator);
            for (var i = 1; i < headers.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(headers[i]))
                {
                    Console.Write("Header is empty or whitespace");
                    return;
                }

                locales.Add(i, new Locale(headers[i]));
            }

            FillLocales(locales, allLines, headers);
            ExportToXml(locales); 
        } 

        private static void FillLocales(IReadOnlyDictionary<int, Locale> locales, IReadOnlyList<string> allLines, IReadOnlyCollection<string> headers)
        {
            for (var i = 1; i < allLines.Count; i++)
            {
                var line = allLines[i];
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var values = IsSimpleLine(line) ? line.Split(Separator) : ProcessComplexLine(allLines, headers.Count, ref i);
                for (var j = 1; j < values.Count; j++)
                    locales[j].Items.Add(new LocaleItem(values[0], values[j]));
            }
        }

        private static void ExportToXml(Dictionary<int, Locale> locales)
        {
            foreach (var locale in locales.Values)
            {
                var rootNode = new XElement("resources");
                foreach (var localeItem in locale.Items)
                {
                    var entry = new XElement("string", new XAttribute("name", localeItem.Id))
                    {
                        Value = localeItem.Value,
                    };
                    rootNode.Add(entry);
                }
                var xmlDoc = new XDocument();
                xmlDoc.Add(rootNode);
                xmlDoc.Save(locale.Name + ".xml");
            }
        }

        private static bool IsSimpleLine(string line)
        {
            if (line.Contains(",\"") || line.Contains("\",") || line.StartsWith("\""))
                return false;

            return true;
        }

        private static void InitSeparator(IReadOnlyList<string> args)
        {
            if (args.Count < 2)
                return;

            Separator = Convert.ToChar(args[1]);
        }

        private static IList<string> ProcessComplexLine(IReadOnlyList<string> allLines, int valuesCount, ref int i)
        {
            var values = new List<string>();
            var line = allLines[i];
            var tempValue = string.Empty;

            while (values.Count < valuesCount)
            {
                var res = GetValue(line, string.IsNullOrEmpty(tempValue));
                if (res.Item2)
                {
                    if (string.IsNullOrEmpty(tempValue))
                        values.Add(res.Item1);
                    else
                    {
                        values.Add(tempValue + res.Item1);
                        tempValue = string.Empty;
                    }
                    line = line.Remove(0, values.Count == valuesCount ? res.Item1.Length : res.Item1.Length + 1);
                }
                else
                {
                    i++;
                    line = allLines[i];
                    tempValue += res.Item1 + Environment.NewLine;
                }
            }

            return values;
        }

        private static Tuple<string, bool> GetValue(string line, bool searchingForNewValue)
        {
            if (line.StartsWith("\"") || !searchingForNewValue)
            {
                if (string.IsNullOrWhiteSpace(line))
                    return new Tuple<string, bool>(line, false);

                var indexOfQuote = line.IndexOf("\"", searchingForNewValue ? 1 : 0, StringComparison.Ordinal);
                if (indexOfQuote == -1)
                    return new Tuple<string, bool>(line, false);

                return new Tuple<string, bool>(line.Substring(0, indexOfQuote + 1), true);
            }

            var indexOfSeparator = line.IndexOf(Separator.ToString(), 0, StringComparison.Ordinal);
            if (indexOfSeparator == -1)
                return new Tuple<string, bool>(line, true);

            return new Tuple<string, bool>(line.Substring(0, indexOfSeparator), true);
        }
    }
}
