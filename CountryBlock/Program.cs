using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CountryBlock
{
    public class Program
    {
        public static string CurrentDir { get; private set; }

        public static string CacheFile
        {
            get
            {
                return Path.Combine(CurrentDir, "cache.json");
            }
        }

        public static FullCountry[] Cache
        {
            get
            {
                try
                {
                    return File.Exists(CacheFile) ? JsonConvert.DeserializeObject<FullCountry[]>(File.ReadAllText(CacheFile)) : new FullCountry[0];
                }
                catch
                {
                    return null;
                }
            }
            private set
            {
                if (value != null && value.Length > 0)
                {
                    File.WriteAllText(CacheFile, JsonConvert.SerializeObject(value));
                }
                else
                {
                    File.Delete(CacheFile);
                }
            }
        }

        static Program()
        {
            CurrentDir = (new FileInfo(Process.GetCurrentProcess().MainModule.FileName)).Directory.FullName;
        }

        static int Main(string[] args)
        {
            if (args.Contains("/PANIC"))
            {
                Console.Error.WriteLine("PANIC MODE: REMOVING ALL COUNTRYBLOCK RULES");
                var CList = Firewall.GetBlockedCountries().Select(m => m.Key).ToArray();
                foreach (var Code in CList)
                {
                    Console.Error.WriteLine("PANIC MODE: REMOVING {0}", Code);
                    Firewall.UnblockCountry(Code);
                }
                Console.Error.WriteLine("PANIC MODE: ALL COUNTRYBLOCK RULES REMOVED!");
                return 0;
            }

            if (args.Length == 0 || args.Contains("/?"))
            {
                ShowHelp();
            }
            else
            {
                var Arg = ParseArgs(args);
                if (Arg.M == OperationMode.Invalid || Arg.M == OperationMode.None)
                {
                    return 1;
                }
                if (!File.Exists(CacheFile))
                {
                    Cache = API.GetCache();
                }

                var Country = Cache.FirstOrDefault(m => m.code == Arg.Country);

                switch (Arg.M)
                {
                    case OperationMode.AddCountry:
                        if (Country.code != null)
                        {
                            Console.WriteLine($"Blocking {Country.name}...");
                            Firewall.UnblockCountry(Country.code);
                            Firewall.BlockCountry(Country.code, Country.addr, Arg.Direction);
                            Console.WriteLine("Done");
                        }
                        else
                        {
                            Console.Error.WriteLine("Invalid Country Code");
                            return 1;
                        }
                        break;
                    case OperationMode.RemoveCountry:
                        if (Country.code != null)
                        {
                            Console.WriteLine($"Removing Country Rules for {Country.name}...");
                            Firewall.UnblockCountry(Country.code, Arg.Direction);
                            Console.WriteLine("Done");
                        }
                        else
                        {
                            if (Arg.Country == "ALL")
                            {
                                var List = Firewall.GetBlockedCountries().Select(m => m.Key);
                                foreach (var C in List)
                                {
                                    Console.WriteLine($"Removing Country Rules for {C}...");
                                    Firewall.UnblockCountry(C, Arg.Direction);
                                }
                                Console.WriteLine("Done");
                            }
                            else
                            {
                                Console.Error.WriteLine("Invalid Country Code");
                                return 1;
                            }
                        }
                        break;
                    case OperationMode.ListCountries:
                        Console.WriteLine(string.Join("\n", Cache.Select(m => $"{m.code}={m.name}")));
                        break;
                    case OperationMode.ListRules:
                        Console.WriteLine(string.Join("\n", Firewall.GetBlockedCountries().Select(m => $"{m.Key}={m.Value}")));
                        break;
                    case OperationMode.ShowAddresses:
                        if (Country.code != null)
                        {
                            Console.WriteLine(string.Join("\n", Country.addr));
                        }
                        else
                        {
                            Console.Error.WriteLine("Invalid Country Code");
                            return 1;
                        }
                        break;
                }

            }
#if DEBUG
            Console.Error.WriteLine("#END");
            Console.ReadKey(true);
#endif
            return 0;
        }

        private static Operation ParseArgs(string[] Args)
        {
            Operation Ret = new Operation();
            Ret.M = OperationMode.None;
            Ret.Direction = Firewall.Direction.Both;

            for (int i = 0; i < Args.Length && Ret.M != OperationMode.Invalid; i++)
            {
                switch (Args[i].ToLower())
                {
                    case "/add":
                        if (i < Args.Length - 1)
                        {
                            Ret.M = OperationMode.AddCountry;
                            Ret.Country = Args[++i].ToUpper();
                            if (Ret.Country == "ALL")
                            {
                                Console.Error.WriteLine("/add ALL is a stupid idea");
                                Ret.M = OperationMode.Invalid;
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("/add requires a country code");
                            Ret.M = OperationMode.Invalid;
                        }
                        break;
                    case "/remove":
                        if (Ret.M == OperationMode.None)
                        {
                            if (i < Args.Length - 1)
                            {
                                Ret.M = OperationMode.RemoveCountry;
                                Ret.Country = Args[++i].ToUpper();
                            }
                            else
                            {
                                Console.Error.WriteLine("/remove requires a country code or 'ALL'");
                                Ret.M = OperationMode.Invalid;
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("Invalid Operation mode combination");
                            Ret.M = OperationMode.Invalid;
                        }
                        break;
                    case "/addr":
                        if (Ret.M == OperationMode.None)
                        {
                            if (i < Args.Length - 1)
                            {
                                Ret.M = OperationMode.ShowAddresses;
                                Ret.Country = Args[++i].ToUpper();
                                if (Ret.Country == "ALL")
                                {
                                    Console.Error.WriteLine("/addr ALL is a stupid idea. Just open the cache file in a text editor instead");
                                    Ret.M = OperationMode.Invalid;
                                }
                            }
                            else
                            {
                                Console.Error.WriteLine("/addr requires a country code");
                                Ret.M = OperationMode.Invalid;
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("Invalid Operation mode combination");
                            Ret.M = OperationMode.Invalid;
                        }
                        break;
                    case "/countries":
                        if (Ret.M == OperationMode.None)
                        {
                            Ret.M = OperationMode.ListCountries;
                        }
                        else
                        {
                            Console.Error.WriteLine("Invalid Operation mode combination");
                            Ret.M = OperationMode.Invalid;
                        }
                        break;
                    case "/rules":
                        if (Ret.M == OperationMode.None)
                        {
                            Ret.M = OperationMode.ListRules;
                        }
                        else
                        {
                            Console.Error.WriteLine("Invalid Operation mode combination");
                            Ret.M = OperationMode.Invalid;
                        }
                        break;
                    case "/dir":
                        if (i < Args.Length - 1)
                        {
                            switch (Args[++i].ToUpper())
                            {
                                case "IN":
                                    Ret.Direction = Firewall.Direction.In;
                                    break;
                                case "OUT":
                                    Ret.Direction = Firewall.Direction.Out;
                                    break;
                                case "BOTH":
                                    Ret.Direction = Firewall.Direction.Both;
                                    break;
                                default:
                                    Console.Error.WriteLine("Invalid direction. Use only 'in' or 'out'");
                                    Console.Error.WriteLine("Don't specify a direction to use both");
                                    Ret.M = OperationMode.Invalid;
                                    break;
                            }
                        }
                        else
                        {
                            Console.Error.WriteLine("/remove requires a country");
                            Ret.M = OperationMode.Invalid;
                        }
                        break;
                    default:
                        Console.Error.WriteLine("Invalid Argument: {0}", Args[i]);
                        Ret.M = OperationMode.Invalid;
                        break;
                }
            }
            return Ret;
        }

        private static void ShowHelp()
        {
            Console.Error.WriteLine(@"CountryBlock.exe {/add|/remove|/addr} <country> [/dir {in|out}] | /countries | /rules
Blocks entire countries via Windows Firewall

/add         - Adds the specified country to the list
/remove      - Removes the specified country from the list
               (use 'ALL' to remove all)
/addr        - Shows all addresses of the specified country
/countries   - Lists all countries
/rules       - Lists all blocked countries
country      - Country to (un-)block (2 letter country code)
/dir in|out  - Direction of connection to block.
               In most cases you only want 'in'
               Not specifying any direction (un-)blocks both.
               Only has an effect on /add and /remove command.

WARNING
=======
It is very easy to block addresses you need.
Blocking 'CH' will also block the API, when blocking '__' you will also block
local addresses and thus render your network connections defunct after a
short period. You can restore this wih '/remove all' if you need to.
Please read the readme file carefully.");
        }

        private enum OperationMode
        {
            None,
            Invalid,
            AddCountry,
            RemoveCountry,
            ListRules,
            ShowAddresses,
            ListCountries
        }

        private struct Operation
        {
            public OperationMode M;
            public string Country;
            public Firewall.Direction Direction;
        }
    }
}
