using System.Collections.Generic;

namespace GiamSat.Models
{
    public class ProfileModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<StepModel> Steps { get; set; }
    }
}