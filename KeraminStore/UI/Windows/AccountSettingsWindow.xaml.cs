using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace KeraminStore.UI.Windows
{
    public partial class AccountSettingsWindow : UserControl
    {
        static string connectionString1 = ConfigurationManager.ConnectionStrings["KeraminStore.Properties.Settings.KeraminStoreConnectionString"].ConnectionString;
        //readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");
        readonly SqlConnection connectionString = new SqlConnection(connectionString1);

        public AccountSettingsWindow()
        {
            InitializeComponent();

            StreamReader file = new StreamReader("UserCode.txt");
            int employeeCode = Convert.ToInt32(file.ReadLine());
            file.Close();

            int postCode = 0;
            string selectEmployeeInfo = "SELECT * FROM Employee WHERE employeeCode = " + employeeCode + "";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectEmployeeInfo, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0)
                {
                    loginField.Text = table.Rows[0]["employeeLogin"].ToString();
                    passwordField.Text = table.Rows[0]["employeePassword"].ToString();
                    nameField.Text = table.Rows[0]["employeeName"].ToString();
                    surnameField.Text = table.Rows[0]["employeeSurname"].ToString();
                    patronymicField.Text = table.Rows[0]["employeePatronymic"].ToString();
                    birthdayDateField.Text = table.Rows[0]["employeeDateOfBirth"].ToString();
                    postCode = int.Parse(table.Rows[0]["postCode"].ToString());
                    if (Convert.ToBoolean(table.Rows[0]["employeeAdminStatus"].ToString()) == false) postField.IsEnabled = false;
                }
            }

            string selectNamesQuery = "SELECT postName FROM Post WHERE postCode = " + postCode + "";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectNamesQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0) postField.Text = table.Rows[0]["postName"].ToString();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StreamReader file = new StreamReader("UserCode.txt");
            int employeeCode = Convert.ToInt32(file.ReadLine());
            file.Close();

            string employeeLogin = string.Empty;

            string selectEmployeeInfoQuery = "SELECT employeeLogin FROM Employee WHERE employeeCode = " + employeeCode + "";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectEmployeeInfoQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0) employeeLogin = table.Rows[0]["employeeLogin"].ToString();
            }

            if (surnameField.Text != Employee.CheckEmployeeFullName(surnameField.Text, "Фамилия не может быть пустой.", "Фамилия содержит недопустимые символы.", "Длина фамилии может составлять 2-50 символов."))
            {
                MessageBox.Show(Employee.CheckEmployeeFullName(surnameField.Text, "Фамилия не может быть пустой.", "Фамилия содержит недопустимые символы.", "Длина фамилии может составлять 2-50 символов."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (nameField.Text != Employee.CheckEmployeeFullName(nameField.Text, "Имя не может быть пустым.", "Имя содержит недопустимые символы.", "Длина имени может составлять 2-50 символов."))
            {
                MessageBox.Show(Employee.CheckEmployeeFullName(nameField.Text, "Имя не может быть пустым.", "Имя содержит недопустимые символы.", "Длина имени может составлять 2-50 символов."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (patronymicField.Text != Employee.CheckEmployeeFullName(patronymicField.Text, "Отчество не может быть пустым.", "Отчество содержит недопустимые символы.", "Длина отчества может составлять 2-50 символов."))
            {
                MessageBox.Show(Employee.CheckEmployeeFullName(patronymicField.Text, "Отчество не может быть пустым.", "Отчество содержит недопустимые символы.", "Длина отчества может составлять 2-50 символов."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (loginField.Text != Employee.CheckEmployeeLogin(loginField.Text, "Необходимо указать логин.", "Логин содержит недопустимые символы.", "Длина логина может составлять 3-30 символов."))
            {
                MessageBox.Show(Employee.CheckEmployeeLogin(loginField.Text, "Необходимо указать логин.", "Логин содержит недопустимые символы.", "Длина логина может составлять 3-30 символов."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string selectUniqEmployeeLoginQuery = "SELECT * FROM Employee WHERE employeeLogin= '" + loginField.Text + "'";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectUniqEmployeeLoginQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0 && table.Rows[0]["employeeLogin"].ToString() != employeeLogin)
                {
                    MessageBox.Show("Этот логин занят, выберите другой.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (passwordField.Text != Employee.CheckEmployeePassword(passwordField.Text, "Необходимо указать пароль.", "Пароль должен: \n- содержать минимум одну прописную букву латинского алфавита; \n- содержать минимум одну заглавную букву латинского алфавита; " +
                                           "\n- содержать минимум одну латинскую цифру; " +
                                           "\n- состоять из 6-30 символов;"))
            {
                MessageBox.Show(Employee.CheckEmployeePassword(passwordField.Text, "Необходимо указать пароль.", "Пароль должен: \n- содержать минимум одну прописную букву латинского алфавита; \n- содержать минимум одну заглавную букву латинского алфавита; " +
                                           "\n- содержать минимум одну латинскую цифру; " +
                                           "\n- состоять из 6-30 символов;"), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (birthdayDateField.SelectedDate == null)
            {
                MessageBox.Show("Необходимо указать дату рождения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (birthdayDateField.SelectedDate.Value.ToShortDateString() != Employee.CheckEmployeeBirthdayDate(birthdayDateField.SelectedDate.Value, "Возраст сотрудника должен быть не менее восемнадцати лет."))
            {
                MessageBox.Show(Employee.CheckEmployeeBirthdayDate(birthdayDateField.SelectedDate.Value, "Возраст сотрудника должен быть не менее восемнадцати лет."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string alterationQuery = "SELECT * FROM Employee WHERE employeeCode = '" + employeeCode + "'";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(alterationQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);

                if (table.Rows.Count > 0 && (table.Rows[0]["employeeLogin"].ToString() != loginField.Text || table.Rows[0]["employeeName"].ToString() != nameField.Text || table.Rows[0]["employeeSurname"].ToString() != surnameField.Text || table.Rows[0]["employeePatronymic"].ToString() != patronymicField.Text || table.Rows[0]["employeePassword"].ToString() != passwordField.Text || Convert.ToDateTime(table.Rows[0]["employeeDateOfBirth"].ToString()).ToShortDateString() != Convert.ToDateTime(birthdayDateField.Text).ToShortDateString()))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "UPDATE Employee SET employeeLogin = @login, employeePassword = @password, employeeName = @name, employeeSurname = @surname, employeePatronymic = @patronymic, employeeDateOfBirth = @date WHERE employeeCode = @code";
                    cmd.Parameters.Add("@code", SqlDbType.Int).Value = employeeCode;
                    cmd.Parameters.Add("@login", SqlDbType.VarChar).Value = loginField.Text;
                    cmd.Parameters.Add("@password", SqlDbType.VarChar).Value = passwordField.Text;
                    cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = nameField.Text;
                    cmd.Parameters.Add("@surname", SqlDbType.VarChar).Value = surnameField.Text;
                    cmd.Parameters.Add("@patronymic", SqlDbType.VarChar).Value = patronymicField.Text;
                    cmd.Parameters.Add("@date", SqlDbType.Date).Value = birthdayDateField.SelectedDate.Value.ToShortDateString();
                    cmd.Connection = connectionString;
                    connectionString.Open();
                    cmd.ExecuteNonQuery();
                    connectionString.Close();
                    MessageBox.Show("Изменения сохранены.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show("Вы не внесли никаких изменений.", "Преупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }
        }
    }
}