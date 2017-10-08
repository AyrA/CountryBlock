using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace CountryBlock
{
    /// <summary>
    /// API Functions
    /// </summary>
    public static class API
    {
#pragma warning disable 0649
        /// <summary>
        /// Basic API Response Layout
        /// </summary>
        private struct ApiResponse
        {
            /// <summary>
            /// Success
            /// </summary>
            public bool success;
            /// <summary>
            /// Mode specific Data (null on error)
            /// </summary>
            public object data;
        }
#pragma warning restore 0649

        /// <summary>
        /// Gets all Countries
        /// </summary>
        /// <returns>Country Array</returns>
        public static Country[] GetCountries()
        {
            var Response = GetResponse("countries");
            if (Response.success)
            {
                try
                {
                    return JsonConvert.DeserializeObject<Dictionary<string, string>>(Response.data.ToString()).Select(m => new Country() { Code = m.Key, Name = m.Value }).ToArray();
                }
                catch
                {
                }
            }
            return null;
        }

        /// <summary>
        /// Gets all Addresses from a Country
        /// </summary>
        /// <param name="C">Country</param>
        /// <returns>IP Array</returns>
        public static string[] GetAddresses(Country C)
        {
            return GetAddresses(C.Code);
        }

        /// <summary>
        /// Gets all Addresses from a Country
        /// </summary>
        /// <param name="CountryCode">Country</param>
        /// <returns>IP Array</returns>
        public static string[] GetAddresses(string CountryCode)
        {
            var Params = new Dictionary<string, string>();
            Params["c"] = CountryCode;
            var Response = GetResponse("country", Params);
            if (Response.success)
            {
                try
                {
                    return JsonConvert.DeserializeObject<string[]>(Response.data.ToString());
                }
                catch
                {
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the Full Cache from the API
        /// </summary>
        /// <returns></returns>
        public static FullCountry[] GetCache()
        {
            var D = new Dictionary<string, string>();
            D["v"] = "4";
            var Response4 = GetResponse("all", D);
            D["v"] = "6";
            var Response6 = GetResponse("all", D);

            FullCountry[] Countries4;
            FullCountry[] Countries6;
            List<FullCountry> Ret=new List<FullCountry>();

            try
            {
                Countries4 = JsonConvert.DeserializeObject<FullCountry[]>(Response4.data.ToString());
                Countries6 = JsonConvert.DeserializeObject<FullCountry[]>(Response6.data.ToString());
            }
            catch
            {
                return null;
            }
            //Loop through all country codes
            foreach (var Code in Countries4.Select(m => m.code).Concat(Countries6.Select(m => m.code)).Distinct())
            {
                //Get V4 Addresses
                var C4 = Countries4.FirstOrDefault(m => m.code == Code);
                //Get V6 Addresses
                var C6 = Countries6.FirstOrDefault(m => m.code == Code);
                //Check if V4 was found
                if (C4.code != null)
                {
                    //Check if V6 was found
                    if (C6.code != null)
                    {
                        //Both found, simplify and concatenate
                        C4.addr= C4.addr.Concat(C6.addr.Select(m => SimplifyV6(m))).ToArray();
                    }
                    //Add possibly extended V4 Entry
                    Ret.Add(C4);
                }
                else
                {
                    //V4 does not exists, just simplify V6 addresses and add
                    Ret.Add(SimplifyV6(C6));
                }
            }
            //Return combined and sorted list
            return Ret.OrderBy(m => m.code).ToArray();
        }

        /// <summary>
        /// Shortens IPv6 Notation by replacing zero segments with ::
        /// </summary>
        /// <param name="Entry">Country</param>
        /// <returns>Country with Shortened IP Address masks</returns>
        private static FullCountry SimplifyV6(FullCountry Entry)
        {
            Entry.addr = Entry.addr.Select(m => SimplifyV6(m)).ToArray();
            return Entry;
        }

        /// <summary>
        /// Shortens IPv6 Notation by replacing zero segments with ::
        /// </summary>
        /// <param name="Entry">IP with optional mask</param>
        /// <returns>Shortened IP Address</returns>
        private static string SimplifyV6(string Entry)
        {
            var Mask = Entry.IndexOf('/');
            if (Mask >= 0)
            {
                return IPAddress.Parse(Entry.Substring(0, Mask)).ToString() + "/" + Entry.Substring(Mask + 1);
            }
            else
            {
                return IPAddress.Parse(Entry).ToString();
            }
        }

        /// <summary>
        /// Gets Generic API Response
        /// </summary>
        /// <param name="Mode">API Mode</param>
        /// <param name="Params">Optional Mode Parameters</param>
        /// <returns>API Response</returns>
        private static ApiResponse GetResponse(string Mode, IDictionary<string, string> Params = null)
        {
            var URL = $"https://cable.ayra.ch/ip/api.php?mode={Uri.EscapeDataString(Mode)}";
            if (Params != null)
            {
                URL += "&" + string.Join("&", Params.Select(m => $"{Uri.EscapeDataString(m.Key)}={Uri.EscapeDataString(m.Value)}"));
            }

            var Req = WebRequest.Create(URL);
            WebResponse Res = null;
            try
            {
                Res = Req.GetResponse();
            }
            catch
            {
                return new ApiResponse() { success = false };
            }

            using (Res)
            {
                using (var SR = new StreamReader(Res.GetResponseStream()))
                {
                    try
                    {
                        return JsonConvert.DeserializeObject<ApiResponse>(SR.ReadToEnd());
                    }
                    catch
                    {
                        return new ApiResponse() { success = false };
                    }
                }
            }
        }
    }
}
