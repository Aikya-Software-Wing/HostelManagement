using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostelManagement.Areas.Administration.Models
{
    public class AddHostelViewModel
    {
        [Display(Name ="Block Number")]
        [Required]
        public int blockNumber { get; set; }

        [Display(Name ="Description")]
        [Required]
        public string type { get; set; }

        [Display(Name ="Occupant Type")]
        [Required]
        public int occupantType { get; set; }
    }
}
