using System.Collections.Generic;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model for student mess due summary
    /// </summary>
    public class StudentMessDueSummaryViewModel
    {
        /// <summary>
        /// the border ID of the student
        /// </summary>
        public string bid { get; set; }
        
        /// <summary>
        /// A list of mess fee dues
        /// </summary>
        public List<MessFeeDueViewModel> dues { get; set; }
    }
}