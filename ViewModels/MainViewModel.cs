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

        private Dictionary<string, ChatViewModel> chatViewModels = new Dictionary<string, ChatViewModel>();
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
                    User user = FindOrAddUser(jid);
                    chatViewModels.Add(jid, new ChatViewModel(user, client));
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
                    ChatViewModel model;
                    chatViewModels.TryGetValue(user.ToString(), out model);
                    window = new ChatWindow { DataContext = model };
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

            client.MessageRecieved += ProcessMessage;
            client.PresenceRecieved += ProcessPresence;
        }

        private void ProcessMessage(Message message)
        {
            AddResourceToRoster(message.Jid, message.Resource);
        }

        private void ProcessPresence(Presence presence)
        {
            AddResourceToRoster(presence.Jid, presence.Resource);
        }

        private void AddResourceToRoster(string userJid, string resource)
        {
            User user = FindOrAddUser(userJid);

            user.Resources.Add(resource);
        }

        private User FindOrAddUser(string userJid)
        {
            User user = Roster.Where(user => user.Jid == userJid).FirstOrDefault();

            if (user == null)
            {
                user = new User(userJid);
                Application.Current.Dispatcher.Invoke(() => Roster.Add(user));
            }

            return user;
        }

        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
