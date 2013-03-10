namespace sync.remotefilestore.parse.api
{
    internal struct ParseFileInfo
    {
        public string Url;
        public string Name;

        public static ParseFileInfo Parse(string text)
        {
            var parts = text.Split('#');
            return new ParseFileInfo
                {
                    Url = parts[0],
                    Name = parts[1]
                };
        }

        public override string ToString()
        {
            return string.Format("{0}#{1}", Url, Name);
        }
    }
}