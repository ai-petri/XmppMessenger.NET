using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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


        public ChatViewModel(User user)
        {
            this.user = user;
        }

        public void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
