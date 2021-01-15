using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RecentBookins
{
    public class BookedInRepair : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string Product { get; set; }
        private int _quantityBookedIn;
        public int QuantityBookedIn
        {
            get => _quantityBookedIn;
            set
            {
                _quantityBookedIn = value; 
                NotifyPropertyChanged();
            }
        }
        private DateTime _lastBookedIn;
        public DateTime LastBookedIn
        {
            get => _lastBookedIn;
            set
            {
                _lastBookedIn = value;
                NotifyPropertyChanged();
            }
        }
        

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}