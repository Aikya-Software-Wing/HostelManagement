using System.Web.Mvc;

namespace HostelManagement.Areas.HostelMessManagement
{
    public class HostelMessManagementAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "HostelMessManagement";
            }
        }

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