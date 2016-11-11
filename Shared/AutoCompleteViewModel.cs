namespace Shared
{
    /// <summary>
    /// View model for auto complete
    /// </summary>
    public class AutoCompleteViewModel
    {
        /// <summary>
        /// The value that was entered by the user
        /// </summary>
        public string value { get; set; }

        /// <summary>
        /// The label to be displayed
        /// </summary>
        public string label { get; set; }

        /// <summary>
        /// The department that the cadidate student belongs to
        /// </summary>
        public string dept { get; set; }

        /// <summary>
        /// The room number of the potential student
        /// </summary>
        public string room { get; set; }

        /// <summary>
        /// The usn of the student
        /// </summary>
        public string usn { get; set; }
    }
}
