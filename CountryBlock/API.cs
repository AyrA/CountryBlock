using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace CountryBlock
{
    public static class API
    {
        #pragma warning disable 0649
        private struct ApiResponse
        {
            public bool success;
            public object data;
        }
        #pragma warning restore 0649

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

        public static string[] GetAddresses(Country C)
        {
            return GetAddresses(C.Code);
        }

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

        public static FullCountry[] GetCache()
        {
            var Response = GetResponse("all");
            try
            {
                return JsonConvert.DeserializeObject<FullCountry[]>(Response.data.ToString());
            }
            catch
            {
            }
            return null;
        }

        private static ApiResponse GetResponse(string Mode, IDictionary<string, string> Params = null)
        {
            var URL = $"https://cable.ayra.ch/ip/api.php?mode={Uri.EscapeDataString(Mode)}";
            if (Params != null)
            {
                URL += "&" + string.Join("&", Params.Select(m => $"{Uri.EscapeDataString(m.Key)}={Uri.EscapeDataString(m.Value)}"));
            }

            var Req = WebRequest.CreateHttp(URL);
            HttpWebResponse Res = null;
            try
            {
                Res = (HttpWebResponse)Req.GetResponse();
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
