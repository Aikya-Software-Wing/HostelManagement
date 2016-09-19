using System.Web.Mvc;

namespace HostelManagement.Areas.HostelMessManagement
{
    /// <summary>
    /// Class to register the hostel mess management area
    /// </summary>
    public class HostelMessManagementAreaRegistration : AreaRegistration 
    {
        /// <summary>
        /// The name of the area
        /// </summary>
        public override string AreaName 
        {
            get 
            {
                return "HostelMessManagement";
            }
        }

        /// <summary>
        /// Method to register the area
        /// </summary>
        /// <param name="context">the context</param>
        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "HostelMessManagement_default",
                "HostelMessManagement/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "HostelManagement.Areas.HostelMessManagement.Controllers" }
            );
        }
    }
}