using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SampleCustomSessionIDFull
{
	public partial class Default : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
			Session["testvar1"] = "my test variable";

			Response.Write("SessionID:" + Session.SessionID);
		}
	}
}