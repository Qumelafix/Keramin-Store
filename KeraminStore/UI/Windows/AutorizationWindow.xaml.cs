using KeraminStore.UI.Windows;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Configuration;

namespace KeraminStore
{
    public partial class AutorizationWindow : Window
    {
        static string connectionString1 = ConfigurationManager.ConnectionStrings["KeraminStore.Properties.Settings.KeraminStoreConnectionString"].ConnectionString;
        //readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");
        readonly SqlConnection connectionString = new SqlConnection(connectionString1);

        public AutorizationWindow()
        {
            InitializeComponent();
            //StreamReader file = new StreamReader("ServerInfo.txt");
            //string serverName = file.ReadLine();
            //file.Close();
            //Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //ConnectionStringSettingsCollection collect = config.ConnectionStrings.ConnectionStrings;
            //ConnectionStringSettings myConnection = collect["KeraminStore.Properties.Settings.KeraminStoreConnectionString"];
            //if (myConnection == null)
            //{
            //    myConnection = new ConnectionStringSettings
            //    (
            //        "KeraminStore.Properties.Settings.KeraminStoreConnectionString",
            //        "Data Source=" + serverName + ";Initial Catalog=KeraminStore;Integrated Security=True",
            //        "System.Data.SqlClient"
            //    );
            //    collect.Add(myConnection);
            //    config.Save();
            //    ConfigurationManager.RefreshSection("connectionStrings");
                //connectionString1 = ConfigurationManager.ConnectionStrings["KeraminStore.Properties.Settings.KeraminStoreConnectionString"].ConnectionString;
                //connectionString = new SqlConnection(connectionString1);
            //}

            //RegistryKey rk = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Microsoft SQL Server");
            //String[] instances = (String[])rk.GetValue("InstalledInstances");
            //if (instances.Length > 0)
            //{
            //    foreach (String element in instances)
            //    {
            //        if (element == "MSSQLSERVER")
            //           MessageBox.Show(Environment.MachineName.ToString());
            //        else
            //            Console.WriteLine(Environment.MachineName.ToString() + @"\" + element.ToString());

            //    }
            //    MessageBox.Show("\n");
            //}

            string path = Environment.CurrentDirectory.ToString();
            string subPath = @"ProductImages";
            string subPath2 = @"OrdersDocuments";
            string subPath3 = @"EmployeesStatisticsDocuments";
            string subPath4 = @"ProductsStatisticsDocuments";
            DirectoryInfo dirInfo = new DirectoryInfo(path);
            if (dirInfo.Exists)
            {
                dirInfo.CreateSubdirectory(subPath);
                dirInfo.CreateSubdirectory(subPath2);
                dirInfo.CreateSubdirectory(subPath3);
                dirInfo.CreateSubdirectory(subPath4);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => Application.Current.Shutdown();

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = userLogin.Text;
            string password = userPassword.Password.ToString();

            string selectEmployeeInfoQuery = "SELECT * FROM Employee WHERE employeeLogin = '" + login + "'and employeePassword ='" + password + "'";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectEmployeeInfoQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0)
                {
                    if (table.Rows[0]["employeeLogin"].ToString() == login && table.Rows[0]["employeePassword"].ToString() == password)
                    {
                        StreamWriter loginFile = new StreamWriter("UserCode.txt");
                        loginFile.Write(table.Rows[0]["employeeCode"].ToString());
                        loginFile.Close();

                        MainWindow mainWindow = new MainWindow();
                        mainWindow.WorkPlace.Children.Add(new MainPage());
                        mainWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
                else if (table.Rows.Count == 0)
                {
                    MessageBox.Show("Неверный логин или пароль.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();
    }
}
