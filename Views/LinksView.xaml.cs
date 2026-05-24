using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace PersonalBlog.Views
{
    public partial class LinksView : UserControl
    {
        public LinksView()
        {
            InitializeComponent();
        }

        private void LinkButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string url)
            {
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }
    }
}
