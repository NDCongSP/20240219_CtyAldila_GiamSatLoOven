using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public static class ApiRoutes
    {
        public const string GetById = "{id}";
        public const string GetAll = "";
        /// <summary>
        /// HttpPost.
        /// </summary>
        public const string Update = "update";
        /// <summary>
        /// HttpPost.
        /// </summary>
        public const string Insert = "insert";

        public static class Token
        {
            public static string Get = "api/identity/token";
            public static string Refresh = "api/identity/token/refresh";
        }
        public static class DisplayRealtime
        {
            public const string BasePath = "api/DisplayRealtime";
            public const string enUpdateTChuong = "updatetenchuong";
        }

        public static class RealtimeDisplay
        {
            public const string BasePath = "api/RealtimeDisplay";
            public const string GetTop1 = "GetTop1";
        }

        public static class DataLog
        {
            public const string BasePath = "api/DataLog";
            public const string GetFromToByName = "GetByName/{from}/{to}/{tenChuong}";
        }


        public static class FT01
        {
            public const string BasePath = "api/FT01";
        }
        public static class FT02
        {
            public const string BasePath = "api/FT02";
        }
        public static class FT03
        {
            public const string BasePath = "api/FT03";
            public const string GetFilter = "GetFilter";
        }
        public static class FT04
        {
            public const string BasePath = "api/FT04";
            public const string GetFilter = "GetFilter";
        }
        public static class FT05
        {
            public const string BasePath = "api/FT05";
            public const string GetFilter = "GetFilter";
        }

        public static class FT06
        {
            public const string BasePath = "api/FT06";
        }
    }
}
