namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model for hostel fee dues
    /// </summary>
    public class HostelFeeDueViewModel
    {
        /// <summary>
        /// The academic year
        /// </summary>
        public int academicYear { get; set; }

        /// <summary>
        /// The account head, that is, rent, fixed charges and so on
        /// </summary>
        public string accountHead { get; set; }
        
        /// <summary>
        /// The amount of the said fee that the student has paid so far
        /// </summary>
        public decimal amountPaid { get; set; }

        /// <summary>
        /// The amount of fee expected
        /// </summary>
        public decimal amount { get; set; }

        /// <summary>
        /// The amount due
        /// </summary>
        public decimal amountDue { get; set; }
    }
}