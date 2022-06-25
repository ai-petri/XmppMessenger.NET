using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using XmppMessenger.Commands;
using XmppMessenger.Models;

namespace XmppMessenger.ViewModels
{
    class ChatViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;


        private User user;

        public User User
        {
            get => user;
            set
            {
                user = value;
                RaisePropertyChanged();
            }
        }

        private string text;

        public string Text
        {
            get => text;
            set
            {
                text = value;
                RaisePropertyChanged();
            }
        }

        private string chat;

        public string Chat
        {
            get => chat;
            set
            {
                chat = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand SendMessageCommand { get; private set; }

        public ChatViewModel(User user, XMPPClient client)
        {
            this.user = user;

            client.MessageRecieved += ProcessMessage;

            SendMessageCommand = new RelayCommand(_ => { client.SendMessage(user, Text); Text = ""; });
        }

        private void ProcessMessage(Message message)
        {
            if(message.From.StartsWith(User.Jid))
            {
                Chat += message.Text;
                Chat += "\n";
            }
        }

        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
