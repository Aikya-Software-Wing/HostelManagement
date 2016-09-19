using System.Web.Mvc;

namespace HostelManagement.Areas.Archive
{
    /// <summary>
    /// Class to register the archive area
    /// </summary>
    public class ArchiveAreaRegistration : AreaRegistration 
    {
        /// <summary>
        /// The name of the area
        /// </summary>
        public override string AreaName 
        {
            get 
            {
                return "Archive";
            }
        }

        /// <summary>
        /// Method to register the area
        /// </summary>
        /// <param name="context">the context</param>
        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Archive_default",
                "Archive/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "HostelManagement.Areas.Archive.Controllers" }
            );
        }
    }
}