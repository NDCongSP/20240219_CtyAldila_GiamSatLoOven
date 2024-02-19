using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public class LoginResult
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
    }
}
