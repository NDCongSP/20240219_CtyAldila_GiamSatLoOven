using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    public class OvenInfoModel
    {
        /// <summary>
        /// Id của lò Oven, từ 1-->13
        /// </summary>
        public int Id { get; set; }
        public string Name { get; set; }
        public List<ProfileModel> Profiles { get; set; }
    }
}
