using KeraminStore.UI.Windows;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace KeraminStore
{
    public partial class MainWindow : Window
    {
        readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");

        public MainWindow()
        {
            InitializeComponent();

            //AddEmployees.Visibility = Visibility.Hidden;
            //ChangeEmployeesInfo.Visibility = Visibility.Hidden;

            //StreamReader file = new StreamReader("UserLogin.txt");
            //string employeeLogin = file.ReadLine();
            //file.Close();
            //login.Text = employeeLogin;

            //string post = string.Empty;
            //string selectEmployeePostQuery = "SELECT postName FROM Employee JOIN Post ON Employee.postCode = Post.postCode WHERE employeeLogin = '" + login.Text + "'";
            //using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectEmployeePostQuery, myConnectionString))
            //{
            //    DataTable table = new DataTable();
            //    dataAdapter.Fill(table);
            //    if (table.Rows.Count > 0)
            //    {
            //        post = table.Rows[0]["postName"].ToString();
            //    }
            //}
            //if (post == "Администратор")
            //{
            //    AddEmployees.Visibility = Visibility.Visible;
            //    ChangeEmployeesInfo.Visibility = Visibility.Visible;
            //}
            //else
            //{
            //    Height = 575;
            //}
        }

        private void ButtonPopUpLogout_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
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

        private void Grid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

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

        private void ComponentsInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //ComponentsInfo componentsInfo = new ComponentsInfo();
            //componentsInfo.Show();
            //this.Close();
        }

        private void AddComponents_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //AddComponentsWindow addComponents = new AddComponentsWindow();
            //addComponents.Show();
            //this.Close();
        }

        private void AddEmployees_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //RegistrationWindow registrationWindow = new RegistrationWindow();
            //registrationWindow.Title.Content = "Add employees";
            //registrationWindow.Description.Content = "Fill the fields below to adding employee";
            //registrationWindow.RegisterButton.Content = "ADD EMPLOYEE";
            //registrationWindow.Account.Visibility = Visibility.Hidden;
            //registrationWindow.SingInButton.Visibility = Visibility.Hidden;
            //registrationWindow.successfulRegistration = "Adding employee is successful!";
            //registrationWindow.Show();
            //this.Close();
        }

        private void ChangeComponentsInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //ChangeComponentsInfoWindow changeComponentsInfoWindow = new ChangeComponentsInfoWindow();
            //changeComponentsInfoWindow.Show();
            //this.Close();
        }

        private void ChangeEmployeesInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //ChangeEmployeesInfoWindow changeEmployeesInfoWindow = new ChangeEmployeesInfoWindow();
            //changeEmployeesInfoWindow.Show();
            //this.Close();
        }

        private void CreateConsignmentNote_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //AddToConsignmentNoteWindow addToConsignmentNoteWindow = new AddToConsignmentNoteWindow();
            //addToConsignmentNoteWindow.Show();
            //this.Close();
        }

        private void AddConsumers_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //AddConsumersWindow addConsumersWindow = new AddConsumersWindow();
            //addConsumersWindow.Show();
            //this.Close();
        }

        private void ChangeConsumersInfo_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //ChangeConsumersInfoWindow changeConsumersInfoWindow = new ChangeConsumersInfoWindow();
            //changeConsumersInfoWindow.Show();
            //this.Close();
        }

        private void CreateReport_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //CreateReportWindow createReportWindow = new CreateReportWindow();
            //createReportWindow.Show();
            //this.Close();
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            //string post = string.Empty;
            //string selectEmployeePostQuery = "SELECT postName FROM Employee JOIN Post ON Employee.postCode = Post.postCode WHERE employeeLogin = '" + login.Text + "'";
            //using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectEmployeePostQuery, myConnectionString))
            //{
            //    DataTable table = new DataTable();
            //    dataAdapter.Fill(table);
            //    if (table.Rows.Count > 0)
            //    {
            //        post = table.Rows[0]["postName"].ToString();
            //    }
            //}
            //if (post == "Администратор")
            //{
            //    HelpNavigator navigator = HelpNavigator.Topic;
            //    Help.ShowHelp(null, "help.chm", navigator, "rukovodstvo_administratora.htm");
            //}
            //else
            //{
            //    HelpNavigator navigator = HelpNavigator.Topic;
            //    Help.ShowHelp(null, "help.chm", navigator, "rukovodstvo_sotrudnika.htm");
            //}
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
                case 3:
                    WorkPlace.Children.Clear();
                    Menu.SelectedIndex = -1;
                    WorkPlace.Children.Add(new AddNewProductWindow());
                    break;
                case 4:
                    WorkPlace.Children.Clear();
                    WorkPlace.Children.Add(new MainPage());
                    ProductsListWindow productsListWindow = new ProductsListWindow();
                    Menu.SelectedIndex = -1;
                    productsListWindow.ShowDialog();
                    break;

                case 7:
                    WorkPlace.Children.Clear();
                    Menu.SelectedIndex = -1;
                    WorkPlace.Children.Add(new RegistrationWindow());
                    break;
                case 8:
                    WorkPlace.Children.Clear();
                    WorkPlace.Children.Add(new MainPage());
                    StaffWindow staffWindow = new StaffWindow();
                    Menu.SelectedIndex = -1;
                    staffWindow.ShowDialog();
                    break;

                default:
                    break;
            }
        }
    }
}
