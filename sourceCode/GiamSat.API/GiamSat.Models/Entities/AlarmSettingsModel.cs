using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GiamSat.Models
{
    [Table("AlarmSettings")]
    public class AlarmSettingsModel
    {
        [Key]
        public Guid Id { get; set; }

        public string? EmailsString { get; set; }

        public string? CCString { get; set; }

        public bool? Enabled { get; set; } = true;


        public string? Host { get; set; } = "smtp.gmail.com";


        public int? Port { get; set; } = 587;


        public bool? EnableSSL { get; set; } = true;


        public string? CredentialEmail { get; set; }

        public string? CredentialPassword { get; set; }

        public int? Timeout { get; set; } = 100000;        

        public IEnumerable<string> GetEmails()
        {
            string[] array = EmailsString?.Split(',');
            for (int i = 0; i < array.Length; i++)
            {
                yield return array[i];
            }
        }

        public IEnumerable<string> GetCC()
        {
            string[] array = CCString?.Split(',');
            for (int i = 0; i < array.Length; i++)
            {
                yield return array[i];
            }
        }

        public DateTime? CreatedDate { get; set; } = DateTime.Now;
        public bool? IsActived { get; set; } = true;
    }
}
