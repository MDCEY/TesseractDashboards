using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Windows.Threading;
using Dapper;
using LiveCalls.BoilerPlate;
using LiveCalls.Properties;

namespace LiveCalls
{
    public class VMLiveCalls : ViewModelBase
    {


        private OpenCall _openCall;
        private ObservableCollection<OpenCall> _openCalls;
        public OpenCall OpenCall
        {
            get => _openCall;
            set
            {
                _openCall = value;
                NotifyPropertyChanged(nameof(OpenCall));
            }
        }
        public ObservableCollection<OpenCall> OpenCalls
        {
            get => _openCalls;
            set
            {
                _openCalls = value;
                NotifyPropertyChanged(nameof(OpenCalls));
            }
        }

        public VMLiveCalls()
        {
            InitializeTimer();
            OpenCall = new OpenCall();
            OpenCalls = new ObservableCollection<OpenCall>();
            OpenCalls.CollectionChanged += OpenCalls_CollectionChanged;
            Update();
        }

        private void Update()
        {
            var resultsFromDb = FetchResultsFromDB();
            // add to datagrid
            foreach (var result in resultsFromDb)
            {
                var resultDescription = result.MaterialDescription;
                var existingCall = OpenCalls.FirstOrDefault(x => x.MaterialDescription == resultDescription);
                if (existingCall != null)
                {
                    existingCall.Newest = result.Newest;
                    existingCall.QuantityOpen = result.QuantityOpen;
                    existingCall.Oldest = result.Oldest;
                    existingCall.RepairPrice = result.RepairPrice;
                }
                else
                {
                    OpenCalls.Add(new OpenCall
                    {
                        MaterialDescription = result.MaterialDescription,
                        Newest = result.Newest,
                        Oldest = result.Oldest,
                        QuantityOpen = result.QuantityOpen,
                        RepairPrice = result.RepairPrice,
                        Turnaround = result.Turnaround
                    });
                }
            }
            // remove from datagrid

            // loop through datagrid. if description not in db results remove description
            List<OpenCall> toRemove = new List<OpenCall>();
            foreach (var openCall in OpenCalls)
            {
                var desc = openCall.MaterialDescription; //if this not in db then remove it
                var inDb = resultsFromDb.FirstOrDefault(x => x.MaterialDescription == desc);
                if (inDb == null)
                {
                    toRemove.Add(openCall);
                }
            }

            foreach (var call in toRemove)
            {
                OpenCalls.Remove(call);
            }

        }

        private List<OpenCall> FetchResultsFromDB()
        {
            var query = @"SELECT MaterialDescription = Prod_Desc,
                                 QuantityOpen = COUNT(Call_Num),
                                 RepairPrice = SUM(Part_Repair_End),
                                 Oldest = MIN(Call_InDate),
                                 Newest = MAX(Call_InDate),
                                 Turnaround = Prod_Ref2 FROM COOPESOLBRANCHLIVE.dbo.SCCALL
                          LEFT JOIN COOPESOLBRANCHLIVE.dbo.SCProd ON Call_Prod_Num = Prod_Num
                          LEFT JOIN COOPESOLBRANCHLIVE.dbo.SCPart ON Job_Part_Num = Part_Num
                          WHERE Job_Ref6 is not null
                          AND Call_Employ_Num is null
                          AND Job_CDate is null
                          group by Prod_Desc, Prod_Ref2
                          ORDER BY Oldest";
            var _ = Convert.FromBase64String(Resources.ConnectionString);
            var connection = new SqlConnection(Encoding.UTF8.GetString(_));
            var update = connection.Query<OpenCall>(query).ToList();
            return update;
        }



        private void OpenCalls_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged(nameof(OpenCalls));
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
    }
}
