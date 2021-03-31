using KeraminStore.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace KeraminStore.UI.Windows
{
    public partial class CreateOrderWindow : Window
    {
        readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");

        public CreateOrderWindow()
        {
            InitializeComponent();
            File.WriteAllText(@"BasketNumber.txt", string.Empty);
            OrderNumberWindow ordr = new OrderNumberWindow();
            ordr.ShowDialog();
            GetDeliveryInfo();
            GetTownInfo();
            GetStreetInfo();
            GetPaymentInfo();
            connectionString.Open();
            FillDataGrid();
            connectionString.Close();
        }

        private void GetTownInfo()
        {
            List<string> townNames = new List<string>();
            connectionString.Open();
            string query = @"SELECT townName FROM Town";
            SqlCommand sqlCommand = new SqlCommand(query, connectionString);
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    townNames.Add(dataReader["townName"].ToString());
                    var newList = from i in townNames orderby i select i;
                    selfDeliveryTown.ItemsSource = newList;
                    deliveryTown.ItemsSource = newList;
                }
            }
            dataReader.Close();
            connectionString.Close();
        }

        private void GetStreetInfo()
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
                    selfDeliveryStreet.ItemsSource = newList;
                    deliveryStreet.ItemsSource = newList;
                }
            }
            dataReader.Close();
            connectionString.Close();
        }

        private void GetPaymentInfo()
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

        private void GetDeliveryInfo()
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

        private void FillDataGrid()
        {
            StreamReader streamReader = new StreamReader("BasketNumber.txt");
            string basketNumber = streamReader.ReadLine();
            streamReader.Close();
            string productsInfoQuery = "SELECT productName, productImage, productArticle, productCostCount, productCostArea, boxesCount, CONCAT(boxesCount, ' | ', productCostCount, ' | ', productCostArea) as 'count', productsCount, productsArea, productsWeight, generalSum " +
                                       "FROM Basket " +
                                       "JOIN Product ON Basket.productCode = Product.productCode WHERE basketNumber = '" + basketNumber + "'";
            using (SqlDataAdapter adapter = new SqlDataAdapter(productsInfoQuery, connectionString))
            {
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                if (dataTable.Rows.Count >= 0)
                {
                    DataTable datatable = new DataTable();
                    using (SqlCommand cmd = new SqlCommand(productsInfoQuery, connectionString))
                    {
                        using (IDataReader rdr = cmd.ExecuteReader())
                        {
                            datatable.Load(rdr);
                        }
                    }
                    ProductsInfoGrid.ItemsSource = datatable.DefaultView;
                }
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsInfoGrid.Items.Count == 0)
            {
                MessageBox.Show("Вы не добавили ни одного изделия для покупки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                double calculateSum = 0;
                if (sum.Text == string.Empty) sum.Text = "0";
                for (int i = 0; i < ProductsInfoGrid.Items.Count; i++)
                {
                    DataRowView productInfo = (DataRowView)ProductsInfoGrid.Items[i];
                    calculateSum = double.Parse(sum.Text) + double.Parse(productInfo["generalSum"].ToString());
                    sum.Text = calculateSum.ToString();
                }
                Height = 1000;
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            StreamReader streamReader = new StreamReader("BasketNumber.txt");
            string basketNumber = streamReader.ReadLine();
            streamReader.Close();
            if (ProductsInfoGrid.SelectedItem == null)
            {
                MessageBox.Show("Невозможно удалить изделие из корзины, так как оно не было выбрано.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                DataRowView productInfo = (DataRowView)ProductsInfoGrid.SelectedItems[0];
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "DELETE Basket FROM Basket INNER JOIN Product ON Basket.productCode = Product.productCode WHERE productArticle = @article AND basketNumber = @number";
                cmd.Parameters.Add("@article", SqlDbType.VarChar).Value = productInfo["productArticle"].ToString();
                cmd.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                cmd.Connection = connectionString;
                connectionString.Open();
                cmd.ExecuteNonQuery();
                FillDataGrid();
                connectionString.Close();
                MessageBox.Show("Удаление успешно выполнено.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(@"BasketNumber.txt", string.Empty);
            this.Close();
        }

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

        private void delivery_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (delivery.SelectedItem.ToString() == "Самовывоз")
            {
                deliveryCost.Text = "0";
                selfDeliveryInfo.Visibility = Visibility.Visible;
                deliveryInfo.Visibility = Visibility.Hidden;
            }
            else
            {
                deliveryCost.Text = "15";
                selfDeliveryInfo.Visibility = Visibility.Hidden;
                deliveryInfo.Visibility = Visibility.Visible;
            }
        }

        private void createOrderbutton_Click(object sender, RoutedEventArgs e)
        {
            if (legalCustomer.IsChecked == false && usualCustomer.IsChecked == false)
            {
                MessageBox.Show("Вы не указали, кем является покупатель.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (legalCustomer.IsChecked == true)
            {
                if (legalCustomerName.Text == string.Empty)
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

                if (legalCustomerUTN.Text == string.Empty)
                {
                    MessageBox.Show("Вы не указали УНП.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                                MessageBox.Show("Вы указали неверные символы в УНП.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                        }
                    }
                    else
                    {
                        MessageBox.Show("Длина УНП может составлять 9 символов.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (legalCustomerPhone.Text == string.Empty)
                {
                    MessageBox.Show("Вы не указали контактный телефон.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    Regex regex = new Regex(@"^(\+375|80)\(\s?(44|25|33|17|29)\s?\)\d{3}-?\s?\d{2}-?\s?\d{2}$");
                    if (!regex.IsMatch(legalCustomerPhone.Text))
                    {
                        MessageBox.Show("Телефон(должен быть записан в формате [код оператора] [2 цифры индентификатора] XXX-XX-XX.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (legalCustomerMail.Text == string.Empty)
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
            else if (usualCustomer.IsChecked == true)
            {
                if (customerName.Text != Employee.CheckEmployeeFullName(customerName.Text, "Имя не может быть пустым.", "Имя содержит недопустимые символы.", "Длина имени может составлять 2-50 символов."))
                {
                    MessageBox.Show(Employee.CheckEmployeeFullName(customerName.Text, "Имя не может быть пустым.", "Имя содержит недопустимые символы.", "Длина имени может составлять 2-50 символов."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (customerSurname.Text != Employee.CheckEmployeeFullName(customerSurname.Text, "Фамилия не может быть пустой.", "Фамилия содержит недопустимые символы.", "Длина фамилии может составлять 2-50 символов."))
                {
                    MessageBox.Show(Employee.CheckEmployeeFullName(customerSurname.Text, "Фамилия не может быть пустой.", "Фамилия содержит недопустимые символы.", "Длина фамилии может составлять 2-50 символов."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (customerPatronymic.Text != Employee.CheckEmployeeFullName(customerPatronymic.Text, "Отчество не может быть пустым.", "Отчество содержит недопустимые символы.", "Длина отчества может составлять 2-50 символов."))
                {
                    MessageBox.Show(Employee.CheckEmployeeFullName(customerPatronymic.Text, "Отчество не может быть пустым.", "Отчество содержит недопустимые символы.", "Длина отчества может составлять 2-50 символов."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (customerPhone.Text == string.Empty)
                {
                    MessageBox.Show("Вы не указали контактный телефон.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    Regex regex = new Regex(@"^(\+375|80)\(\s?(44|25|33|17|29)\s?\)\d{3}-?\s?\d{2}-?\s?\d{2}$");
                    if (!regex.IsMatch(customerPhone.Text))
                    {
                        MessageBox.Show("Телефон(должен быть записан в формате [код оператора] [2 цифры индентификатора] XXX-XX-XX.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                if (customerMail.Text == string.Empty)
                {
                    MessageBox.Show("Вы не указали e-mail.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

            if (delivery.SelectedIndex == -1)
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

                if (selfDeliveryStreet.Text == string.Empty)
                {
                    MessageBox.Show("Необходимо указать улицу для самовывоза.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Regex regex = new Regex(@"^\d{1,3}$");
                if (!regex.IsMatch(selfDeliveryBuilding.Text))
                {
                    MessageBox.Show("Максимальная длина номера дома 3 символа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else
            {
                if (deliveryTown.Text == string.Empty)
                {
                    MessageBox.Show("Необходимо указать город для самовывоза.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (deliveryStreet.Text == string.Empty)
                {
                    MessageBox.Show("Необходимо указать улицу для самовывоза.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Regex regex = new Regex(@"^\d{1,3}$");
                if (!regex.IsMatch(deliveryBuilding.Text))
                {
                    MessageBox.Show("Максимальная длина номера дома 3 символа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Regex regexx = new Regex(@"^\d{1,2}$");
                if (!regexx.IsMatch(deliveryFloor.Text))
                {
                    MessageBox.Show("Максимальная длина этажа 2 символа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!regex.IsMatch(deliveryApartment.Text))
                {
                    MessageBox.Show("Максимальная длина номера квартиры/офиса 3 символа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (payment.SelectedIndex == -1)
            {
                MessageBox.Show("Вы не указали способ оплаты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (deliveryCost.Text == string.Empty)
            {
                MessageBox.Show("Вы должны указать стоимость доставки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (deliveryCost.Text != string.Empty)
            {
                if (deliveryCost.Text != Product.CheckProductCostOrWeight(deliveryCost.Text, "Стоимость доставки не может быть отрицательной.", "Вы указали недопустимые символы в стоимости доставки."))
                {
                    MessageBox.Show(Product.CheckProductCostOrWeight(deliveryCost.Text, "Стоимость доставки не может быть отрицательной.", "Вы указали недопустимые символы в стоимости доставки."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (generalSum.Text == string.Empty)
            {
                MessageBox.Show("Вы не указали итоговую сумму заказа.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            int streetCode = 0;
            int townCode = 0;
            int customerCode = 0;
            int deliveryCode = 0;
            int paymentCode = 0;
            int productCode = 0;
            int basketCode = 0;

            StreamReader streamReader = new StreamReader("BasketNumber.txt");
            string basketNumber = streamReader.ReadLine();
            streamReader.Close();

            StreamReader file = new StreamReader("UserCode.txt");
            int employeeCode = Convert.ToInt32(file.ReadLine());
            file.Close();
            if (usualCustomer.IsChecked == true)
            {
                if (delivery.SelectedItem.ToString() == "Самовывоз")
                {
                    string selectAdressCodesQuery = "SELECT townCode, streetCode FROM Town, Street WHERE townName = '" + selfDeliveryTown.Text + "' AND streetName = '" + selfDeliveryStreet.Text + "'";
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
                    cmd.Parameters.Add("@building", SqlDbType.Int).Value = int.Parse(selfDeliveryBuilding.Text);
                    cmd.Parameters.Add("@floor", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@apartment", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@townCode", SqlDbType.Int).Value = townCode;
                    cmd.Parameters.Add("@streetCode", SqlDbType.Int).Value = streetCode;
                    cmd.Connection = connectionString;
                    connectionString.Open();
                    cmd.ExecuteNonQuery();
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
                    cmd.Parameters.Add("@cusName", SqlDbType.VarChar).Value = customerName.Text;
                    cmd.Parameters.Add("@cusSurname", SqlDbType.VarChar).Value = customerSurname.Text;
                    cmd.Parameters.Add("@cusPatronymic", SqlDbType.VarChar).Value = customerPatronymic.Text;
                    cmd.Parameters.Add("@legalName", SqlDbType.VarChar).Value = DBNull.Value;
                    cmd.Parameters.Add("@utn", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@phone", SqlDbType.VarChar).Value = customerPhone.Text;
                    cmd.Parameters.Add("@mail", SqlDbType.VarChar).Value = customerMail.Text;
                    cmd.Parameters.Add("@building", SqlDbType.Int).Value = int.Parse(deliveryBuilding.Text);
                    cmd.Parameters.Add("@floor", SqlDbType.Int).Value = int.Parse(deliveryFloor.Text);
                    cmd.Parameters.Add("@apartment", SqlDbType.Int).Value = int.Parse(deliveryApartment.Text);
                    cmd.Parameters.Add("@townCode", SqlDbType.Int).Value = townCode;
                    cmd.Parameters.Add("@streetCode", SqlDbType.Int).Value = streetCode;
                    cmd.Connection = connectionString;
                    connectionString.Open();
                    cmd.ExecuteNonQuery();
                    connectionString.Close();
                }

                string selectCustomerCodeQuery = "SELECT customerCode, deliveryCode, paymentCode FROM Customer, Delivery, Payment WHERE phone= '" + customerPhone.Text + "' AND orderNumber = '" + basketNumber + "' " +
                                                 "AND deliveryName = '" + delivery.SelectedItem.ToString() + "' AND paymentType = '" + payment.SelectedItem.ToString() + "'";
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectCustomerCodeQuery, connectionString))
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
                            string selectProductCodeQuery = "SELECT productCode FROM Product WHERE productArticle= '" + productInfo["productArticle"].ToString() + "'";
                            using (SqlDataAdapter dataAdapter1 = new SqlDataAdapter(selectProductCodeQuery, connectionString))
                            {
                                DataTable table1 = new DataTable();
                                dataAdapter1.Fill(table1);
                                if (table1.Rows.Count > 0) productCode = int.Parse(table1.Rows[0]["productCode"].ToString());
                            }
                            string selectBasketCodeQuery = "SELECT basketCode FROM Basket WHERE productCode= " + productCode + " AND basketNumber = '" + basketNumber + "'";
                            using (SqlDataAdapter dataAdapter2 = new SqlDataAdapter(selectBasketCodeQuery, connectionString))
                            {
                                DataTable table2 = new DataTable();
                                dataAdapter2.Fill(table2);
                                if (table2.Rows.Count > 0) basketCode = int.Parse(table2.Rows[0]["basketCode"].ToString());
                            }
                            SqlCommand cmd = new SqlCommand();
                            cmd.CommandText = "INSERT CustomerOrder (orderNumber, issueDate, deliveryCost, deliveryCode, basketCode, customerCode, employeeCode, paymentCode, generalSum) " +
                                              "VALUES (@number, @date, @devCost, @devCode, @bsktCode, @customerCode, @empCode, @paymentCode, @sum)";
                            cmd.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                            cmd.Parameters.Add("@date", SqlDbType.Date).Value = DateTime.Now.ToShortDateString();
                            cmd.Parameters.Add("@devCost", SqlDbType.Float).Value = Convert.ToDouble(deliveryCost.Text);
                            cmd.Parameters.Add("@devCode", SqlDbType.Int).Value = deliveryCode;
                            cmd.Parameters.Add("@bsktCode", SqlDbType.Int).Value = basketCode;
                            cmd.Parameters.Add("@customerCode", SqlDbType.Int).Value = customerCode;
                            cmd.Parameters.Add("@empCode", SqlDbType.Int).Value = employeeCode;
                            cmd.Parameters.Add("@paymentCode", SqlDbType.Int).Value = paymentCode;
                            cmd.Parameters.Add("@sum", SqlDbType.Float).Value = Convert.ToDouble(generalSum.Text);
                            cmd.Connection = connectionString;
                            connectionString.Open();
                            cmd.ExecuteNonQuery();
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
                        MessageBox.Show("Добавление изделия прошло успешно.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            else if (legalCustomer.IsChecked == true)
            {
                if (delivery.SelectedItem.ToString() == "Самовывоз")
                {
                    string selectAdressCodesQuery = "SELECT townCode, streetCode FROM Town, Street WHERE townName = '" + selfDeliveryTown.Text + "' AND streetName = '" + selfDeliveryStreet.Text + "'";
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
                    cmd.Parameters.Add("@building", SqlDbType.Int).Value = int.Parse(selfDeliveryBuilding.Text);
                    cmd.Parameters.Add("@floor", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@apartment", SqlDbType.Int).Value = DBNull.Value;
                    cmd.Parameters.Add("@townCode", SqlDbType.Int).Value = townCode;
                    cmd.Parameters.Add("@streetCode", SqlDbType.Int).Value = streetCode;
                    cmd.Connection = connectionString;
                    connectionString.Open();
                    cmd.ExecuteNonQuery();
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
                    cmd.Parameters.Add("@building", SqlDbType.Int).Value = int.Parse(deliveryBuilding.Text);
                    cmd.Parameters.Add("@floor", SqlDbType.Int).Value = int.Parse(deliveryFloor.Text);
                    cmd.Parameters.Add("@apartment", SqlDbType.Int).Value = int.Parse(deliveryApartment.Text);
                    cmd.Parameters.Add("@townCode", SqlDbType.Int).Value = townCode;
                    cmd.Parameters.Add("@streetCode", SqlDbType.Int).Value = streetCode;
                    cmd.Connection = connectionString;
                    connectionString.Open();
                    cmd.ExecuteNonQuery();
                    connectionString.Close();
                }

                string selectCustomerCodeQuery = "SELECT customerCode, deliveryCode, paymentCode FROM Customer, Delivery, Payment WHERE phone= '" + legalCustomerPhone.Text + "' AND orderNumber = '" + basketNumber + "' " +
                                                 "AND deliveryName = '" + delivery.SelectedItem.ToString() + "' AND paymentType = '" + payment.SelectedItem.ToString() + "'";
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectCustomerCodeQuery, connectionString))
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
                            string selectProductCodeQuery = "SELECT productCode FROM Product WHERE productArticle= '" + productInfo["productArticle"].ToString() + "'";
                            using (SqlDataAdapter dataAdapter1 = new SqlDataAdapter(selectProductCodeQuery, connectionString))
                            {
                                DataTable table1 = new DataTable();
                                dataAdapter1.Fill(table1);
                                if (table1.Rows.Count > 0) productCode = int.Parse(table1.Rows[0]["productCode"].ToString());
                            }
                            string selectBasketCodeQuery = "SELECT basketCode FROM Basket WHERE productCode= " + productCode + " AND basketNumber = '" + basketNumber + "'";
                            using (SqlDataAdapter dataAdapter2 = new SqlDataAdapter(selectBasketCodeQuery, connectionString))
                            {
                                DataTable table2 = new DataTable();
                                dataAdapter2.Fill(table2);
                                if (table2.Rows.Count > 0) basketCode = int.Parse(table2.Rows[0]["basketCode"].ToString());
                            }
                            SqlCommand cmd = new SqlCommand();
                            cmd.CommandText = "INSERT CustomerOrder (orderNumber, issueDate, deliveryCost, deliveryCode, basketCode, customerCode, employeeCode, paymentCode, generalSum) " +
                                              "VALUES (@number, @date, @devCost, @devCode, @bsktCode, @customerCode, @empCode, @paymentCode, @sum)";
                            cmd.Parameters.Add("@number", SqlDbType.Int).Value = basketNumber;
                            cmd.Parameters.Add("@date", SqlDbType.Date).Value = DateTime.Now.ToShortDateString();
                            cmd.Parameters.Add("@devCost", SqlDbType.Float).Value = Convert.ToDouble(deliveryCost.Text);
                            cmd.Parameters.Add("@devCode", SqlDbType.Int).Value = deliveryCode;
                            cmd.Parameters.Add("@bsktCode", SqlDbType.Int).Value = basketCode;
                            cmd.Parameters.Add("@customerCode", SqlDbType.Int).Value = customerCode;
                            cmd.Parameters.Add("@empCode", SqlDbType.Int).Value = employeeCode;
                            cmd.Parameters.Add("@paymentCode", SqlDbType.Int).Value = paymentCode;
                            cmd.Parameters.Add("@sum", SqlDbType.Float).Value = Convert.ToDouble(generalSum.Text);
                            cmd.Connection = connectionString;
                            connectionString.Open();
                            cmd.ExecuteNonQuery();
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
                        MessageBox.Show("Добавление изделия прошло успешно.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            ProductsInfoGrid.ItemsSource = null;
            ProductsInfoGrid.Items.Refresh();
            Height = 530;
        }

        private void deliveryCost_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            generalSum.Text = (double.Parse(deliveryCost.Text) + double.Parse(sum.Text)).ToString();
        }
    }
}