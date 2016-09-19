using System.Web.Mvc;

namespace HostelManagement.Areas.Administration
{
    /// <summary>
    /// Class to register the admin area
    /// </summary>
    public class AdministrationAreaRegistration : AreaRegistration 
    {
        /// <summary>
        /// The name of the area
        /// </summary>
        public override string AreaName 
        {
            get 
            {
                return "Administration";
            }
        }

        /// <summary>
        /// Method to register the area
        /// </summary>
        /// <param name="context">the context</param>
        public override void RegisterArea(AreaRegistrationContext context) 
        {
            context.MapRoute(
                "Administration_default",
                "Administration/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional },
                new[] { "HostelManagement.Areas.Administration.Controllers" }
            );
        }
    }
}