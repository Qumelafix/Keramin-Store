using KeraminStore.UI.Windows;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace KeraminStore
{
    public partial class AutorizationWindow : Window
    {
        readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");

        public AutorizationWindow()
        {
            InitializeComponent();
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
