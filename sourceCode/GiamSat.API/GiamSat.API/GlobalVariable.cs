using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GiamSat.API
{
    public static class GlobalVariable
    {
        public static string ConString { get; set; }

        public static IDbConnection GetDbProvider()
        {
            return new SqlConnection(ConString);
        }
    }
}
