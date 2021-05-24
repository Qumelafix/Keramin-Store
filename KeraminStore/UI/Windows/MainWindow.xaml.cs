using KeraminStore.UI.Windows;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace KeraminStore
{
    public partial class MainWindow : Window
    {
        readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");

        public MainWindow()
        {
            InitializeComponent();

            StreamReader file = new StreamReader("UserCode.txt");
            int employeeCode = Convert.ToInt32(file.ReadLine());
            file.Close();

            string selectEmployeeStatusQuery = "SELECT employeeAdminStatus FROM Employee WHERE employeeCode = " + employeeCode + "";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectEmployeeStatusQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0 && Convert.ToBoolean(table.Rows[0]["employeeAdminStatus"].ToString()) == false)
                {
                    //AddEmployee.Visibility = Visibility.Hidden;
                    //StaffList.Visibility = Visibility.Hidden;
                    Employee.Visibility = Visibility.Hidden;
                    Pickup.Visibility = Visibility.Hidden;
                }
            }
        }

        private void ButtonPopUpLogout_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Application.Current.Shutdown();
            File.WriteAllText(@"UserCode.txt", string.Empty);
        }

        private void ButtonOpenMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenMenu.Visibility = Visibility.Collapsed;
            ButtonCloseMenu.Visibility = Visibility.Visible;
        }

        private void ButtonCloseMenu_Click(object sender, RoutedEventArgs e)
        {
            ButtonOpenMenu.Visibility = Visibility.Visible;
            ButtonCloseMenu.Visibility = Visibility.Collapsed;
        }

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();

        private void ChangeAccButton_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(@"UserCode.txt", string.Empty);
            AutorizationWindow autorizationWindow = new AutorizationWindow();
            autorizationWindow.Show();
            this.Close();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            WorkPlace.Children.Clear();
            WorkPlace.Children.Add(new AccountSettingsWindow());
        }

        private void Menu_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            int menuIndex = Menu.SelectedIndex;
            switch (menuIndex)
            {
                case 0:
                    WorkPlace.Children.Clear();
                    Menu.SelectedIndex = -1;
                    WorkPlace.Children.Add(new MainPage());
                    break;

                case 1:
                    WorkPlace.Children.Clear();
                    Menu.SelectedIndex = -1;
                    WorkPlace.Children.Add(new CatalogWindow());
                    break;

                case 2:
                    WorkPlace.Children.Clear();
                    Menu.SelectedIndex = -1;
                    WorkPlace.Children.Add(new TileCalculatorWindow());
                    break;

                //case 3:
                //    WorkPlace.Children.Clear();
                //    Menu.SelectedIndex = -1;
                //    WorkPlace.Children.Add(new AddNewProductWindow());
                //    break;

                //case 4:
                //    WorkPlace.Children.Clear();
                //    WorkPlace.Children.Add(new MainPage());
                //    ProductsListWindow productsListWindow = new ProductsListWindow();
                //    Menu.SelectedIndex = -1;
                //    productsListWindow.ShowDialog();
                //    break;

                case 4:
                    WorkPlace.Children.Clear();
                    WorkPlace.Children.Add(new MainPage());
                    CreateOrderWindow createOrder = new CreateOrderWindow();
                    Menu.SelectedIndex = -1;
                    createOrder.ShowDialog();
                    break;

                case 6:
                    WorkPlace.Children.Clear();
                    Menu.SelectedIndex = -1;
                    WorkPlace.Children.Add(new OrdersListWindow());
                    break;

                //case 7:
                //    WorkPlace.Children.Clear();
                //    Menu.SelectedIndex = -1;
                //    WorkPlace.Children.Add(new RegistrationWindow());
                //    break;

                //case 8:
                //    WorkPlace.Children.Clear();
                //    Menu.SelectedIndex = -1;
                //    WorkPlace.Children.Add(new EmployeesWindow());
                //    break;

                //case 8:
                //    WorkPlace.Children.Clear();
                //    WorkPlace.Children.Add(new MainPage());
                //    File.WriteAllText(@"WindowCode.txt", string.Empty);
                //    StatisticTypeWindow statisticTypeWindow = new StatisticTypeWindow();
                //    Menu.SelectedIndex = -1;
                //    statisticTypeWindow.ShowDialog();
                //    StreamReader reader = new StreamReader("WindowCode.txt");
                //    string windowCode = reader.ReadLine();
                //    reader.Close();
                //    if (windowCode == "1") WorkPlace.Children.Add(new EmpoyeesStatistic());
                //    else if (windowCode == "2") WorkPlace.Children.Add(new ProductsStatistic());
                //    break;

                default:
                    break;
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            StreamReader file = new StreamReader("UserCode.txt");
            int employeeCode = Convert.ToInt32(file.ReadLine());
            file.Close();

            string selectEmployeeStatusQuery = "SELECT employeeAdminStatus FROM Employee WHERE employeeCode = " + employeeCode + "";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectEmployeeStatusQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0 && Convert.ToBoolean(table.Rows[0]["employeeAdminStatus"].ToString()) == true)
                {
                    HelpNavigator navigator = HelpNavigator.Topic;
                    Help.ShowHelp(null, "help.chm", navigator, "rukovodstvo_administratora.htm");
                }
                else if (table.Rows.Count > 0 && Convert.ToBoolean(table.Rows[0]["employeeAdminStatus"].ToString()) == false)
                {
                    HelpNavigator navigator = HelpNavigator.Topic;
                    Help.ShowHelp(null, "help.chm", navigator, "rukovodstvo_sotrudnika.htm");
                }
            }
        }
        
        private void Statistic_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var mouseDownEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
            {
                RoutedEvent = Mouse.MouseUpEvent,
                Source = Statistic,
            };
            InputManager.Current.ProcessInput(mouseDownEvent);
        }

        private void EmployeesStatistic_Click(object sender, RoutedEventArgs e)
        {
            WorkPlace.Children.Clear();
            WorkPlace.Children.Add(new EmpoyeesStatistic());
            Menu.SelectedIndex = -1;
        }

        private void ProductsStatistic_Click(object sender, RoutedEventArgs e)
        {
            WorkPlace.Children.Clear();
            WorkPlace.Children.Add(new ProductsStatistic());
            Menu.SelectedIndex = -1;
        }

        private void Product_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var mouseDownEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
            {
                RoutedEvent = Mouse.MouseUpEvent,
                Source = Product,
            };
            InputManager.Current.ProcessInput(mouseDownEvent);
        }

        private void ProductsList_Click(object sender, RoutedEventArgs e)
        {
            WorkPlace.Children.Clear();
            WorkPlace.Children.Add(new MainPage());
            ProductsListWindow productsListWindow = new ProductsListWindow();
            Menu.SelectedIndex = -1;
            productsListWindow.ShowDialog();
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            WorkPlace.Children.Clear();
            Menu.SelectedIndex = -1;
            WorkPlace.Children.Add(new AddNewProductWindow());
        }

        private void Employee_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var mouseDownEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
            {
                RoutedEvent = Mouse.MouseUpEvent,
                Source = Employee,
            };
            InputManager.Current.ProcessInput(mouseDownEvent);
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            WorkPlace.Children.Clear();
            Menu.SelectedIndex = -1;
            WorkPlace.Children.Add(new RegistrationWindow());
        }

        private void EmployeesList_Click(object sender, RoutedEventArgs e)
        {
            WorkPlace.Children.Clear();
            Menu.SelectedIndex = -1;
            WorkPlace.Children.Add(new EmployeesWindow());
        }

        private void Pickup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            var mouseDownEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
            {
                RoutedEvent = Mouse.MouseUpEvent,
                Source = Pickup,
            };
            InputManager.Current.ProcessInput(mouseDownEvent);
        }

        private void AddPickup_Click(object sender, RoutedEventArgs e)
        {
            WorkPlace.Children.Clear();
            WorkPlace.Children.Add(new MainPage());
            AddPickupAdressWindow addPickupAdress = new AddPickupAdressWindow();
            Menu.SelectedIndex = -1;
            addPickupAdress.ShowDialog();
        }

        private void PickupList_Click(object sender, RoutedEventArgs e)
        {
            WorkPlace.Children.Clear();
            Menu.SelectedIndex = -1;
            WorkPlace.Children.Add(new PickupsListWindow());
        }
    }
}