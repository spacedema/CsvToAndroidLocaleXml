namespace CsvToAndroidLocaleXml.Model
{
    public class LocaleItem
    {
        private LocaleItem()
        {
            
        }

        public LocaleItem(string id, string value)
        {
            Id = id;
            Value = value;
        }

        public string Id { get; }
        public string Value { get; }
    }
}