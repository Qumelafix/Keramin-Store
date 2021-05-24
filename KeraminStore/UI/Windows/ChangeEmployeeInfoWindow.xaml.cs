using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace KeraminStore.UI.Windows
{
    public partial class ChangeEmployeeInfoWindow : Window
    {
        readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");

        public ChangeEmployeeInfoWindow()
        {
            InitializeComponent();
            GetPostName();
        }

        private void GetPostName()
        {
            List<string> postNames = new List<string>();
            connectionString.Open();
            string query = @"SELECT postName FROM Post";
            SqlCommand sqlCommand = new SqlCommand(query, connectionString);
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    postNames.Add(dataReader["postName"].ToString());
                    var newList = from i in postNames orderby i select i;
                    postField.ItemsSource = newList;
                }
            }
            dataReader.Close();
            connectionString.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StreamReader file = new StreamReader("ChangingEmployee.txt");
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

            if (postField.Text == string.Empty)
            {
                MessageBox.Show("Необходимо указать должность.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                int postCode = 0;

                DataTable table = new DataTable();
                dataAdapter.Fill(table);

                string selectNamesQuery = "SELECT postCode FROM Post WHERE postName = '" + postField.Text + "'";
                using (SqlDataAdapter codesDataAdapter = new SqlDataAdapter(selectNamesQuery, connectionString))
                {
                    DataTable codesTable = new DataTable();
                    codesDataAdapter.Fill(codesTable);
                    if (codesTable.Rows.Count > 0) postCode = Convert.ToInt32(codesTable.Rows[0]["postCode"].ToString());
                }

                if (table.Rows.Count > 0 && (table.Rows[0]["employeeLogin"].ToString() != loginField.Text || table.Rows[0]["employeeName"].ToString() != nameField.Text || table.Rows[0]["employeeSurname"].ToString() != surnameField.Text || table.Rows[0]["employeePatronymic"].ToString() != patronymicField.Text || table.Rows[0]["employeePassword"].ToString() != passwordField.Text || Convert.ToDateTime(table.Rows[0]["employeeDateOfBirth"].ToString()).ToShortDateString() != Convert.ToDateTime(birthdayDateField.Text).ToShortDateString() || Convert.ToInt32(table.Rows[0]["postCode"].ToString()) != postCode))
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "UPDATE Employee SET employeeLogin = @login, employeePassword = @password, employeeName = @name, employeeSurname = @surname, employeePatronymic = @patronymic, employeeDateOfBirth = @date, postCode = @post, employeeAdminStatus = @status WHERE employeeCode = @code";
                    cmd.Parameters.Add("@code", SqlDbType.Int).Value = employeeCode;
                    cmd.Parameters.Add("@login", SqlDbType.VarChar).Value = loginField.Text;
                    cmd.Parameters.Add("@password", SqlDbType.VarChar).Value = passwordField.Text;
                    cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = nameField.Text;
                    cmd.Parameters.Add("@surname", SqlDbType.VarChar).Value = surnameField.Text;
                    cmd.Parameters.Add("@patronymic", SqlDbType.VarChar).Value = patronymicField.Text;
                    if (postCode == 1) cmd.Parameters.Add("@status", SqlDbType.Bit).Value = 1;
                    else cmd.Parameters.Add("@status", SqlDbType.Bit).Value = 0;
                    cmd.Parameters.Add("@date", SqlDbType.Date).Value = birthdayDateField.SelectedDate.Value.ToShortDateString();
                    cmd.Parameters.Add("@post", SqlDbType.VarChar).Value = postCode;
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(@"ChangingEmployee.txt", string.Empty);
            this.Close();
        }
    }
}