using KeraminStore.UI.Windows;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace KeraminStore
{
    public partial class MainWindow : Window
    {
        static string connectionString1 = ConfigurationManager.ConnectionStrings["KeraminStore.Properties.Settings.KeraminStoreConnectionString"].ConnectionString;
        readonly SqlConnection connectionString = new SqlConnection(connectionString1);

        public MainWindow()
        {
            InitializeComponent();

            StreamReader file = new StreamReader("UserCode.txt");
            int employeeCode = Convert.ToInt32(file.ReadLine()); //Считывание кода сотрудника
            file.Close();

            string selectEmployeeStatusQuery = "SELECT employeeAdminStatus FROM Employee WHERE employeeCode = " + employeeCode + "";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectEmployeeStatusQuery, connectionString)) //Отображение пунктов меню
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0 && Convert.ToBoolean(table.Rows[0]["employeeAdminStatus"].ToString()) == false)
                {
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

        private void ChangeAccButton_Click(object sender, RoutedEventArgs e) //Метод для смены аккаунта
        {
            File.WriteAllText(@"UserCode.txt", string.Empty);
            AutorizationWindow autorizationWindow = new AutorizationWindow();
            autorizationWindow.Show();
            this.Close();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e) //Метод для открытия окна изменения учетной записи
        {
            WorkPlace.Children.Clear();
            WorkPlace.Children.Add(new AccountSettingsWindow());
        }

        private void Menu_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) //Метод для открытия пункта бокового меню
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

                default:
                    break;
            }
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e) //Метод для вызова справки
        {
            StreamReader file = new StreamReader("UserCode.txt");
            int employeeCode = Convert.ToInt32(file.ReadLine()); //Считывание кода сотрудника
            file.Close();

            string selectEmployeeStatusQuery = "SELECT employeeAdminStatus FROM Employee WHERE employeeCode = " + employeeCode + "";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectEmployeeStatusQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0 && Convert.ToBoolean(table.Rows[0]["employeeAdminStatus"].ToString()) == true) //Открытие справки на соответствующем разделе
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
        
        private void Statistic_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) //Метод для открытия контекстного меню
        {
            e.Handled = true;
            var mouseDownEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
            {
                RoutedEvent = Mouse.MouseUpEvent,
                Source = Statistic,
            };
            InputManager.Current.ProcessInput(mouseDownEvent);
        }

        private void EmployeesStatistic_Click(object sender, RoutedEventArgs e) //Метод для отображения статистики о продуктивности сотрудников
        {
            WorkPlace.Children.Clear();
            WorkPlace.Children.Add(new EmpoyeesStatistics());
            Menu.SelectedIndex = -1;
        }

        private void ProductsStatistic_Click(object sender, RoutedEventArgs e) //Метод для отображения статистики по продажам изделий
        {
            WorkPlace.Children.Clear();
            WorkPlace.Children.Add(new ProductsStatistics());
            Menu.SelectedIndex = -1;
        }

        private void Product_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) //Метод для открытия контекстного меню
        {
            e.Handled = true;
            var mouseDownEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
            {
                RoutedEvent = Mouse.MouseUpEvent,
                Source = Product,
            };
            InputManager.Current.ProcessInput(mouseDownEvent);
        }

        private void ProductsList_Click(object sender, RoutedEventArgs e) //Метод для открытия списка изделий
        {
            WorkPlace.Children.Clear();
            WorkPlace.Children.Add(new MainPage());
            ProductsListWindow productsListWindow = new ProductsListWindow();
            Menu.SelectedIndex = -1;
            productsListWindow.ShowDialog();
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e) //Метод для добавления изделия
        {
            WorkPlace.Children.Clear();
            Menu.SelectedIndex = -1;
            WorkPlace.Children.Add(new AddNewProductWindow());
        }

        private void Employee_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) //Метод для открытия контекстного меню
        {
            e.Handled = true;
            var mouseDownEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
            {
                RoutedEvent = Mouse.MouseUpEvent,
                Source = Employee,
            };
            InputManager.Current.ProcessInput(mouseDownEvent);
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e) //Метод для регистрации сотрудников
        {
            WorkPlace.Children.Clear();
            Menu.SelectedIndex = -1;
            WorkPlace.Children.Add(new RegistrationWindow());
        }

        private void EmployeesList_Click(object sender, RoutedEventArgs e) //Метод для открытия списка сотрудников
        {
            WorkPlace.Children.Clear();
            Menu.SelectedIndex = -1;
            WorkPlace.Children.Add(new EmployeesWindow());
        }

        private void Pickup_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) //Метод для открытия контекстного меню
        {
            e.Handled = true;
            var mouseDownEvent = new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
            {
                RoutedEvent = Mouse.MouseUpEvent,
                Source = Pickup,
            };
            InputManager.Current.ProcessInput(mouseDownEvent);
        }

        private void AddPickup_Click(object sender, RoutedEventArgs e) //Метод для добавления пунктов самовывоза
        {
            WorkPlace.Children.Clear();
            WorkPlace.Children.Add(new MainPage());
            AddPickupAdressWindow addPickupAdress = new AddPickupAdressWindow();
            Menu.SelectedIndex = -1;
            addPickupAdress.ShowDialog();
        }

        private void PickupList_Click(object sender, RoutedEventArgs e) //Метод для открытия списка пунктов самовывоза
        {
            WorkPlace.Children.Clear();
            Menu.SelectedIndex = -1;
            WorkPlace.Children.Add(new PickupsListWindow());
        }
    }
}