using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Applications.Sidebar
{
	public class FlyoutEventArgs
	{
		// Fields
		private FrameworkElement _FlyoutContent;

		// Properties
		public FrameworkElement FlyoutContent
		{
			get
			{
				return this._FlyoutContent;
			}
			set
			{
				this._FlyoutContent = value;
			}
		}
	}


}
