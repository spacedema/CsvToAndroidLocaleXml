using System.Collections.Generic;

namespace CsvToAndroidLocaleXml.Model
{
    public class Locale
    {
        public string Name { get; }

        public List<LocaleItem> Items;

        private Locale()
        {
            
        }

        public Locale(string name)
        {
            Name = name;
            Items = new List<LocaleItem>();
        }
    }
}