using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    public class RemoveStudentViewModel : DisplayStudentViewModel
    {
        [Required]
        [Display(Name = "Border ID")]
        public string bid { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal rentRefund { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal fixRefund { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public decimal depRefund { get; set; }

        public string rentRefundRef { get; set; }

        public string fixRefundRef { get; set; }
        
        public string depRefundRef { get; set; }
    }
}
