using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using NotifyIcon = System.Windows.Forms.NotifyIcon;
using XmppMessenger.Commands;
using XmppMessenger.ViewModels;

namespace XmppMessenger
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon;

        public MainWindow()
        {
            InitializeComponent();

            notifyIcon = new NotifyIcon();

            using (Stream stream = Application.GetResourceStream(new Uri("pack://application:,,,/Icon.ico")).Stream)
            {
                notifyIcon.Icon = new System.Drawing.Icon(stream);
            }

            notifyIcon.Click += NotifyIcon_Click;

        }

        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            Application.Current.MainWindow.Show();

            notifyIcon.Visible = false;
        }

        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            notifyIcon.Dispose();

            MainViewModel model = (MainViewModel)DataContext;

            model.LogoutCommand.Execute(null);

            Application.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.MainWindow.Hide();

            notifyIcon.Visible = true;
            
        }

        private void RosterItem_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MainViewModel model = (MainViewModel)DataContext;

            model.OpenChatCommand.Execute(((TextBlock)sender).Text);

        }
    }
}
