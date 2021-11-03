using System.Web.Mvc;

namespace Aplos.Areas.Accounts
{
	public class AccountsAreaRegistration : AreaRegistration
	{
		public override string AreaName
		{
			get
			{
				return "Accounts";
			}
		}

		public override void RegisterArea(AreaRegistrationContext context)
		{
			context.MapRoute(
				name: "Accounts",
				url: "accounts/{controller}/{action}/{id}",
				defaults: new { action = "aplos", id = UrlParameter.Optional },
				namespaces: new string[] { "Aplos.Areas.Accounts.Controllers" }
			);
		}
	}
}