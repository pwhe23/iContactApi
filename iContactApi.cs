
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;

namespace Tagovi.iContact
{
	public class iContactApi
	{
		private const string PROD_URL = "https://app.icontact.com/icp";
		private const string SANDBOX_URL = "https://app.sandbox.icontact.com/icp";

		public iContactApi()
		{
			TestMode = false;
		}

		public string BaseUrl { get; set; }
		public string AppId { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string AccountId { get; set; }
		public string ClientFolderId { get; set; }

		private bool _testMode;

		public bool TestMode
		{
			get { return _testMode; }
			set
			{
				_testMode = value;
				BaseUrl = value ? SANDBOX_URL : PROD_URL;
			}
		}

		public ContactsResponse Execute(GetContactsRequest request)
		{
			return Execute<ContactsResponse>(request);
		}

		public ContactsResponse Execute(AddContactsRequest request)
		{
			return Execute<ContactsResponse>(request);
		}

		public ListsResponse Execute(GetListsRequest request)
		{
			return Execute<ListsResponse>(request);
		}

		public SubscribeResponse Execute(SubscribeRequest request)
		{
			return Execute<SubscribeResponse>(request);
		}

		private T Execute<T>(iContactRequest request)
		{
			var jsonRequest = BuildJsonRequest(request.GetUrl(), request.Method, request.GetBody());
			return GetJsonResponse<T>(jsonRequest);
		}

		private HttpWebRequest BuildJsonRequest(string resource, string method, string data)
		{
			if (string.IsNullOrWhiteSpace(BaseUrl))
				throw new ApplicationException("BaseUrl must have a value");

			if (string.IsNullOrWhiteSpace(AccountId))
				throw new ApplicationException("AccountId must have a value");

			if (string.IsNullOrWhiteSpace(ClientFolderId))
				throw new ApplicationException("ClientFolderId must have a value");

			if (string.IsNullOrWhiteSpace(AppId))
				throw new ApplicationException("AppId must have a value");

			if (string.IsNullOrWhiteSpace(Username))
				throw new ApplicationException("Username must have a value");

			if (string.IsNullOrWhiteSpace(Password))
				throw new ApplicationException("Password must have a value");

			var uri = new Uri(BaseUrl + "/a/" + AccountId + "/c/" + ClientFolderId + "/" + resource);

			var request = (HttpWebRequest)WebRequest.Create(uri);
			request.Method = method;
			request.Accept = "application/json";
			request.Headers.Add("Api-Version", "2.2");
			request.Headers.Add("Api-AppId", AppId);
			request.Headers.Add("Api-Username", Username);
			request.Headers.Add("Api-Password", Password);
			request.ContentType = "application/json";

			if (data != null)
			{
				var bytes = Encoding.UTF8.GetBytes(data);
				request.ContentLength = bytes.Length;

				using (var stream = request.GetRequestStream())
				{
					stream.Write(bytes, 0, bytes.Length);
				}
			}

			return request;
		}

		private static T GetJsonResponse<T>(HttpWebRequest request)
		{
			using (var response = (HttpWebResponse)request.GetResponse())
			{
				using (var reader = new StreamReader(response.GetResponseStream()))
				{
					var jsonData = reader.ReadToEnd();
					var serializer = new JavaScriptSerializer();
					return serializer.Deserialize<T>(jsonData);
				}
			}
		}
	};

	public class Contact : Dictionary<string, string>
	{
		public int? ContactId
		{
			get { return this.GetValue<int?>("contactId"); }
		}

		public string Email
		{
			get { return this.GetValue<string>("email"); }
			set { this.SetValue("email", value); }
		}

		public string FirstName
		{
			get { return this.GetValue<string>("firstName"); }
			set { this.SetValue("firstName", value); }
		}

		public string LastName
		{
			get { return this.GetValue<string>("lastName"); }
			set { this.SetValue("lastName", value); }
		}

		public string Prefix
		{
			get { return this.GetValue<string>("prefix"); }
			set { this.SetValue("prefix", value); }
		}

		public string Suffix
		{
			get { return this.GetValue<string>("suffix"); }
			set { this.SetValue("suffix", value); }
		}

		public string Fax
		{
			get { return this.GetValue<string>("fax"); }
			set { this.SetValue("fax", value); }
		}

		public string Phone
		{
			get { return this.GetValue<string>("phone"); }
			set { this.SetValue("phone", value); }
		}

		public string Street
		{
			get { return this.GetValue<string>("street"); }
			set { this.SetValue("street", value); }
		}

		public string Street2
		{
			get { return this.GetValue<string>("street2"); }
			set { this.SetValue("street2", value); }
		}

		public string City
		{
			get { return this.GetValue<string>("city"); }
			set { this.SetValue("city", value); }
		}

		public string State
		{
			get { return this.GetValue<string>("state"); }
			set { this.SetValue("state", value); }
		}

		public string PostalCode
		{
			get { return this.GetValue<string>("postalCode"); }
			set { this.SetValue("postalCode", value); }
		}

		public ContactStatus Status
		{
			get { return this.GetValue<ContactStatus>("status"); }
			set { this.SetValue("status", value.ToString().ToLower()); }
		}

		public DateTime CreateDate
		{
			get { return this.GetValue<DateTime>("createDate"); }
		}

		public int? BounceCount
		{
			get { return this.GetValue<int?>("bounceCount"); }
		}
	};

	public class GetContactsRequest : iContactRequest
	{
		public ContactsStatus Status { get; set; }
		public string OrderBy { get; set; }
		public string FirstName { get; set; }
		public DateTime? CreatedAfter { get; set; }

		public string Method
		{
			get { return "GET"; }
		}

		public string GetUrl()
		{
			var qs = new Dictionary<string, string>();

			if (Status != ContactsStatus.Default)
				qs["status"] = Status.ToString();

			if (!string.IsNullOrWhiteSpace(OrderBy))
				qs["orderBy"] = OrderBy;

			if (!string.IsNullOrWhiteSpace(FirstName))
				qs["firstName"] = FirstName;

			if (CreatedAfter.HasValue)
			{
				qs["createDate"] = CreatedAfter.Value.ToString("yyyy-MM-dd");
				qs["createDateSearchType"] = "gt";
			}

			return qs.ToUrl("contacts");
		}

		public string GetBody()
		{
			return null;
		}

		public enum ContactsStatus
		{
			Default,
			Unlisted,
			Total, //All
		};
	}

	public class ContactsResponse
	{
		public List<Contact> Contacts { get; set; }
		public int Limit { get; set; }
		public int Offset { get; set; }
		public int Total { get; set; }
	};

	public class AddContactsRequest : iContactRequest
	{
		public AddContactsRequest()
		{
			Contacts = new List<Contact>();
		}

		public List<Contact> Contacts { get; set; }

		public string Method
		{
			get { return "POST"; }
		}

		public string GetUrl()
		{
			return "contacts";
		}

		public string GetBody()
		{
			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(Contacts);
		}
	};

	public enum ContactStatus
	{
		Normal,
		Bounced,
		DoNotContact,
		Pending,
		Invitable,
		Delted,
	};

	public class List : Dictionary<string, string>
	{
		public int? ListId
		{
			get { return this.GetValue<int?>("listId"); }
		}

		public string Name
		{
			get { return this.GetValue<string>("name"); }
			set { this.SetValue("name", value); }
		}

		public bool EmailOwnerOnChange
		{
			get { return this.GetValue<string>("emailOwnerOnChange") == "1"; }
			set { this.SetValue("emailOwnerOnChange", value ? "1" : "0"); }
		}

		public bool WelcomeOnManualAdd
		{
			get { return this.GetValue<string>("welcomeOnManualAdd") == "1"; }
			set { this.SetValue("welcomeOnManualAdd", value ? "1" : "0"); }
		}

		public bool WelcomeOnSignupAdd
		{
			get { return this.GetValue<string>("welcomeOnSignupAdd") == "1"; }
			set { this.SetValue("welcomeOnSignupAdd", value ? "1" : "0"); }
		}

		public int? WelcomeMessageId
		{
			get { return this.GetValue<int?>("welcomeMessageId"); }
			set { this.SetValue("welcomeMessageId", value); }
		}

		public string Description
		{
			get { return this.GetValue<string>("description"); }
			set { this.SetValue("description", value); }
		}

		public string PublicName
		{
			get { return this.GetValue<string>("publicname"); }
			set { this.SetValue("publicname", value); }
		}
	};

	public class GetListsRequest : iContactRequest
	{
		public string Method
		{
			get { return "GET"; }
		}

		public string GetUrl()
		{
			return "lists";
		}

		public string GetBody()
		{
			return null;
		}
	};

	public class ListsResponse
	{
		public List<List> Lists { get; set; }
	};

	public class Subscription : Dictionary<string, string>
	{
		public string SubscriptionId
		{
			get { return this.GetValue<string>("subscriptionId"); }
		}

		public int? ContactId
		{
			get { return this.GetValue<int?>("contactId"); }
			set { this.SetValue("contactId", value); }
		}

		public int? ListId
		{
			get { return this.GetValue<int?>("listId"); }
			set { this.SetValue("listId", value); }
		}

		public ContactStatus Status
		{
			get { return this.GetValue<ContactStatus>("status"); }
			set { this.SetValue("status", value.ToString().ToLower()); }
		}

		public DateTime? AddDate
		{
			get { return this.GetValue<DateTime?>("addDate"); }
		}
	};

	public class SubscribeRequest : iContactRequest
	{
		public SubscribeRequest()
		{
			Subscriptions = new List<Subscription>();
		}

		public List<Subscription> Subscriptions { get; set; }

		public string Method
		{
			get { return "POST"; }
		}

		public string GetUrl()
		{
			return "subscriptions";
		}

		public string GetBody()
		{
			var serializer = new JavaScriptSerializer();
			return serializer.Serialize(Subscriptions);
		}
	};

	public class SubscribeResponse
	{
		public List<Subscription> Subscriptions { get; set; }
	};

	public interface iContactRequest
	{
		string Method { get; }
		string GetUrl();
		string GetBody();
	};

	internal static class iContactExtensions
	{
		public static V GetValue<V>(this IDictionary<string, string> dict, string key)
		{
			return dict != null && dict.ContainsKey(key) ? dict[key].To(default(V)) : default(V);
		}

		public static void SetValue(this IDictionary<string, string> dict, string key, object value)
		{
			dict[key] = value == null ? null : value.ToString();
		}

		public static T To<T>(this object obj, T def = default(T))
		{
			var value = To(obj, typeof(T));
			return value == null ? def : (T)value;
		}

		public static Object To(this object obj, Type type)
		{
			if (obj is DBNull) obj = null;
			if (obj == null || type == null) return null;
			if (type.IsInstanceOfType(obj)) return obj;
			if (obj is string && (string)obj == string.Empty) return null;
			if (type == typeof(int) || type == typeof(int?))
			{
				int num;
				if (int.TryParse(obj.ToString(), out num)) return num;
				return null;
			}
			if (type == typeof(short) || type == (typeof(short?)))
			{
				short num;
				if (short.TryParse(obj.ToString(), out num)) return num;
				return null;
			}
			if (type == typeof(bool) || type == typeof(bool?))
			{
				bool bul;
				if (bool.TryParse(obj.ToString(), out bul)) return bul;
				return null;
			}
			if (type.IsEnum)
			{
				return Enum.Parse(type, obj.ToString(), true);
			}
			if (type.CanConvertFrom<string>())
			{
				return Convert.ChangeType(obj.ToString(), type);
			}
			throw new Exception("Couldn't convert " + obj + " to " + type.FullName);
		}

		public static bool CanConvertFrom<T>(this Type type)
		{
			return CanConvertFrom(type, typeof(T));
		}

		public static bool CanConvertFrom(this Type to, Type from)
		{
			return TypeDescriptor.GetConverter(to).CanConvertFrom(from);
		}

		public static string ToUrl(this IDictionary<string, string> dict, string path)
		{
			if (dict.Count == 0)
				return path;

			return path + "?" + string.Join("&", dict.Select(x => x.Key + "=" + HttpUtility.UrlEncode(x.Value)));
		}
	};
}
