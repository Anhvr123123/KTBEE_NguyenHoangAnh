using System;
using System.Web.Mvc;
using System.Web.Routing;

namespace KTBEE_NguyenHoangAnh.Filters
{
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(
            ActionExecutingContext filterContext)
        {
            string username = Convert.ToString(
                filterContext.HttpContext.Session["Username"]);

            string role = Convert.ToString(
                filterContext.HttpContext.Session["UserRole"]).Trim();

            if (string.IsNullOrWhiteSpace(username) || role != "A")
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "area", "Admin" },
                        { "controller", "Account" },
                        { "action", "Login" }
                    });

                return;
            }

            base.OnActionExecuting(filterContext);
        }
    }
}