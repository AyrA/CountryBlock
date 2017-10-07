namespace CountryBlock
{
    /// <summary>
    /// Full Country Entry for Cache
    /// </summary>
    public struct FullCountry
    {
        /// <summary>
        /// Country Code
        /// </summary>
        public string code;
        /// <summary>
        /// Country Name
        /// </summary>
        public string name;
        /// <summary>
        /// Address List
        /// </summary>
        public string[] addr;
    }

    /// <summary>
    /// Simple Country Entry for Firewall
    /// </summary>
    public struct Country
    {
        /// <summary>
        /// Country Code
        /// </summary>
        public string Code;
        /// <summary>
        /// Country Name
        /// </summary>
        public string Name;

        /// <summary>
        /// String Representation
        /// </summary>
        /// <returns>Code=Name</returns>
        public override string ToString()
        {
            return $"{Code}={Name}";
        }
    }
}
