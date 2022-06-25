using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using XmppMessenger.Commands;
using XmppMessenger.Models;
using XmppMessenger.Views;

namespace XmppMessenger.ViewModels
{
    class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private XMPPClient client = new XMPPClient();

        private string jid;
        public string Jid
        {
            get => jid;
            set
            {
                jid = value;
                RaisePropertyChanged();
            }
        }

        private string password;
        public string Password 
        {

            get => password;
            set
            {
                password = value;
                RaisePropertyChanged();
            }
        }

        private bool loggedIn = false;

        public bool LoggedIn
        {
            get => loggedIn;
            set
            {
                loggedIn = value;
                RaisePropertyChanged();
            }
        }

        public string Username
        {
            get => Jid.Split("@")[0];
        }

        public string Hostname
        {
            get => Jid.Split("@")[1];
        }

        public ObservableCollection<User> Roster { get; private set; } = new ObservableCollection<User>();

        private List<ChatWindow> chatWindows = new List<ChatWindow>();

        public RelayCommand LoginCommand { get; private set; }
        public RelayCommand LogoutCommand { get; private set; }
        public RelayCommand OpenChatCommand { get; private set; }

        public MainViewModel()
        {
            client.RosterRecieved += jids =>
            {      
                foreach (string jid in jids)
                {
                    Application.Current.Dispatcher.Invoke(() => Roster.Add(new User(jid)));
                }
            };


            LoginCommand = new RelayCommand(async _ =>
            {
                bool success = await client.Connect(Hostname, Username, Password);

                if(success)
                {
                    LoggedIn = true;

                    client.Listen();

                    await Task.Delay(500);

                    client.Roster();
                }

            }, _=> !LoggedIn);


            LogoutCommand = new RelayCommand(_ =>
            {
                LoggedIn = false;

                client.Close();
            
            }, _=> LoggedIn);
            OpenChatCommand = new RelayCommand(user =>
            {

                ChatWindow window = chatWindows.Where(w => ((ChatViewModel)w.DataContext).User.ToString() == user.ToString()).FirstOrDefault();
                if(window == null)
                {
                    window = new ChatWindow { DataContext = new ChatViewModel((User)user, client) };
                    chatWindows.Add(window);
                    window.Closed += (obj, args) => chatWindows.Remove(window);
                    window.Show();
                }
                else
                {
                    if(window.WindowState == WindowState.Minimized)
                    {
                        window.WindowState = WindowState.Normal;
                    }
                    window.Focus();
                }
                
            });
        }



        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
