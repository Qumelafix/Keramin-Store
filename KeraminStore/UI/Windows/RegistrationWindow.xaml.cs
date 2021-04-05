using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Data;
using System.IO;
using System;

namespace KeraminStore
{
    public partial class RegistrationWindow : UserControl
    {
        readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");

        public RegistrationWindow()
        {
            InitializeComponent();
            GetPostName();
            GetEducationName();
        }

        private void GetPostName ()
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

        private void GetEducationName()
        {
            List<string> educationNames = new List<string>();
            connectionString.Open();
            string query = @"SELECT educationName FROM Education";
            SqlCommand sqlCommand = new SqlCommand(query, connectionString);
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    educationNames.Add(dataReader["educationName"].ToString());
                    var newList = from i in educationNames orderby i select i;
                    educationField.ItemsSource = newList;
                }
            }
            dataReader.Close();
            connectionString.Close();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
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

            if (pasportField.Text != Employee.CheckEmployeePasportNumber(pasportField.Text, "Необходимо указать номер паспорта.", "В номере паспорта указаны недопустимые символы.", "Номер паспорта должен состоять из двух букв и семи цифр."))
            {
                MessageBox.Show(Employee.CheckEmployeePasportNumber(pasportField.Text, "Необходимо указать номер паспорта.", "В номере паспорта указаны недопустимые символы.", "Номер паспорта должен состоять из двух букв и семи цифр."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (educationField.Text == string.Empty)
            {
                MessageBox.Show("Необходимо указать образование.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (loginField.Text != Employee.CheckEmployeeLogin(loginField.Text, "Необходимо указать логин.", "Логин содержит недопустимые символы.", "Длина логина может составлять 3-30 символов."))
            {
                MessageBox.Show(Employee.CheckEmployeeLogin(loginField.Text, "Необходимо указать логин.", "Логин содержит недопустимые символы.", "Длина логина может составлять 3-30 символов."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (passwordField.Password.ToString() != Employee.CheckEmployeePassword(passwordField.Password.ToString(), "Необходимо указать пароль.", "Пароль содержит недопустимые символы.", "Длина пароля может составлять 6-30 символов."))
            {
                MessageBox.Show(Employee.CheckEmployeePassword(passwordField.Password.ToString(), "Необходимо указать пароль.", "Пароль содержит недопустимые символы.", "Длина пароля может составлять 6-30 символов."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

            int postCode = 0;
            int educationCode = 0;
            string selectPostCodeQuery = "SELECT postCode, educationCode FROM Post, Education WHERE postName = '" + postField.Text + "' AND educationName = '" + educationField.Text + "'";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectPostCodeQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0)
                {
                    postCode = int.Parse(table.Rows[0]["postCode"].ToString());
                    educationCode = int.Parse(table.Rows[0]["educationCode"].ToString());
                }
            }

            string selectEmployeeLoginQuery = "SELECT * FROM Employee WHERE employeeLogin = '" + loginField.Text + "'";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectEmployeeLoginQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0)
                {
                    MessageBox.Show("Этот логин занят, введите другой.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (table.Rows.Count == 0)
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT Employee (employeeLogin, employeePassword, employeeName, employeeSurname, employeePatronymic, employeePasportNumber, employeeAdminStatus, employeeDateOfBirth, postCode, educationCode) VALUES (@login, @password, @name, @surname, @patronymic, @pasport, @status, @date, @post, @education)";
                    cmd.Parameters.Add("@login", SqlDbType.VarChar).Value = loginField.Text;
                    cmd.Parameters.Add("@password", SqlDbType.VarChar).Value = passwordField.Password.ToString();
                    cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = nameField.Text;
                    cmd.Parameters.Add("@surname", SqlDbType.VarChar).Value = surnameField.Text;
                    cmd.Parameters.Add("@patronymic", SqlDbType.VarChar).Value = patronymicField.Text;
                    cmd.Parameters.Add("@pasport", SqlDbType.VarChar).Value = pasportField.Text;
                    if (postCode == 1) cmd.Parameters.Add("@status", SqlDbType.Bit).Value = 1;
                    else cmd.Parameters.Add("@status", SqlDbType.Bit).Value = 0;
                    cmd.Parameters.Add("@date", SqlDbType.Date).Value = birthdayDateField.SelectedDate.Value.ToShortDateString();
                    cmd.Parameters.Add("@post", SqlDbType.VarChar).Value = postCode;
                    cmd.Parameters.Add("@education", SqlDbType.VarChar).Value = educationCode;
                    cmd.Connection = connectionString;
                    connectionString.Open();
                    cmd.ExecuteNonQuery();
                    connectionString.Close();
                    MessageBox.Show("Регистрация успешно завершена.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearFields();
                }
            }
        }

        private void ClearFields()
        {
            loginField.Clear();
            passwordField.Clear();
            nameField.Clear();
            surnameField.Clear();
            patronymicField.Clear();
            pasportField.Clear();
            educationField.SelectedIndex = -1;
            postField.SelectedIndex = -1;
            birthdayDateField.Text = null;
            birthdayDateField.Text = "Дата рождения";
        }
    }
}