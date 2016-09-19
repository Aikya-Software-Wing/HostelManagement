namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model for mess fee dues
    /// </summary>
    public class MessFeeDueViewModel
    {
        /// <summary>
        /// Academic year
        /// </summary>
        public int academicYear { get; set; }

        /// <summary>
        /// The month in question
        /// </summary>
        public int month { get; set; }

        /// <summary>
        /// The number of days that the student was present
        /// </summary>
        public int numberOfDays { get; set; }

        /// <summary>
        /// The amount to be paid
        /// </summary>
        public decimal amount { get; set; }

        /// <summary>
        /// The amount that the student has actually paid
        /// </summary>
        public decimal amountPaid { get; set; }

        /// <summary>
        /// The amount that is due
        /// </summary>
        public decimal amountDue { get; set; }
    }
}