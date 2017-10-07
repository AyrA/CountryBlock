using NetFwTypeLib;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CountryBlock
{
    public static class Firewall
    {
        /// <summary>
        /// Firewall Rule Direction
        /// </summary>
        /// <remarks>
        /// For TCP, the direction is only applied for the Connection Initiation.
        /// Blocking Inbound still allows you to connect to any system and receive
        /// a Response but it prevents them from starting a Connection
        /// </remarks>
        [Flags]
        public enum Direction
        {
            /// <summary>
            /// Inbound
            /// </summary>
            In = 1,
            /// <summary>
            /// Outbound
            /// </summary>
            Out = 2,
            /// <summary>
            /// Both Directions
            /// </summary>
            Both = In | Out
        }

        /// <summary>
        /// Rule Name Prefix
        /// </summary>
        public const string BLOCK = "CountryBlock";

        /// <summary>
        /// Gets the Firewall Policy Object
        /// </summary>
        /// <returns></returns>
        private static INetFwPolicy2 GetPolicy()
        {
            return (INetFwPolicy2)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwPolicy2"));
        }

        /// <summary>
        /// Unblocks A Country
        /// </summary>
        /// <param name="C">Country</param>
        /// <param name="D">Direction to Unblock</param>
        public static void UnblockCountry(Country C, Direction D = Direction.Both)
        {
            UnblockCountry(C.Code, D);
        }

        /// <summary>
        /// Unblocks A Country
        /// </summary>
        /// <param name="CountryCode">Country</param>
        /// <param name="D">Direction to Unblock</param>
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

        /// <summary>
        /// Blocks A Country
        /// </summary>
        /// <param name="C">Country</param>
        /// <param name="IPList">IP Address List to Block</param>
        /// <param name="D">Direction to Block</param>
        /// <remarks>This first completely unblocks said Country</remarks>
        public static void BlockCountry(Country C, string[] IPList, Direction D = Direction.Both)
        {
            BlockCountry(C.Code, IPList, D);
        }

        /// <summary>
        /// Blocks A Country
        /// </summary>
        /// <param name="CountryCode">Country Code</param>
        /// <param name="IPList">IP Address List to Block</param>
        /// <param name="D">Direction to Block</param>
        /// <remarks>This first completely unblocks said Country</remarks>
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

        /// <summary>
        /// Gets All blocked Countries and the Direction they are blocked
        /// </summary>
        /// <returns>Dictionary with Country Codes and Block Direction</returns>
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
