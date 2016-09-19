using System;

namespace HostelManagement.Areas.HostelMessManagement.Models
{
    /// <summary>
    /// View model for transactions
    /// </summary>
    public class TransactionsViewModel
    {
        /// <summary>
        /// The transaction ID
        /// </summary>
        public string id { get; set; }

        /// <summary>
        /// The border ID of the student
        /// </summary>
        public string bid { get; set; }

        /// <summary>
        /// The date of payment
        /// </summary>
        public DateTime dateOfPay { get; set; }

        /// <summary>
        /// The payment type, that is, NEFT, challan and so on
        /// </summary>
        public string paymentType { get; set; }

        /// <summary>
        /// The account head, that is, rent, fixed deposit and so on
        /// </summary>
        public string accountHead { get; set; }

        /// <summary>
        /// The name of the bank
        /// </summary>
        public string bankName { get; set; }

        /// <summary>
        /// The academic year
        /// </summary>
        public string academicYear { get; set; }

        /// <summary>
        /// The amount
        /// </summary>
        public decimal amount { get; set; }

        /// <summary>
        /// The transaction refrence
        /// </summary>
        public string transaction { get; set; }
    }
}
