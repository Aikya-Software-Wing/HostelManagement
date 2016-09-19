using System.Web.Mvc;

namespace HostelManagement
{
    /// <summary>
    /// a class for filter configuration
    /// </summary>
    public class FilterConfig
    {
        /// <summary>
        /// Method to register the filters
        /// </summary>
        /// <param name="filters">the filters</param>
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
            filters.Add(new AuthorizeAttribute());
        }
    }
}