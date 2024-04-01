using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public class RefreshTokenModel
    {
        public string OldToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
