using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace LiveCalls
{
    public class OpenCall : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public string MaterialDescription { get; set; }
        private int _quantityOpen;
        public int QuantityOpen
        {
            get => _quantityOpen;
            set
            {
                _quantityOpen = value;
                NotifyPropertyChanged();
            }
        }
        private DateTime _oldest;
        public DateTime Oldest
        {
            get => _oldest;
            set
            {
                _oldest = value;
                NotifyPropertyChanged();

            }
        }
        private DateTime _newest;
        public DateTime Newest
        {
            get => _newest;
            set
            {
                _newest = value;
                NotifyPropertyChanged();
            }
        }
        public int Turnaround { get; set; }
        private int _repairPrice;
        public int RepairPrice
        {
            get => _repairPrice;
            set
            {
                _repairPrice = value;
                NotifyPropertyChanged();
            }
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    /*public class OpenCall
    {
        public string MaterialDescription { get; set; }
        public int Turnaround { get; set; }
        public DateTime Oldest { get; set;}
        public DateTime Newest { get;set;}
        public int QuantityOpen { get; set;}
        public int RepairPrice { get; set; }
    }*/
}