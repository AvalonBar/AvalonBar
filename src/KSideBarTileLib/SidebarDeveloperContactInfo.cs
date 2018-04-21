using System;
using System.Collections.Generic;
using System.Text;

namespace Applications.Sidebar
{
	[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false)]
	public sealed class SidebarDeveloperContactInfo : Attribute
	{
		// Fields
		public string email;
		public string eSubj;
		public PreferredDeveloperContact howContact;
		public string website;

		// Methods
		public SidebarDeveloperContactInfo(string emailAddress, string emailSubject, string websiteURL, PreferredDeveloperContact preferedContactMethod)
		{
			this.email = emailAddress;
			this.website = websiteURL;
			this.howContact = preferedContactMethod;
		}
	}
}
