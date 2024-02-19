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
        public const string Test = "test";
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

        public static class ChuongInfo
        {
            public const string BasePath = "api/ChuongInfo";
            public const string Test = "test";
            public const string GetByName = "GetByName/{id}";
        }
        public static class FT100
        {
            public const string BasePath = "api/FT100";
        }
        public static class FT101
        {
            public const string BasePath = "api/FT101";
        }

        public static class AlarmSettings
        {
            public const string BasePath = "api/AlarmSettings";
        }

        public static class AlarmLog
        {
            public const string BasePath = "api/AlarmLog";
        }
    }
}
