using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace Demo
{
    public class ModelView : INotifyPropertyChanged
    {
        #region Public Constructors

        public ModelView(int i = 10_000)
        {
            count = i;
            SelectedItem = count;
        }

        #endregion Public Constructors

        #region Public Events

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion Public Events

        #region Private Fields

        private ICollectionView collView;
        private int count;
        private string search = string.Empty;

        #endregion Private Fields

        #region Public Properties

        public ObservableCollection<Employee> Employees { get; set; }

        public ObservableCollection<Employee> FilteredList { get; set; }

        public int[] NumberItems { get; } =
        {
            10, 100, 1000, 10_000, 100_000, 500_000, 1_000_000
        };

        /// <summary>
        ///     Refresh all
        /// </summary>
        public ICommand RefreshCommand => new DelegateCommand(RefreshData);

        /// <summary>
        ///     Global filter
        /// </summary>
        public string Search
        {
            get => search;
            set
            {
                search = value;

                collView.Filter = e =>
                {
                    var item = (Employee)e;
                    return item != null &&
                           (item.LastName != null && item.LastName.StartsWith(search, StringComparison.OrdinalIgnoreCase)
                            || item.FirstName != null && item.FirstName.StartsWith(search, StringComparison.OrdinalIgnoreCase));
                };

                if (collView != null)
                {
                    collView.Refresh();
                    FilteredList = new ObservableCollection<Employee>(collView.OfType<Employee>());
                }

                OnPropertyChanged(nameof(Search));
                OnPropertyChanged(nameof(FilteredList));
            }
        }

        public int SelectedItem
        {
            get => count;
            set
            {
                count = value;
                OnPropertyChanged(nameof(SelectedItem));
                Task.Run(FillData);
            }
        }

        #endregion Public Properties

        #region Private Methods

        /// <summary>
        ///     Fill data
        /// </summary>
        private async void FillData()
        {
            search = "";

            var employee = new List<Employee>(count);
            var countries = new Countries();

            // for distinct lastname set "true" at CreateRandomEmployee(true)
            await Task.Run(() =>
            {
                for (var i = 0; i < count; i++)
                    employee.Add(RandomGenerator.CreateRandomEmployee(true, countries));
            });

            Employees = new ObservableCollection<Employee>(employee);
            FilteredList = new ObservableCollection<Employee>(employee);
            collView = CollectionViewSource.GetDefaultView(FilteredList);

            OnPropertyChanged("Search");
            OnPropertyChanged("Employes");
            OnPropertyChanged("FilteredList");
        }

        public void OnPropertyChanged(string propertyname)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyname));
        }

        /// <summary>
        ///     refresh data
        /// </summary>
        /// <param name="obj"></param>
        private void RefreshData(object obj)
        {
            collView = CollectionViewSource.GetDefaultView(new object());
            Task.Run(FillData);
        }

        #endregion Private Methods
    }
}
