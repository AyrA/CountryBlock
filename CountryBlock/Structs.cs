namespace CountryBlock
{
    public struct FullCountry
    {
        public string code;
        public string name;
        public string[] addr;
    }

    public struct Country
    {
        public string Code;
        public string Name;

        public override string ToString()
        {
            return $"{Code}={Name}";
        }
    }
}
