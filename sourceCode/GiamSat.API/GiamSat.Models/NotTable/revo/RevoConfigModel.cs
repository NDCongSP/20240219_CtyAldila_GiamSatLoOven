using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public class RevoConfigs : List<RevoConfigModel>
    {

    }

    public class RevoConfigModel
    {
        public int? Id { get; set; }

        public string? Name { get; set; }

        public string? Path { get; set; }

        public string? ConstringAccessDb { get; set; }
    }
}
