using KeraminStore.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace KeraminStore.UI.Windows
{
    public partial class CreateOrderWindow : Window
    {
        static string connectionString1 = ConfigurationManager.ConnectionStrings["KeraminStore.Properties.Settings.KeraminStoreConnectionString"].ConnectionString;
        readonly SqlConnection connectionString = new SqlConnection(connectionString1);
        public CreateOrderWindow()
        {
            InitializeComponent();
            GetDeliveryInfo(); //Вызов методов для заполнения комбобоксов
            GetStreetInfo();
            GetPaymentInfo();
            connectionString.Open();
            FillDataGrid();
            connectionString.Close();
            if (ProductsInfoGrid.Items.Count > 0) OrderButton.IsEnabled = true;
        }

        private void selfDeliveryTown_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) //Метод для закрузки адресов по выбранному городу (самовывоз)
        {
            selfDeliveryAdress.SelectedIndex = -1;
            List<string> adress = new List<string>();
            connectionString.Close();
            connectionString.Open();
            if (selfDeliveryTown.SelectedItem != null)
            {
                string query = @"SELECT streetName, building FROM Pickup JOIN PickupTown ON Pickup.pickupTownCode = PickupTown.pickupTownCode WHERE pickupTownName = '" + selfDeliveryTown.SelectedItem.ToString() + "'";
                SqlCommand sqlCommand = new SqlCommand(query, connectionString);
                SqlDataReader dataReader = sqlCommand.ExecuteReader();
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        adress.Add("ул. " + dataReader["streetName"].ToString() + ", " + dataReader["building"].ToString()); //формирование полного адреса
                        var newList = from i in adress orderby i select i;
                        selfDeliveryAdress.ItemsSource = newList;
                    }
                }
                else selfDeliveryAdress.ItemsSource = null;
                dataReader.Close();
            }      
            connectionString.Close();
        }

        private void GetStreetInfo() //Метод для получения улиц
        {
            List<string> streetNames = new List<string>();
            connectionString.Open();
            string query = @"SELECT streetName FROM Street";
            SqlCommand sqlCommand = new SqlCommand(query, connectionString);
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    streetNames.Add(dataReader["streetName"].ToString());
                    var newList = from i in streetNames orderby i select i;
                    deliveryStreet.ItemsSource = newList;
                }
            }
            dataReader.Close();
            connectionString.Close();
        }

        private void GetPaymentInfo() //Метод для получения типа оплаты
        {
            List<string> paymentNames = new List<string>();
            connectionString.Open();
            string query = @"SELECT paymentType FROM Payment";
            SqlCommand sqlCommand = new SqlCommand(query, connectionString);
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    paymentNames.Add(dataReader["paymentType"].ToString());
                    var newList = from i in paymentNames orderby i select i;
                    payment.ItemsSource = newList;
                }
            }
            dataReader.Close();
            connectionString.Close();
        }

        private void GetDeliveryInfo() //Метод для получения типа доставки
        {
            List<string> deliveryNames = new List<string>();
            connectionString.Open();
            string query = @"SELECT deliveryName FROM Delivery";
            SqlCommand sqlCommand = new SqlCommand(query, connectionString);
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    deliveryNames.Add(dataReader["deliveryName"].ToString());
                    var newList = from i in deliveryNames orderby i select i;
                    delivery.ItemsSource = newList;
                }
            }
            dataReader.Close();
            connectionString.Close();
        }

        private void FillDataGrid() //Метод для заполения таблицы
        {
            StreamReader streamReader = new StreamReader("BasketNumber.txt");
            string basketNumber = streamReader.ReadLine();
            streamReader.Close();
            string productsInfoQuery = "SELECT productName, productImage, productArticle, productCostCount, productCostArea, boxesCount, CONCAT(boxesCount, ' | ', productsCount, ' | ', productsArea) as 'count', productsCount, productsArea, productsWeight, generalSum " +
                                       "FROM Basket " +
                                       "JOIN Product ON Basket.productCode = Product.productCode WHERE basketNumber = '" + basketNumber + "'";
            using (SqlDataAdapter adapter = new SqlDataAdapter(productsInfoQuery, connectionString))
            {
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                if (dataTable.Rows.Count >= 0)
                {
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(productsInfoQuery, connectionString))
                    {
                        DataTable table = new DataTable();
                        dataAdapter.Fill(table);
                        if (table.Rows.Count > 0) ProductsInfoGrid.ItemsSource = table.DefaultView;
                    }
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();

        private void OrderButton_Click(object sender, RoutedEventArgs e) //Метод для началана оформления заказа
        {
            windowName.Content = "Оформление заказа";
            windowDescription.Content = "Заполните все необходимые поля для оформления заказа";
            ProductsInfoGrid.Columns[11].Visibility = Visibility.Hidden;
            double calculateSum = 0;
            if (sum.Text == string.Empty) sum.Text = "0";
            for (int i = 0; i < ProductsInfoGrid.Items.Count; i++)
            {
                DataRowView productInfo = (DataRowView)ProductsInfoGrid.Items[i];
                calculateSum = Math.Round(double.Parse(sum.Text) + double.Parse(productInfo["generalSum"].ToString()), 2);
                sum.Text = calculateSum.ToString();
            }
            Height = 1000;
            OrderButton.IsEnabled = false;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) //Метод для удаления выбранного изделия из корзины
        {
            StreamReader streamReader = new StreamReader("BasketNumber.txt");
            string basketNumber = streamReader.ReadLine(); //Считывание номера заказа
            streamReader.Close();
            if (ProductsInfoGrid.SelectedItem == null)
            {
                MessageBox.Show("Невозможно удалить изделие из корзины, так как оно не было выбрано.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                DataRowView productInfo = (DataRowView)ProductsInfoGrid.SelectedItems[0]; //Получение информации о выбранном изделии
                string selectProductCount = "SELECT productsCount, productCount FROM Basket " +
                                            "JOIN Product ON Basket.productCode = Product.productCode " +
                                            "WHERE productArticle = '" + productInfo["productArticle"].ToString() + "' AND basketNumber = '" + basketNumber + "'";
                using (SqlDataAdapter countAdapter = new SqlDataAdapter(selectProductCount, connectionString)) //
                {
                    DataTable dataTable = new DataTable();
                    countAdapter.Fill(dataTable);
                    if (dataTable.Rows.Count > 0)
                    {
                        int productCount = int.Parse(dataTable.Rows[0]["productCount"].ToString());
                        int productsCount = int.Parse(dataTable.Rows[0]["productsCount"].ToString());
                        SqlCommand updateProductInfoCmd = new SqlCommand();
                        updateProductInfoCmd.CommandType = CommandType.Text;
                        updateProductInfoCmd.CommandText = "UPDATE Product SET productCount = @count, availabilityStatusCode = @status WHERE productArticle = @article";
                        updateProductInfoCmd.Parameters.Add("@count", SqlDbType.Int).Value = productCount + productsCount; //Возвращение количества изделий из корзины
                        updateProductInfoCmd.Parameters.Add("@status", SqlDbType.Int).Value = 1;
                        updateProductInfoCmd.Parameters.Add("@article", SqlDbType.VarChar).Value = productInfo["productArticle"].ToString();
                        updateProductInfoCmd.Connection = connectionString;
                        connectionString.Open();
                        updateProductInfoCmd.ExecuteNonQuery();
                        connectionString.Close();
                    }
                }

                SqlCommand deleteCmd = new SqlCommand();
                deleteCmd.CommandType = CommandType.Text;
                deleteCmd.CommandText = "DELETE Basket FROM Basket INNER JOIN Product ON Basket.productCode = Product.productCode WHERE productArticle = @article AND basketNumber = @number";
                deleteCmd.Parameters.Add("@article", SqlDbType.VarChar).Value = productInfo["productArticle"].ToString();
                deleteCmd.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                deleteCmd.Connection = connectionString;
                connectionString.Open();
                deleteCmd.ExecuteNonQuery(); //Удаление издлеия из корзины
                ProductsInfoGrid.ItemsSource = null;
                ProductsInfoGrid.Items.Refresh();
                FillDataGrid();
                connectionString.Close();
                MessageBox.Show("Удаление успешно выполнено.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                if (ProductsInfoGrid.Items.Count == 0) OrderButton.IsEnabled = false;
            }
        }

        [DllImport("user32")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId0);

        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();

        private void legalCustomer_Click(object sender, RoutedEventArgs e)
        {
            legalCustomerInfo.Visibility = Visibility.Visible;
            customerInfo.Visibility = Visibility.Hidden;
        }

        private void сustomer_Click(object sender, RoutedEventArgs e)
        {
            legalCustomerInfo.Visibility = Visibility.Hidden;
            customerInfo.Visibility = Visibility.Visible;
        }

        private void delivery_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e) //Метод для отображения стоимости доставки
        {
            List<string> townNames = new List<string>();
            if (delivery.SelectedItem.ToString() == "Самовывоз")
            {
                deliveryCost.Text = "0";
                deliveryCost.IsReadOnly = true;
                selfDeliveryInfo.Visibility = Visibility.Visible;
                deliveryInfo.Visibility = Visibility.Hidden;
                deliveryTown.SelectedIndex = -1;
                deliveryStreet.SelectedIndex = -1;
                deliveryApartment.Clear();
                deliveryBuilding.Clear();
                deliveryFloor.Clear();
                string query = @"SELECT pickupTownName FROM PickupTown";
                connectionString.Open();
                SqlCommand sqlCommand = new SqlCommand(query, connectionString);
                SqlDataReader dataReader = sqlCommand.ExecuteReader();
                if (dataReader.HasRows)
                {
                    while (dataReader.Read())
                    {
                        townNames.Add(dataReader["pickupTownName"].ToString());
                        var newList = from i in townNames orderby i select i;
                        selfDeliveryTown.ItemsSource = newList;
                    }
                }
                dataReader.Close();
            }
            else
            {
                if (delivery.Text == string.Empty || deliveryTown.Items.Count == 0)
                {
                    deliveryCost.IsReadOnly = false;
                    deliveryCost.Text = "15";
                    selfDeliveryInfo.Visibility = Visibility.Hidden;
                    deliveryInfo.Visibility = Visibility.Visible;
                    selfDeliveryAdress.SelectedIndex = -1;
                    selfDeliveryTown.SelectedIndex = -1;
                    string query = @"SELECT townName FROM Town";
                    connectionString.Open();
                    SqlCommand sqlCommand = new SqlCommand(query, connectionString);
                    SqlDataReader dataReader = sqlCommand.ExecuteReader();
                    if (dataReader.HasRows)
                    {
                        while (dataReader.Read())
                        {
                            townNames.Add(dataReader["townName"].ToString());
                            var newList = from i in townNames orderby i select i;
                            deliveryTown.ItemsSource = newList;
                        }
                    }
                    dataReader.Close();
                }
                else
                {
                    deliveryCost.IsReadOnly = false;
                    deliveryCost.Text = "15";
                    selfDeliveryInfo.Visibility = Visibility.Hidden;
                    deliveryInfo.Visibility = Visibility.Visible;
                    selfDeliveryAdress.SelectedIndex = -1;
                    selfDeliveryTown.SelectedIndex = -1;
                }
            }
            connectionString.Close();
        }

        private void CreateOrderButton_Click(object sender, RoutedEventArgs e) //Метод для оформления заказа
        {
            if (legalCustomer.IsChecked == false && usualCustomer.IsChecked == false)
            {
                MessageBox.Show("Вы не указали, кем является покупатель.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (legalCustomer.IsChecked == true) //Если покупатель является юр.лицом
            {
                if (legalCustomerName.Text == string.Empty) //Проверка названия организации
                {
                    MessageBox.Show("Вы не указали название организации.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    if (legalCustomerName.Text.Length >= 10 && legalCustomerName.Text.Length <= 100)
                    {
                        char[] organizationNameArray = legalCustomerName.Text.ToCharArray();
                        for (int i = 0; i < organizationNameArray.Length; i++)
                        {
                            if (!char.IsLetterOrDigit(organizationNameArray[i]) && organizationNameArray[i] != ' ' && organizationNameArray[i] != '"')
                            {
                                MessageBox.Show("Вы указали неверные символы в названии организации.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Длина наименования орагнизации может составлять 10-100 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (legalCustomerUTN.Text == string.Empty) //Проверка ИНН
                {
                    MessageBox.Show("Вы не указали ИНН.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    if (legalCustomerUTN.Text.Length == 9)
                    {
                        char[] UTNArray = legalCustomerUTN.Text.ToCharArray();
                        for (int i = 0; i < UTNArray.Length; i++)
                        {
                            if (!char.IsDigit(UTNArray[i]))
                            {
                                MessageBox.Show("Вы указали неверные символы в ИНН.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Длина ИНН может составлять 9 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (legalCustomerPhone.Text == string.Empty) //Проверка номера телефона
                {
                    MessageBox.Show("Вы не указали контактный телефон.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    Regex regex = new Regex(@"^(\+375|80)\(\s?(44|25|33|17|29)\s?\)\d{3}\W[-]?\s?\d{2}\W[-]?\s?\d{2}$");
                    if (!regex.IsMatch(legalCustomerPhone.Text))
                    {
                        MessageBox.Show("Телефон(должен быть записан в формате +375/80 (код оператора) XXX-XX-XX.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (legalCustomerMail.Text == string.Empty) //Проверка почты
                {
                    MessageBox.Show("Вы не указали e-mail.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    Regex regex = new Regex(@"^\s*[\w\-\+_']+(\.[\w\-\+_']+)*\@[A-Za-z0-9]([\w\.-]*[A-Za-z0-9])?\.[A-Za-z][A-Za-z\.]*[A-Za-z]$");
                    if (!regex.IsMatch(legalCustomerMail.Text))
                    {
                        MessageBox.Show("Вы указали неверный e-mail.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

            }
            else if (usualCustomer.IsChecked == true) //Если покупатель является физ.лицом
            {
                if (customerName.Text != Employee.CheckEmployeeFullName(customerName.Text, "Имя не может быть пустым.", "Имя содержит недопустимые символы.", "Длина имени может составлять 2-50 символов.")) //Проверка имени покупателя
                {
                    MessageBox.Show(Employee.CheckEmployeeFullName(customerName.Text, "Имя не может быть пустым.", "Имя содержит недопустимые символы.", "Длина имени может составлять 2-50 символов."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (customerSurname.Text != Employee.CheckEmployeeFullName(customerSurname.Text, "Фамилия не может быть пустой.", "Фамилия содержит недопустимые символы.", "Длина фамилии может составлять 2-50 символов.")) //Проверка фамилии покупателя
                {
                    MessageBox.Show(Employee.CheckEmployeeFullName(customerSurname.Text, "Фамилия не может быть пустой.", "Фамилия содержит недопустимые символы.", "Длина фамилии может составлять 2-50 символов."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (customerPatronymic.Text != Employee.CheckEmployeeFullName(customerPatronymic.Text, "Отчество не может быть пустым.", "Отчество содержит недопустимые символы.", "Длина отчества может составлять 2-50 символов.")) //Проверка отчества покупателя
                {
                    MessageBox.Show(Employee.CheckEmployeeFullName(customerPatronymic.Text, "Отчество не может быть пустым.", "Отчество содержит недопустимые символы.", "Длина отчества может составлять 2-50 символов."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (customerPhone.Text == string.Empty) //Проверка телефона покупателя
                {
                    MessageBox.Show("Вы не указали контактный телефон.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    Regex regex = new Regex(@"^(\+375|80)\(\s?(44|25|33|17|29)\s?\)\d{3}\W[-]?\s?\d{2}\W[-]?\s?\d{2}$");
                    if (!regex.IsMatch(customerPhone.Text))
                    {
                        MessageBox.Show("Телефон(должен быть записан в формате +375/80 (код оператора) XXX-XX-XX.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (customerMail.Text == string.Empty) //Проверка почты покупателя
                {
                    MessageBox.Show("Вы не указали e-mail.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    if (customerMail.Text.Length > 40)
                    {
                        MessageBox.Show("Длина e-mail должна составлять до 40 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        Regex regex = new Regex(@"^\s*[\w\-\+_']+(\.[\w\-\+_']+)*\@[A-Za-z0-9]([\w\.-]*[A-Za-z0-9])?\.[A-Za-z][A-Za-z\.]*[A-Za-z]$");
                        if (!regex.IsMatch(customerMail.Text))
                        {
                            MessageBox.Show("Вы указали неверный e-mail.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                }
            }

            if (delivery.SelectedIndex == -1) //Проверка выбора способа доставки
            {
                MessageBox.Show("Вам необходимо выбрать способ доставки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (delivery.SelectedItem.ToString() == "Самовывоз")
            {
                if (selfDeliveryTown.Text == string.Empty)
                {
                    MessageBox.Show("Необходимо указать город для самовывоза.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (selfDeliveryAdress.Text == string.Empty)
                {
                    MessageBox.Show("Необходимо указать адрес для самовывоза.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                if (deliveryTown.Text == string.Empty)
                {
                    MessageBox.Show("Необходимо указать город.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (deliveryStreet.Text == string.Empty)
                {
                    MessageBox.Show("Необходимо указать улицу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Regex regex = new Regex(@"^(\d{1,3}|\d{1,3}\/\d{1,2}|\d{1,3}(?:[а-я])|\d{1,3}(?:[а-я])\/\d{1,2}|\d{1,3}\/\d{1,2}(?:[а-я]))$");
                if (!regex.IsMatch(deliveryBuilding.Text))
                {
                    MessageBox.Show("Номер здания должен выглядеть как 15 | 15а | 15/1 | 15а/1 | 15/1а.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                regex = new Regex(@"^\d{1,2}$");
                if (!regex.IsMatch(deliveryFloor.Text) || (Convert.ToInt32(deliveryFloor.Text) <= 0 || Convert.ToInt32(deliveryFloor.Text) > 32))
                {
                    MessageBox.Show("Этаж должен быть числовой величиной с максимальным значением, равным 32.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                regex = new Regex(@"^\d{1,3}$");
                if (!regex.IsMatch(deliveryApartment.Text) || Convert.ToInt32(deliveryApartment.Text) <= 0)
                {
                    MessageBox.Show("Номер квартиры/офиса должен быть числовой величиной с максимальной длиной в 3 символа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (payment.SelectedIndex == -1)
            {
                MessageBox.Show("Вы не указали способ оплаты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int streetCode = 0;
            int townCode = 0;
            int customerCode = 0;
            int deliveryCode = 0;
            int paymentCode = 0;
            int productCode = 0;
            int basketCode = 0;
            int pickupCode = 0;

            StreamReader streamReader = new StreamReader("BasketNumber.txt");
            string basketNumber = streamReader.ReadLine(); //Считывание номера заказа
            streamReader.Close();

            StreamReader file = new StreamReader("UserCode.txt");
            int employeeCode = Convert.ToInt32(file.ReadLine()); //Считывание кода сотрудника, оформляющего заказ
            file.Close();

            Microsoft.Office.Interop.Excel.Workbooks excelappworkbooks; //Формирование структуры документа Excel с указанием пути сохранения
            Microsoft.Office.Interop.Excel.Workbook excelappworkbook;
            Microsoft.Office.Interop.Excel.Sheets excelsheets;
            Microsoft.Office.Interop.Excel.Worksheet excelworksheet;
            Microsoft.Office.Interop.Excel.Range excelcells;
            string path = Environment.CurrentDirectory + "\\OrdersDocuments";
            Microsoft.Office.Interop.Excel.Application excelapp = new Microsoft.Office.Interop.Excel.Application();
            excelapp.Interactive = false;
            uint processId;
            GetWindowThreadProcessId((IntPtr)excelapp.Hwnd, out processId);

            if (usualCustomer.IsChecked == true)
            {
                if (delivery.SelectedItem.ToString() == "Самовывоз") //Запись данных о покупателе в таблицу
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT Customer (orderNumber, customerName, customerSurname, customerPatronymic, legalName, UTN, phone, mail, building, floor_, apartment, townCode, streetCode) " +
                                      "VALUES (@number, @cusName, @cusSurname, @cusPatronymic, @legalName, @utn, @phone, @mail, @building, @floor, @apartment, @townCode, @streetCode)";
                    cmd.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                    cmd.Parameters.Add("@cusName", SqlDbType.VarChar).Value = customerName.Text;
                    cmd.Parameters.Add("@cusSurname", SqlDbType.VarChar).Value = customerSurname.Text;
                    cmd.Parameters.Add("@cusPatronymic", SqlDbType.VarChar).Value = customerPatronymic.Text;
                    cmd.Parameters.Add("@legalName", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@utn", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@phone", SqlDbType.VarChar).Value = customerPhone.Text;
                    cmd.Parameters.Add("@mail", SqlDbType.VarChar).Value = customerMail.Text;
                    cmd.Parameters.Add("@building", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@floor", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@apartment", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@townCode", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@streetCode", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Connection = connectionString;
                    connectionString.Open();
                    cmd.ExecuteNonQuery();
                    connectionString.Close();
                }
                else //Запись данных о покупателе в таблицу
                {
                    string selectAdressCodesQuery = "SELECT townCode, streetCode FROM Town, Street WHERE townName = '" + deliveryTown.Text + "' AND streetName = '" + deliveryStreet.Text + "'";
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectAdressCodesQuery, connectionString))
                    {
                        DataTable table = new DataTable();
                        dataAdapter.Fill(table);
                        if (table.Rows.Count > 0)
                        {
                            townCode = int.Parse(table.Rows[0]["townCode"].ToString());
                            streetCode = int.Parse(table.Rows[0]["streetCode"].ToString());
                        }
                    }

                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT Customer (orderNumber, customerName, customerSurname, customerPatronymic, legalName, UTN, phone, mail, building, floor_, apartment, townCode, streetCode) " +
                                      "VALUES (@number, @cusName, @cusSurname, @cusPatronymic, @legalName, @utn, @phone, @mail, @building, @floor, @apartment, @townCode, @streetCode)";
                    cmd.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                    cmd.Parameters.Add("@cusName", SqlDbType.VarChar).Value = customerName.Text;
                    cmd.Parameters.Add("@cusSurname", SqlDbType.VarChar).Value = customerSurname.Text;
                    cmd.Parameters.Add("@cusPatronymic", SqlDbType.VarChar).Value = customerPatronymic.Text;
                    cmd.Parameters.Add("@legalName", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@utn", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@phone", SqlDbType.VarChar).Value = customerPhone.Text;
                    cmd.Parameters.Add("@mail", SqlDbType.VarChar).Value = customerMail.Text;
                    cmd.Parameters.Add("@building", SqlDbType.VarChar).Value = deliveryBuilding.Text;
                    cmd.Parameters.Add("@floor", SqlDbType.Int).Value = int.Parse(deliveryFloor.Text);
                    cmd.Parameters.Add("@apartment", SqlDbType.Int).Value = int.Parse(deliveryApartment.Text);
                    cmd.Parameters.Add("@townCode", SqlDbType.Int).Value = townCode;
                    cmd.Parameters.Add("@streetCode", SqlDbType.Int).Value = streetCode;
                    cmd.Connection = connectionString;
                    connectionString.Open();
                    cmd.ExecuteNonQuery();
                    connectionString.Close();
                }

                if (delivery.SelectedItem.ToString() == "Самовывоз")
                {
                    string adress = selfDeliveryAdress.Text.ToString().Replace("ул. ", "");
                    adress = adress.Replace(", ", ",");
                    string[] adressArray = adress.Split(',');

                    string selectCustomerCodeQuery = "SELECT customerCode, deliveryCode, paymentCode, pickupCode FROM Customer, Delivery, Payment, Pickup " +
                                                 "JOIN PickupTown ON Pickup.pickupTownCode = PickupTown.pickupTownCode " +
                                                 "WHERE phone = '" + customerPhone.Text + "' AND orderNumber = '" + basketNumber + "' AND pickupTownName = '" + selfDeliveryTown.Text + "' AND Pickup.streetName = '" + adressArray[0] + "' " +
                                                 "AND Pickup.building = '" + adressArray[1] + "' AND deliveryName = '" + delivery.SelectedItem.ToString() + "' AND paymentType = '" + payment.SelectedItem.ToString() + "'";
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectCustomerCodeQuery, connectionString)) //Получение данных о покупателе
                    {
                        DataTable table = new DataTable();
                        dataAdapter.Fill(table);
                        if (table.Rows.Count > 0)
                        {
                            customerCode = int.Parse(table.Rows[0]["customerCode"].ToString());
                            deliveryCode = int.Parse(table.Rows[0]["deliveryCode"].ToString());
                            paymentCode = int.Parse(table.Rows[0]["paymentCode"].ToString());

                            pickupCode = int.Parse(table.Rows[0]["pickupCode"].ToString());
                            for (int i = 0; i < ProductsInfoGrid.Items.Count; i++)
                            {
                                DataRowView productInfo = (DataRowView)ProductsInfoGrid.Items[i];
                                string selectProductCodeQuery = "SELECT productCode FROM Product WHERE productArticle = '" + productInfo["productArticle"].ToString() + "'";
                                using (SqlDataAdapter dataAdapter1 = new SqlDataAdapter(selectProductCodeQuery, connectionString))
                                {
                                    DataTable table1 = new DataTable();
                                    dataAdapter1.Fill(table1);
                                    if (table1.Rows.Count > 0) productCode = int.Parse(table1.Rows[0]["productCode"].ToString());
                                }
                                string selectBasketCodeQuery = "SELECT basketCode FROM Basket WHERE productCode = " + productCode + " AND basketNumber = '" + basketNumber + "'";
                                using (SqlDataAdapter dataAdapter2 = new SqlDataAdapter(selectBasketCodeQuery, connectionString))
                                {
                                    DataTable table2 = new DataTable();
                                    dataAdapter2.Fill(table2);
                                    if (table2.Rows.Count > 0) basketCode = int.Parse(table2.Rows[0]["basketCode"].ToString());
                                }
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "INSERT CustomerOrder (orderNumber, issueDate, deliveryCost, deliveryCode, basketCode, pickupCode, customerCode, employeeCode, paymentCode, generalSum) " +
                                                  "VALUES (@number, @date, @devCost, @devCode, @bsktCode, @pckpCode, @customerCode, @empCode, @paymentCode, @sum)";
                                cmd.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                                cmd.Parameters.Add("@date", SqlDbType.Date).Value = DateTime.Now.ToShortDateString();
                                cmd.Parameters.Add("@devCost", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(deliveryCost.Text), 2);
                                cmd.Parameters.Add("@devCode", SqlDbType.Int).Value = deliveryCode;
                                cmd.Parameters.Add("@bsktCode", SqlDbType.Int).Value = basketCode;
                                cmd.Parameters.Add("@pckpCode", SqlDbType.Int).Value = pickupCode;
                                cmd.Parameters.Add("@customerCode", SqlDbType.Int).Value = customerCode;
                                cmd.Parameters.Add("@empCode", SqlDbType.Int).Value = employeeCode;
                                cmd.Parameters.Add("@paymentCode", SqlDbType.Int).Value = paymentCode;
                                cmd.Parameters.Add("@sum", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(generalSum.Text), 2);
                                cmd.Connection = connectionString;
                                connectionString.Open();
                                cmd.ExecuteNonQuery(); //Запись информации о заказе в БД
                                connectionString.Close();

                                SqlCommand cmd1 = new SqlCommand();
                                cmd1.CommandType = CommandType.Text;
                                cmd1.CommandText = "UPDATE Basket SET paymentStatus = @payment WHERE basketCode = @code AND basketNumber = @number";
                                cmd1.Parameters.Add("@code", SqlDbType.Int).Value = basketCode;
                                cmd1.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                                cmd1.Parameters.Add("@payment", SqlDbType.Bit).Value = true;
                                cmd1.Connection = connectionString;
                                connectionString.Open();
                                cmd1.ExecuteNonQuery();
                                connectionString.Close();
                            }
                        }
                    }
                }
                else
                {
                    string selectCustomerCodeQuery = "SELECT customerCode, deliveryCode, paymentCode FROM Customer, Delivery, Payment WHERE phone = '" + customerPhone.Text + "' AND orderNumber = '" + basketNumber + "' " +
                                                     "AND deliveryName = '" + delivery.SelectedItem.ToString() + "' AND paymentType = '" + payment.SelectedItem.ToString() + "'";
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectCustomerCodeQuery, connectionString)) //Получение данных о покупателе
                    {
                        DataTable table = new DataTable();
                        dataAdapter.Fill(table);
                        if (table.Rows.Count > 0)
                        {
                            customerCode = int.Parse(table.Rows[0]["customerCode"].ToString());
                            deliveryCode = int.Parse(table.Rows[0]["deliveryCode"].ToString());
                            paymentCode = int.Parse(table.Rows[0]["paymentCode"].ToString());

                            for (int i = 0; i < ProductsInfoGrid.Items.Count; i++)
                            {
                                DataRowView productInfo = (DataRowView)ProductsInfoGrid.Items[i];
                                string selectProductCodeQuery = "SELECT productCode FROM Product WHERE productArticle = '" + productInfo["productArticle"].ToString() + "'";
                                using (SqlDataAdapter dataAdapter1 = new SqlDataAdapter(selectProductCodeQuery, connectionString))
                                {
                                    DataTable table1 = new DataTable();
                                    dataAdapter1.Fill(table1);
                                    if (table1.Rows.Count > 0) productCode = int.Parse(table1.Rows[0]["productCode"].ToString());
                                }
                                string selectBasketCodeQuery = "SELECT basketCode FROM Basket WHERE productCode = " + productCode + " AND basketNumber = '" + basketNumber + "'";
                                using (SqlDataAdapter dataAdapter2 = new SqlDataAdapter(selectBasketCodeQuery, connectionString))
                                {
                                    DataTable table2 = new DataTable();
                                    dataAdapter2.Fill(table2);
                                    if (table2.Rows.Count > 0) basketCode = int.Parse(table2.Rows[0]["basketCode"].ToString());
                                }
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "INSERT CustomerOrder (orderNumber, issueDate, deliveryCost, deliveryCode, basketCode, pickupCode, customerCode, employeeCode, paymentCode, generalSum) " +
                                                  "VALUES (@number, @date, @devCost, @devCode, @bsktCode, @pckpCode, @customerCode, @empCode, @paymentCode, @sum)";
                                cmd.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                                cmd.Parameters.Add("@date", SqlDbType.Date).Value = DateTime.Now.ToShortDateString();
                                cmd.Parameters.Add("@devCost", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(deliveryCost.Text), 2);
                                cmd.Parameters.Add("@devCode", SqlDbType.Int).Value = deliveryCode;
                                cmd.Parameters.Add("@bsktCode", SqlDbType.Int).Value = basketCode;
                                cmd.Parameters.Add("@pckpCode", SqlDbType.Int).Value = DBNull.Value;
                                cmd.Parameters.Add("@customerCode", SqlDbType.Int).Value = customerCode;
                                cmd.Parameters.Add("@empCode", SqlDbType.Int).Value = employeeCode;
                                cmd.Parameters.Add("@paymentCode", SqlDbType.Int).Value = paymentCode;
                                cmd.Parameters.Add("@sum", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(generalSum.Text), 2);
                                cmd.Connection = connectionString;
                                connectionString.Open();
                                cmd.ExecuteNonQuery(); //Запись информации о заказе в БД
                                connectionString.Close();

                                SqlCommand cmd1 = new SqlCommand();
                                cmd1.CommandType = CommandType.Text;
                                cmd1.CommandText = "UPDATE Basket SET paymentStatus = @payment WHERE basketCode = @code AND basketNumber = @number";
                                cmd1.Parameters.Add("@code", SqlDbType.Int).Value = basketCode;
                                cmd1.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                                cmd1.Parameters.Add("@payment", SqlDbType.Bit).Value = true;
                                cmd1.Connection = connectionString;
                                connectionString.Open();
                                cmd1.ExecuteNonQuery();
                                connectionString.Close();
                            }
                        }
                    }
                }
                MessageBox.Show("Заказ успешно оформлен.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                int q = 16;
                try
                {
                    excelappworkbooks = excelapp.Workbooks;
                    excelappworkbook = excelapp.Workbooks.Open(Path.GetFullPath(@"SalesReceiptExample.xls")); //Открытие шаблона
                    excelsheets = excelappworkbook.Worksheets;
                    excelworksheet = (Microsoft.Office.Interop.Excel.Worksheet)excelsheets.get_Item(1);
                    excelcells = excelworksheet.get_Range("D5");
                    excelcells.Value = basketNumber;
                    excelcells = excelworksheet.get_Range("F5");
                    excelcells.Value = DateTime.Now.ToShortDateString();
                    excelcells = excelworksheet.get_Range("C13");
                    excelcells.Value = "ОАО \"Керамин\"";
                    string infoQuery = "SELECT customerName, customerSurname, customerPatronymic, phone, mail, productName, productCostCount, productCostArea, productsCount, Basket.generalSum, CustomerOrder.generalSum, employeeSurname, employeeName, employeePatronymic FROM CustomerOrder " +
                                       "JOIN Basket ON CustomerOrder.basketCode = Basket.basketCode " +
                                       "JOIN Product ON Basket.productCode = Product.productCode " +
                                       "JOIN Customer ON CustomerOrder.customerCode = Customer.customerCode " +
                                       "JOIN Employee ON CustomerOrder.employeeCode = Employee.employeeCode " +
                                       "WHERE CustomerOrder.orderNumber = " + basketNumber + "";
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(infoQuery, connectionString)) //Заполнения документа данными
                    {
                        int position = 1;
                        int k = 5;
                        string ed = "шт.";
                        DataTable table = new DataTable();
                        dataAdapter.Fill(table);
                        if (table.Rows.Count > 0)
                        {
                            for (int rows = 0; rows < table.Rows.Count; ++rows, ++position)
                            {
                                for (int j = 2; j < 9; j++)
                                {
                                    excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, j];
                                    excelcells.Borders.ColorIndex = 0;
                                    excelcells.Font.Size = 9;
                                    excelcells.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                    if (j == 2) excelcells.Value2 = position.ToString();
                                    else if (j == 3)
                                    {
                                        excelcells.WrapText = true;
                                        excelcells.EntireRow.AutoFit();
                                        excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlLeft;
                                        excelcells.Value = table.Rows[rows][k].ToString();
                                        ++k;
                                    }
                                    else if (j == 4) excelcells.Value2 = ed.ToString();
                                    else
                                    {
                                        excelcells.Value = table.Rows[rows][k].ToString();
                                        ++k;
                                    }
                                }
                                ++q;
                                k = 5;
                            }
                            excelcells = excelworksheet.get_Range("C7");
                            excelcells.Value = table.Rows[0]["customerSurname"].ToString() + " " + table.Rows[0]["customerName"].ToString() + " " + table.Rows[0]["customerPatronymic"].ToString();

                            excelcells = excelworksheet.get_Range("C9");
                            excelcells.Value = table.Rows[0]["phone"].ToString();

                            excelcells = excelworksheet.get_Range("C11");
                            excelcells.Value = table.Rows[0]["mail"].ToString();

                            for (int j = 2; j < 9; j++)
                            {
                                excelcells.Font.Size = 9;
                                excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, j];
                                excelcells.Borders.ColorIndex = 0;
                                excelcells.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                if (j == 2)
                                {
                                    excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlCenter;
                                    excelcells.Value2 = "ИТОГО";
                                }
                                else if (j == 7) { }
                                else if (j == 8)
                                {
                                    excelcells.Font.Size = 9;
                                    excelcells.Value2 = table.Rows[0][10].ToString();
                                }
                                else excelcells.Value2 = "х";
                            }

                            q = q + 2;
                            excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 1];
                            excelcells = excelworksheet.Range[excelworksheet.Cells[q, 1], excelworksheet.Cells[q, 2]];
                            excelcells.Merge(true);
                            excelcells.Font.Size = 9;
                            excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlRight;
                            excelcells.Value2 = "ФИО сотрудника:";
                            excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 6];
                            excelcells.Font.Size = 9;
                            excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlRight;
                            excelcells.Value2 = "Подпись:";
                            excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 7];
                            excelcells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                            excelcells.Value2 = " ";
                            excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 3];
                            string employeeInfo = table.Rows[0]["employeeSurname"].ToString() + " " + table.Rows[0]["employeeName"].ToString() + " " + table.Rows[0]["employeePatronymic"].ToString();
                            excelcells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                            excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlLeft;
                            excelcells.Font.Size = 9;
                            excelcells.Value2 = "    " + employeeInfo;
                        }
                    }
                    path += @"\Чек №" + basketNumber.ToString() + ".xls";
                    excelappworkbooks = excelapp.Workbooks;
                    excelappworkbook = excelappworkbooks[1];
                    excelappworkbook.SaveAs(path);
                    MessageBox.Show("Чек был успешно создан.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception m) { MessageBox.Show(m.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
                finally { Process.GetProcessById((int)processId).Kill(); } //Закрытие процесса Excel
            }
            else if (legalCustomer.IsChecked == true)
            {
                if (delivery.SelectedItem.ToString() == "Самовывоз")
                {
                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT Customer (orderNumber, customerName, customerSurname, customerPatronymic, legalName, UTN, phone, mail, building, floor_, apartment, townCode, streetCode) " +
                                      "VALUES (@number, @cusName, @cusSurname, @cusPatronymic, @legalName, @utn, @phone, @mail, @building, @floor, @apartment, @townCode, @streetCode)";
                    cmd.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                    cmd.Parameters.Add("@cusName", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@cusSurname", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@cusPatronymic", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@legalName", SqlDbType.VarChar).Value = legalCustomerName.Text;
                    cmd.Parameters.Add("@utn", SqlDbType.Int).Value = int.Parse(legalCustomerUTN.Text);
                    cmd.Parameters.Add("@phone", SqlDbType.VarChar).Value = legalCustomerPhone.Text;
                    cmd.Parameters.Add("@mail", SqlDbType.VarChar).Value = legalCustomerMail.Text;
                    cmd.Parameters.Add("@building", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@floor", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@apartment", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@townCode", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@streetCode", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Connection = connectionString;
                    connectionString.Open();
                    cmd.ExecuteNonQuery(); //Запись данных о покупателе в таблицу
                    connectionString.Close();
                }
                else
                {
                    string selectAdressCodesQuery = "SELECT townCode, streetCode FROM Town, Street WHERE townName = '" + deliveryTown.Text + "' AND streetName = '" + deliveryStreet.Text + "'";
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectAdressCodesQuery, connectionString))
                    {
                        DataTable table = new DataTable();
                        dataAdapter.Fill(table);
                        if (table.Rows.Count > 0)
                        {
                            townCode = int.Parse(table.Rows[0]["townCode"].ToString());
                            streetCode = int.Parse(table.Rows[0]["streetCode"].ToString());
                        }
                    }

                    SqlCommand cmd = new SqlCommand();
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT Customer (orderNumber, customerName, customerSurname, customerPatronymic, legalName, UTN, phone, mail, building, floor_, apartment, townCode, streetCode) " +
                                      "VALUES (@number, @cusName, @cusSurname, @cusPatronymic, @legalName, @utn, @phone, @mail, @building, @floor, @apartment, @townCode, @streetCode)";
                    cmd.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                    cmd.Parameters.Add("@cusName", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@cusSurname", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@cusPatronymic", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@legalName", SqlDbType.VarChar).Value = legalCustomerName.Text;
                    cmd.Parameters.Add("@utn", SqlDbType.Int).Value = int.Parse(legalCustomerUTN.Text);
                    cmd.Parameters.Add("@phone", SqlDbType.VarChar).Value = legalCustomerPhone.Text;
                    cmd.Parameters.Add("@mail", SqlDbType.VarChar).Value = legalCustomerMail.Text;
                    cmd.Parameters.Add("@building", SqlDbType.VarChar).Value = deliveryBuilding.Text;
                    cmd.Parameters.Add("@floor", SqlDbType.Int).Value = int.Parse(deliveryFloor.Text);
                    cmd.Parameters.Add("@apartment", SqlDbType.Int).Value = int.Parse(deliveryApartment.Text);
                    cmd.Parameters.Add("@townCode", SqlDbType.Int).Value = townCode;
                    cmd.Parameters.Add("@streetCode", SqlDbType.Int).Value = streetCode;
                    cmd.Connection = connectionString;
                    connectionString.Open();
                    cmd.ExecuteNonQuery(); //Запись данных о покупателе в таблицу
                    connectionString.Close();
                }

                if (delivery.SelectedItem.ToString() == "Самовывоз")
                {
                    string adress = selfDeliveryAdress.Text.ToString().Replace("ул. ", "");
                    adress = adress.Replace(", ", ",");
                    string[] adressArray = adress.Split(',');

                    string selectCustomerCodeQuery = "SELECT customerCode, deliveryCode, paymentCode, pickupCode FROM Customer, Delivery, Payment, Pickup " +
                                                     "JOIN PickupTown ON Pickup.pickupTownCode = PickupTown.pickupTownCode " +
                                                     "WHERE phone = '" + legalCustomerPhone.Text + "' AND orderNumber = '" + basketNumber + "' AND pickupTownName = '" + selfDeliveryTown.Text + "' AND Pickup.streetName = '" + adressArray[0] + "' " +
                                                     "AND Pickup.building = '" + adressArray[1] + "' AND deliveryName = '" + delivery.SelectedItem.ToString() + "' AND paymentType = '" + payment.SelectedItem.ToString() + "'";
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectCustomerCodeQuery, connectionString)) //Получение данных о покупателе
                    {
                        DataTable table = new DataTable();
                        dataAdapter.Fill(table);
                        if (table.Rows.Count > 0)
                        {
                            customerCode = int.Parse(table.Rows[0]["customerCode"].ToString());
                            deliveryCode = int.Parse(table.Rows[0]["deliveryCode"].ToString());
                            paymentCode = int.Parse(table.Rows[0]["paymentCode"].ToString());

                            pickupCode = int.Parse(table.Rows[0]["pickupCode"].ToString());
                            for (int i = 0; i < ProductsInfoGrid.Items.Count; i++)
                            {
                                DataRowView productInfo = (DataRowView)ProductsInfoGrid.Items[i];
                                string selectProductCodeQuery = "SELECT productCode FROM Product WHERE productArticle = '" + productInfo["productArticle"].ToString() + "'";
                                using (SqlDataAdapter dataAdapter1 = new SqlDataAdapter(selectProductCodeQuery, connectionString))
                                {
                                    DataTable table1 = new DataTable();
                                    dataAdapter1.Fill(table1);
                                    if (table1.Rows.Count > 0) productCode = int.Parse(table1.Rows[0]["productCode"].ToString());
                                }
                                string selectBasketCodeQuery = "SELECT basketCode FROM Basket WHERE productCode = " + productCode + " AND basketNumber = '" + basketNumber + "'";
                                using (SqlDataAdapter dataAdapter2 = new SqlDataAdapter(selectBasketCodeQuery, connectionString))
                                {
                                    DataTable table2 = new DataTable();
                                    dataAdapter2.Fill(table2);
                                    if (table2.Rows.Count > 0) basketCode = int.Parse(table2.Rows[0]["basketCode"].ToString());
                                }
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "INSERT CustomerOrder (orderNumber, issueDate, deliveryCost, deliveryCode, basketCode, pickupCode, customerCode, employeeCode, paymentCode, generalSum) " +
                                                  "VALUES (@number, @date, @devCost, @devCode, @bsktCode, @pckpCode, @customerCode, @empCode, @paymentCode, @sum)";
                                cmd.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                                cmd.Parameters.Add("@date", SqlDbType.Date).Value = DateTime.Now.ToShortDateString();
                                cmd.Parameters.Add("@devCost", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(deliveryCost.Text), 2);
                                cmd.Parameters.Add("@devCode", SqlDbType.Int).Value = deliveryCode;
                                cmd.Parameters.Add("@bsktCode", SqlDbType.Int).Value = basketCode;
                                cmd.Parameters.Add("@pckpCode", SqlDbType.Int).Value = pickupCode;
                                cmd.Parameters.Add("@customerCode", SqlDbType.Int).Value = customerCode;
                                cmd.Parameters.Add("@empCode", SqlDbType.Int).Value = employeeCode;
                                cmd.Parameters.Add("@paymentCode", SqlDbType.Int).Value = paymentCode;
                                cmd.Parameters.Add("@sum", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(generalSum.Text), 2);
                                cmd.Connection = connectionString;
                                connectionString.Open();
                                cmd.ExecuteNonQuery(); ////Запись информации о заказе в БД
                                connectionString.Close();

                                SqlCommand cmd1 = new SqlCommand();
                                cmd1.CommandType = CommandType.Text;
                                cmd1.CommandText = "UPDATE Basket SET paymentStatus = @payment WHERE basketCode = @code AND basketNumber = @number";
                                cmd1.Parameters.Add("@code", SqlDbType.Int).Value = basketCode;
                                cmd1.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                                cmd1.Parameters.Add("@payment", SqlDbType.Bit).Value = true;
                                cmd1.Connection = connectionString;
                                connectionString.Open();
                                cmd1.ExecuteNonQuery();
                                connectionString.Close();
                            }
                        }
                    }
                }
                else
                {
                    string selectCustomerCodeQuery = "SELECT customerCode, deliveryCode, paymentCode FROM Customer, Delivery, Payment WHERE phone = '" + legalCustomerPhone.Text + "' AND orderNumber = '" + basketNumber + "' " +
                                                     "AND deliveryName = '" + delivery.SelectedItem.ToString() + "' AND paymentType = '" + payment.SelectedItem.ToString() + "'";
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectCustomerCodeQuery, connectionString))  //Получение данных о покупателе
                    {
                        DataTable table = new DataTable();
                        dataAdapter.Fill(table);
                        if (table.Rows.Count > 0)
                        {
                            customerCode = int.Parse(table.Rows[0]["customerCode"].ToString());
                            deliveryCode = int.Parse(table.Rows[0]["deliveryCode"].ToString());
                            paymentCode = int.Parse(table.Rows[0]["paymentCode"].ToString());

                            for (int i = 0; i < ProductsInfoGrid.Items.Count; i++)
                            {
                                DataRowView productInfo = (DataRowView)ProductsInfoGrid.Items[i];
                                string selectProductCodeQuery = "SELECT productCode FROM Product WHERE productArticle = '" + productInfo["productArticle"].ToString() + "'";
                                using (SqlDataAdapter dataAdapter1 = new SqlDataAdapter(selectProductCodeQuery, connectionString))
                                {
                                    DataTable table1 = new DataTable();
                                    dataAdapter1.Fill(table1);
                                    if (table1.Rows.Count > 0) productCode = int.Parse(table1.Rows[0]["productCode"].ToString());
                                }
                                string selectBasketCodeQuery = "SELECT basketCode FROM Basket WHERE productCode = " + productCode + " AND basketNumber = '" + basketNumber + "'";
                                using (SqlDataAdapter dataAdapter2 = new SqlDataAdapter(selectBasketCodeQuery, connectionString))
                                {
                                    DataTable table2 = new DataTable();
                                    dataAdapter2.Fill(table2);
                                    if (table2.Rows.Count > 0) basketCode = int.Parse(table2.Rows[0]["basketCode"].ToString());
                                }
                                SqlCommand cmd = new SqlCommand();
                                cmd.CommandText = "INSERT CustomerOrder (orderNumber, issueDate, deliveryCost, deliveryCode, basketCode, pickupCode, customerCode, employeeCode, paymentCode, generalSum) " +
                                                  "VALUES (@number, @date, @devCost, @devCode, @bsktCode, @pckpCode, @customerCode, @empCode, @paymentCode, @sum)";
                                cmd.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                                cmd.Parameters.Add("@date", SqlDbType.Date).Value = DateTime.Now.ToShortDateString();
                                cmd.Parameters.Add("@devCost", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(deliveryCost.Text), 2);
                                cmd.Parameters.Add("@devCode", SqlDbType.Int).Value = deliveryCode;
                                cmd.Parameters.Add("@bsktCode", SqlDbType.Int).Value = basketCode;
                                cmd.Parameters.Add("@pckpCode", SqlDbType.Int).Value = DBNull.Value;
                                cmd.Parameters.Add("@customerCode", SqlDbType.Int).Value = customerCode;
                                cmd.Parameters.Add("@empCode", SqlDbType.Int).Value = employeeCode;
                                cmd.Parameters.Add("@paymentCode", SqlDbType.Int).Value = paymentCode;
                                cmd.Parameters.Add("@sum", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(generalSum.Text), 2);
                                cmd.Connection = connectionString;
                                connectionString.Open();
                                cmd.ExecuteNonQuery(); //Запись информации о заказе в БД
                                connectionString.Close();

                                SqlCommand cmd1 = new SqlCommand();
                                cmd1.CommandType = CommandType.Text;
                                cmd1.CommandText = "UPDATE Basket SET paymentStatus = @payment WHERE basketCode = @code AND basketNumber = @number";
                                cmd1.Parameters.Add("@code", SqlDbType.Int).Value = basketCode;
                                cmd1.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                                cmd1.Parameters.Add("@payment", SqlDbType.Bit).Value = true;
                                cmd1.Connection = connectionString;
                                connectionString.Open();
                                cmd1.ExecuteNonQuery();
                                connectionString.Close();
                            }
                        }
                    }
                }
                MessageBox.Show("Заказ успешно оформлен.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                int q = 18;
                try
                {
                    excelappworkbooks = excelapp.Workbooks;
                    excelappworkbook = excelapp.Workbooks.Open(Path.GetFullPath(@"LegalSalesReceiptExample.xls")); //Открытие шаблона
                    excelsheets = excelappworkbook.Worksheets;
                    excelworksheet = (Microsoft.Office.Interop.Excel.Worksheet)excelsheets.get_Item(1);
                    excelcells = excelworksheet.get_Range("D5");
                    excelcells.Value = basketNumber;
                    excelcells = excelworksheet.get_Range("F5");
                    excelcells.Value = DateTime.Now.ToShortDateString();
                    excelcells = excelworksheet.get_Range("C15");
                    excelcells.Value = "ОАО \"Керамин\"";
                    string infoQuery = "SELECT legalName, UTN, phone, mail, productName, productCostCount, productCostArea, productsCount, Basket.generalSum, CustomerOrder.generalSum, employeeSurname, employeeName, employeePatronymic FROM CustomerOrder " +
                                       "JOIN Basket ON CustomerOrder.basketCode = Basket.basketCode " +
                                       "JOIN Product ON Basket.productCode = Product.productCode " +
                                       "JOIN Customer ON CustomerOrder.customerCode = Customer.customerCode " +
                                       "JOIN Employee ON CustomerOrder.employeeCode = Employee.employeeCode " +
                                       "WHERE CustomerOrder.orderNumber = " + basketNumber + "";
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(infoQuery, connectionString)) //Заполнения документа данными
                    {
                        int position = 1;
                        int k = 4;
                        string ed = "шт.";
                        DataTable table = new DataTable();
                        dataAdapter.Fill(table);
                        if (table.Rows.Count > 0)
                        {
                            for (int rows = 0; rows < table.Rows.Count; ++rows, ++position)
                            {
                                for (int j = 2; j < 9; j++)
                                {
                                    excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, j];
                                    excelcells.Borders.ColorIndex = 0;
                                    excelcells.Font.Size = 9;
                                    excelcells.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                    if (j == 2) excelcells.Value2 = position.ToString();
                                    else if (j == 3)
                                    {
                                        excelcells.WrapText = true;
                                        excelcells.EntireRow.AutoFit();
                                        excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlLeft;
                                        excelcells.Value = table.Rows[rows][k].ToString();
                                        ++k;
                                    }
                                    else if (j == 4) excelcells.Value2 = ed.ToString();
                                    else
                                    {
                                        excelcells.Value = table.Rows[rows][k].ToString();
                                        ++k;
                                    }
                                }
                                ++q;
                                k = 4;
                            }
                            excelcells = excelworksheet.get_Range("C7");
                            excelcells.Value = table.Rows[0]["legalName"].ToString();

                            excelcells = excelworksheet.get_Range("C9");
                            excelcells.Value = table.Rows[0]["UTN"].ToString();

                            excelcells = excelworksheet.get_Range("C11");
                            excelcells.Value = table.Rows[0]["phone"].ToString();

                            excelcells = excelworksheet.get_Range("C13");
                            excelcells.Value = table.Rows[0]["mail"].ToString();

                            for (int j = 2; j < 9; j++)
                            {
                                excelcells.Font.Size = 9;
                                excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, j];
                                excelcells.Borders.ColorIndex = 0;
                                excelcells.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                if (j == 2)
                                {
                                    excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlCenter;
                                    excelcells.Value2 = "ИТОГО";
                                }
                                else if (j == 7) { }
                                else if (j == 8)
                                {
                                    excelcells.Font.Size = 9;
                                    excelcells.Value2 = table.Rows[0][9].ToString();
                                }
                                else excelcells.Value2 = "х";
                            }

                            q = q + 2;
                            excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 1];
                            excelcells = excelworksheet.Range[excelworksheet.Cells[q, 1], excelworksheet.Cells[q, 2]];
                            excelcells.Merge(true);
                            excelcells.Font.Size = 9;
                            excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlRight;
                            excelcells.Value2 = "ФИО сотрудника:";
                            excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 6];
                            excelcells.Font.Size = 9;
                            excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlRight;
                            excelcells.Value2 = "Подпись:";
                            excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 7];
                            excelcells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                            excelcells.Value2 = " ";
                            excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 3];
                            string employeeInfo = table.Rows[0]["employeeSurname"].ToString() + " " + table.Rows[0]["employeeName"].ToString() + " " + table.Rows[0]["employeePatronymic"].ToString();
                            excelcells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                            excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlLeft;
                            excelcells.Font.Size = 9;
                            excelcells.Value2 = "    " + employeeInfo;
                        }
                    }
                    path += @"\Чек №" + basketNumber.ToString() + ".xls";
                    excelappworkbooks = excelapp.Workbooks;
                    excelappworkbook = excelappworkbooks[1];
                    excelappworkbook.SaveAs(path);
                    MessageBox.Show("Чек был успешно создан.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception m) { MessageBox.Show(m.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
                finally { Process.GetProcessById((int)processId).Kill(); }  //Закрытие процесса Excel
            }
            File.WriteAllText(@"BasketNumber.txt", string.Empty);
            ProductsInfoGrid.ItemsSource = null;
            ProductsInfoGrid.Items.Refresh();
            windowName.Content = "Корзина";
            windowDescription.Content = "Здесь вы можете оформить заказ";
            Height = 530;
        }

        private void deliveryCost_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (deliveryCost.Text == string.Empty) deliveryCost.Text = "0";
            else if (deliveryCost.Text != string.Empty)
            {
                if (deliveryCost.Text != Product.CheckCostForSearch(deliveryCost.Text, "Стоимость доставки не может быть отрицательной.", "Вы указали недопустимые символы в стоимости доставки."))
                {
                    MessageBox.Show(Product.CheckCostForSearch(deliveryCost.Text, "Стоимость доставки не может быть отрицательной.", "Вы указали недопустимые символы в стоимости доставки."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    deliveryCost.Text = "15";
                    return;
                }
            }
            generalSum.Text = (double.Parse(deliveryCost.Text) + double.Parse(sum.Text)).ToString();
        }
    }
}