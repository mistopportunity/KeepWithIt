using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace KeepWithIt
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

		private void Page_LayoutUpdated(object sender,object e) {

			GridLength columnSize;
			if(ActualWidth < ActualHeight) {
				columnSize = new GridLength(0.75,GridUnitType.Star);
			} else {
				columnSize = new GridLength(12,GridUnitType.Star);
			}
			SubColumnFirst.Width = columnSize;
			SubColumnLast.Width = columnSize;

			PlaceholderGrid1.Width = ContentColumn1.ActualWidth;
			PlaceholderGrid1.Height = PlaceholderGrid1.Width;

			PlaceholderGrid2.Width = ContentColumn2.ActualWidth;
			PlaceholderGrid2.Height = PlaceholderGrid2.Width;

			var poorlyDynamicMargin = new Thickness(0,CentralSubColumn.ActualWidth,0,0);

			PlaceholderGrid1.Margin = poorlyDynamicMargin;
			PlaceholderGrid2.Margin = poorlyDynamicMargin;


		}
	}
}
