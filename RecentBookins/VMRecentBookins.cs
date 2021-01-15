using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows.Threading;
using Dapper;
using RecentBookins.Properties;

namespace RecentBookins
{
    public class VMRecentBookins :MVVMBoilerplate.ViewModelBase
    {
        private BookedInRepair _bookedInRepair;
        public BookedInRepair BookedInRepair
        {
            get => _bookedInRepair;
            set
            {
                _bookedInRepair = value;
                OnPropertyChanged(nameof(BookedInRepair));
            }
        }

        private ObservableCollection<BookedInRepair> _bookedInRepairs;
        public ObservableCollection<BookedInRepair> BookedInRepairs
        {
            get => _bookedInRepairs;
            set
            {
                _bookedInRepairs = value;
                OnPropertyChanged(nameof(BookedInRepairs));
            }
        }


        public VMRecentBookins()
        {
            InitializeTimer();
            BookedInRepair = new BookedInRepair();
            BookedInRepairs = new ObservableCollection<BookedInRepair>();
            BookedInRepairs.CollectionChanged += BookedInCalls_CollectionChanged;
            Update();
        }

        private void Update()
        {
            var resultsFromDb = FetchResultsFromDB();

            foreach (var result in resultsFromDb)
            {
                var resultDescription = result.Product;
                var existingCall = BookedInRepairs.FirstOrDefault(x => x.Product == resultDescription);
                if (existingCall != null)
                {
                    existingCall.LastBookedIn = result.LastBookedIn;
                    existingCall.QuantityBookedIn = result.QuantityBookedIn;
                }
                else
                {
                    BookedInRepairs.Add(new BookedInRepair
                    {
                        Product = result.Product,
                        LastBookedIn = result.LastBookedIn,
                        QuantityBookedIn = result.QuantityBookedIn
                    });
                }
            }
            // remove from datagrid

            // loop through datagrid. if description not in db results remove description
            List<BookedInRepair> toRemove = new List<BookedInRepair>();
            foreach (var bookedInRepair in BookedInRepairs)
            {
                var desc = bookedInRepair.Product; //if this not in db then remove it
                var inDb = resultsFromDb.FirstOrDefault(x => x.Product == desc);
                if (inDb == null)
                {
                    toRemove.Add(bookedInRepair);
                }
            }

            foreach (var call in toRemove)
            {
                BookedInRepairs.Remove(call);
            }


        }

        private List<BookedInRepair> FetchResultsFromDB()
        {
            var query = @"select 
                          Prod_Desc as product,
                          Count(Call_Num) as QuantityBookedIn,
                          Max(Call_InDate) as LastBookedIn
                          FROM COOPESOLBRANCHLIVE.dbo.SCCall
                          JOIN COOPESOLBRANCHLIVE.dbo.SCProd ON Call_Prod_Num = Prod_Num
                          WHERE Call_InDate >= convert(varchar(10), getdate(), 120)
                          GROUP BY Prod_Desc";
            var _ = Convert.FromBase64String(Resources.ConnectionString);
            var connection = new SqlConnection(Encoding.UTF8.GetString(_));
            var update = connection.Query<BookedInRepair>(query).ToList();
            connection.Close();
            return update;
        }
        private void InitializeTimer()
        {
            var dispatchTimer = new DispatcherTimer();
            dispatchTimer.Tick += DispatchTimer_Tick;
            dispatchTimer.Interval = new TimeSpan(0, 1, 0);
            dispatchTimer.Start();
        }
        private void DispatchTimer_Tick(object sender, EventArgs e)
        {
            Update();
        }
        private void BookedInCalls_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(BookedInRepairs));
        }
    }
}