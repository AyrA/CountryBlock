using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CountryBlock
{
    public static class Firewall
    {
        [Flags]
        public enum Direction
        {
            In = 1,
            Out = 2,
            Both = In | Out
        }

        public const string BLOCK = "CountryBlock";

        private static INetFwPolicy2 GetPolicy()
        {
            return (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
        }

        public static void UnblockCountry(Country C, Direction D = Direction.Both)
        {
            UnblockCountry(C.Code, D);
        }

        public static void UnblockCountry(string CountryCode, Direction D = Direction.Both)
        {
            var Policy = GetPolicy();
            if (D == Direction.Both)
            {
                UnblockCountry(CountryCode, Direction.In);
                UnblockCountry(CountryCode, Direction.Out);
            }
            else
            {
                var Key = $"{BLOCK}-{D}-{CountryCode}";
                var Rules = Policy.Rules.Cast<INetFwRule2>().Select(m => m.Name).ToArray();
                for (var i = 0; i < Rules.Count(m => m == Key); i++)
                {
                    Policy.Rules.Remove(Key);
                }
            }
        }

        public static void BlockCountry(Country C, string[] IPList, Direction D = Direction.Both)
        {
            BlockCountry(C.Code, IPList, D);
        }

        public static void BlockCountry(string CountryCode, string[] IPList, Direction D = Direction.Both)
        {
            if (D == Direction.Both)
            {
                BlockCountry(CountryCode, IPList, Direction.In);
                BlockCountry(CountryCode, IPList, Direction.Out);
            }
            else
            {
                var Policy = GetPolicy();
                UnblockCountry(CountryCode, D);
                for (var I = 0; I < IPList.Length; I += 1000)
                {
                    Console.Error.Write('.');
                    var R = (INetFwRule2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwRule"));

                    R.Name = $"{BLOCK}-{D}-{CountryCode.ToUpper()}";
                    R.Action = NET_FW_ACTION_.NET_FW_ACTION_BLOCK;
                    R.Description = $"Blocking All IP Addresses of the Country {CountryCode.ToUpper()}";
                    R.Direction = D == Direction.In ? NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN : NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_OUT;
                    R.Enabled = true;
                    R.InterfaceTypes = "All";
                    R.LocalAddresses = "*";
                    R.Profiles = int.MaxValue;
                    R.Protocol = 256;
                    R.RemoteAddresses = string.Join(",", IPList.Skip(I).Take(1000).ToArray());
                    Policy.Rules.Add(R);
                }
            }


        }

        public static Dictionary<string, Direction> GetBlockedCountries()
        {
            var Policies = GetPolicy()
                .Rules
                .Cast<INetFwRule2>()
                .Where(m => m.Name.StartsWith($"{BLOCK}-"))
                .Select(m => new { Code = m.Name.Split('-').Last().ToUpper(), Direction = m.Direction == NET_FW_RULE_DIRECTION_.NET_FW_RULE_DIR_IN ? Direction.In : Direction.Out })
                .ToArray();
            var Ret = new Dictionary<string, Direction>();
            foreach (var Policy in Policies)
            {
                if (!Ret.ContainsKey(Policy.Code))
                {
                    Ret.Add(Policy.Code, Policy.Direction);
                }
                else if (Ret[Policy.Code] != Policy.Direction)
                {
                    Ret[Policy.Code] = Direction.Both;
                }
            }
            return Ret;
        }

    }
}
