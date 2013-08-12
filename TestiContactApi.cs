
using System.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Tagovi.iContact
{
	[TestClass]
	public class TestiContactApi
	{
		private iContactApi _api;

		[TestInitialize]
		public void InitializeTests()
		{
			_api = new iContactApi
				{
					AppId = ConfigurationManager.AppSettings["iContactAppId"],
					Username = ConfigurationManager.AppSettings["iContactUsername"],
					Password = ConfigurationManager.AppSettings["iContactPassword"],
					AccountId = ConfigurationManager.AppSettings["iContactAccountId"],
					ClientFolderId = ConfigurationManager.AppSettings["iContactClientFolderId"],
					TestMode = true,
				};
		}

		[TestMethod]
		public void Test_Get_Lists()
		{
			var response = _api.Execute(new GetListsRequest());
			Assert.IsNotNull(response);
			Assert.IsTrue(response.Lists.Count > 0, "Lists.Count should be greater than zero");
		}
	};
}
