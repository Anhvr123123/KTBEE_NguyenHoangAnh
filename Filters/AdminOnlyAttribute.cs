using System;
using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using KTBEE_NguyenHoangAnh.Models;

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

            bool allowed = false;

            if (!string.IsNullOrWhiteSpace(username) && role == "A")
            {
                using (var db = new LuxuryAutoEntities())
                {
                    allowed = db.Users.Any(
                        u => u.Username == username &&
                             u.UserRole.Trim() == "A");
                }
            }

            if (!allowed)
            {
                if (role == "A")
                {
                    filterContext.HttpContext.Session.Clear();
                }

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