using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using XmppMessenger.Commands;

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



        public RelayCommand LoginCommand { get; private set; }
        public RelayCommand LogoutCommand { get; private set; }

        public MainViewModel()
        {
            LoginCommand = new RelayCommand(_ =>
            {
                bool success = client.Connect(Hostname, Username, Password).Result;

                if(success)
                {
                    LoggedIn = true;

                    client.Listen();
                }

            }, _=> !LoggedIn);


            LogoutCommand = new RelayCommand(_ =>
            {
                LoggedIn = false;

                client.Close();
            
            }, _=> LoggedIn);
        }



        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
