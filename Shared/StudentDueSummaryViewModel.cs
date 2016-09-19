namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model for student fee due summary
    /// </summary>
    public class StudentDueSummaryViewModel
    {
        /// <summary>
        /// The border ID of the student
        /// </summary>
        public string bid { get; set; }

        /// <summary>
        /// Rent
        /// </summary>
        public decimal rent { get; set; }

        /// <summary>
        /// Fixed charges
        /// </summary>
        public decimal fix { get; set; }

        /// <summary>
        /// Security deposit
        /// </summary>
        public decimal deposit { get; set; }

        /// <summary>
        /// Other charges
        /// </summary>
        public decimal other { get; set; }
    }
}