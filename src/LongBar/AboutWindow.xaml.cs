using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LongBar
{
  /// <summary>
  /// Interaction logic for AboutWindow.xaml
  /// </summary>
  public partial class AboutWindow : Window
  {
    public AboutWindow()
    {
      InitializeComponent();
      this.SourceInitialized +=new EventHandler(AboutWindow_SourceInitialized);
    }

    private void AboutWindow_SourceInitialized(object sender, EventArgs e)
    {
      System.Reflection.Assembly _AsmObj = System.Reflection.Assembly.GetExecutingAssembly();
      System.Reflection.AssemblyName _CurrAsmName = _AsmObj.GetName();
      string _Major = _CurrAsmName.Version.Major.ToString();
      string _Minor = _CurrAsmName.Version.Minor.ToString();
      string _Build = _CurrAsmName.Version.Build.ToString();//.Substring(0, 1);
      string _Revision = _CurrAsmName.Version.Revision.ToString();//.Substring(0, 1);

      VersionString.Text = string.Format("{0} {2}.{3} Release Candidate 1. {1} {4} (L{2}.{3}.{4}.{5}rc1",
        TryFindResource("Version"), TryFindResource("Build"), _Major, _Minor, _Build, _Revision);
      CopyrightString1.Text = String.Format("© LongBar Project Group 2009. {0}", TryFindResource("AllRightsReserved"));
      CopyrightString2.Text = String.Format("{0}.", Application.Current.TryFindResource("CopyrightLaw"));
      ContactString.Text = String.Format("{0} stealth2008@live.ru", TryFindResource("Contact"));
    }

    private void OKButton_Click(object sender, RoutedEventArgs e)
    {
      this.Close();
    }
  }
}
