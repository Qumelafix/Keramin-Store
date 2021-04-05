using KeraminStore.Data.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace KeraminStore.UI.Windows
{
    public partial class CatalogWindow : UserControl
    {
        readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");

        public CatalogWindow()
        {
            InitializeComponent();
            connectionString.Open();
            FillDataGrid();
            connectionString.Close();
            GetSurfaceName();
            GetProductType();
            GetCollectionName();
        }

        private void FillDataGrid()
        {
            string productsInfoQuery = "SELECT productName, productImage, productArticle, CONCAT(productLenght, 'x', productWidth) as 'productParametrs', productLenght, productWidth, productBoxWeight, productCountInBox, productCostCount, productCostArea, productDescription, surfaceName, productTypeName, availabilityStatusName, productCollectionName " +
                                       "FROM Product " +
                                       "JOIN ProductCollection ON Product.productCollectionCode = ProductCollection.productCollectionCode " +
                                       "JOIN AvailabilityStatus On Product.availabilityStatusCode = AvailabilityStatus.availabilityStatusCode " +
                                       "JOIN Surface On Product.surfaceCode = Surface.surfaceCode " +
                                       "JOIN ProductType On Product.productTypeCode = ProductType.productTypeCode WHERE availabilityStatusName = '" + "Есть в наличии" + "'";

            DataTable table = new DataTable();
            using (SqlCommand cmd = new SqlCommand(productsInfoQuery, connectionString))
            {
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    table.Load(rdr);
                }
            }
            ProductsInfoGrid.ItemsSource = table.DefaultView;
        }

        private void GetCollectionName()
        {
            List<string> collectionNames = new List<string>();
            connectionString.Open();
            string query = @"SELECT productCollectionName FROM ProductCollection";
            SqlCommand sqlCommand = new SqlCommand(query, connectionString);
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    collectionNames.Add(dataReader["productCollectionName"].ToString());
                    var newList = from i in collectionNames orderby i select i;
                    productCollectionField.ItemsSource = newList;
                }
            }
            dataReader.Close();
            connectionString.Close();
        }

        private void GetProductType()
        {
            List<string> typeNames = new List<string>();
            connectionString.Open();
            string query = @"SELECT productTypeName FROM ProductType";
            SqlCommand sqlCommand = new SqlCommand(query, connectionString);
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    typeNames.Add(dataReader["productTypeName"].ToString());
                    var newList = from i in typeNames orderby i select i;
                    productTypeField.ItemsSource = newList;
                }
            }
            dataReader.Close();
            connectionString.Close();
        }

        private void GetSurfaceName()
        {
            List<string> surfaceNames = new List<string>();
            connectionString.Open();
            string query = @"SELECT surfaceName FROM Surface";
            SqlCommand sqlCommand = new SqlCommand(query, connectionString);
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    surfaceNames.Add(dataReader["surfaceName"].ToString());
                    var newList = from i in surfaceNames orderby i select i;
                    productSurfaceField.ItemsSource = newList;
                }
            }
            dataReader.Close();
            connectionString.Close();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            productTypeField.SelectedIndex = -1;
            productCollectionField.SelectedIndex = -1;
            productSurfaceField.SelectedIndex = -1;
            firstCost.Clear();
            lastCost.Clear();
            connectionString.Open();
            FillDataGrid();
            connectionString.Close();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string productsInfoQuery = "SELECT productImage, productName, productArticle, CONCAT(productLenght, 'x', productWidth) as 'productParametrs', productLenght, productWidth, productBoxWeight, productCountInBox, productCostCount, productCostArea, productDescription, surfaceName, productTypeName, availabilityStatusName, productCollectionName " +
                                       "FROM Product " +
                                       "JOIN ProductCollection ON Product.productCollectionCode = ProductCollection.productCollectionCode " +
                                       "JOIN AvailabilityStatus On Product.availabilityStatusCode = AvailabilityStatus.availabilityStatusCode " +
                                       "JOIN Surface On Product.surfaceCode = Surface.surfaceCode " +
                                       "JOIN ProductType On Product.productTypeCode = ProductType.productTypeCode ";
            if (firstCost.Text != string.Empty)
            {
                if (firstCost.Text != Product.CheckCostForSearch(firstCost.Text, "Цена изделия не может быть отрицательной.", "Вы указали недопустимые символы в цене изделия."))
                {
                    MessageBox.Show(Product.CheckCostForSearch(firstCost.Text, "Цена изделия не может быть отрицательной.", "Вы указали недопустимые символы в цене изделия."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            if (lastCost.Text != string.Empty)
            {
                if (lastCost.Text != Product.CheckCostForSearch(lastCost.Text, "Цена изделия не может быть отрицательной.", "Вы указали недопустимые символы в цене изделия."))
                {
                    MessageBox.Show(Product.CheckCostForSearch(lastCost.Text, "Цена изделия не может быть отрицательной.", "Вы указали недопустимые символы в цене изделия."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            if (firstCost.Text != string.Empty && lastCost.Text != string.Empty && int.Parse(firstCost.Text) > int.Parse(lastCost.Text))
            {
                MessageBox.Show("Начальная цена не может быть больше конечной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (productCollectionField.Text == string.Empty && productSurfaceField.Text == string.Empty && productTypeField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty)
            {
                MessageBox.Show("Невозможно применить фильтр, так как вы не выбрали ни одного критерия.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "'";
            }
            else if (productCollectionField.Text != string.Empty && productTypeField.Text == string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productCollectionName = '" + productCollectionField.Text + "'";
            }
            else if (productSurfaceField.Text != string.Empty && productTypeField.Text == string.Empty && productCollectionField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE surfaceName = '" + productSurfaceField.Text + "'";
            }
            else if (firstCost.Text != string.Empty && productTypeField.Text == string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text == string.Empty && lastCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + "";
            }
            else if (lastCost.Text != string.Empty && productTypeField.Text == string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + "";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "'";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "'";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productCollectionName = '" + productCollectionField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "'";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty)
            {
                productsInfoQuery += "WHERE productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty)
            {
                productsInfoQuery += "WHERE surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty)
            {
                productsInfoQuery += "WHERE (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "'";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productCollectionName = '" + productCollectionField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty)
            {
                productsInfoQuery += "WHERE productCollectionName = '" + productCollectionField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty)
            {
                productsInfoQuery += "WHERE surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty)
            {
                productsInfoQuery += "WHERE surfaceName = '" + productSurfaceField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            connectionString.Open();
            DataTable table = new DataTable();
            using (SqlCommand cmd = new SqlCommand(productsInfoQuery, connectionString))
            {
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    table.Load(rdr);
                }
            }
            ProductsInfoGrid.ItemsSource = table.DefaultView;
            connectionString.Close();
        }

        private void searchField_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (searchField.Text == string.Empty) ProductsInfoGrid.SelectedIndex = -1;
            else
            {
                for (int i = 0; i < ProductsInfoGrid.Items.Count; i++)
                {
                    DataGridRow row = (DataGridRow)ProductsInfoGrid.ItemContainerGenerator.ContainerFromIndex(i);
                    if (row == null)
                    {
                        ProductsInfoGrid.ScrollIntoView(ProductsInfoGrid.Items[i]);
                        row = (DataGridRow)ProductsInfoGrid.ItemContainerGenerator.ContainerFromIndex(i);
                    }
                    TextBlock cellcontent = ProductsInfoGrid.Columns[2].GetCellContent(row) as TextBlock;
                    if (cellcontent != null && cellcontent.Text.ToString().Contains(searchField.Text))
                    {
                        object item = ProductsInfoGrid.Items[i];
                        ProductsInfoGrid.SelectedItem = item;
                        break;
                    }
                }
            }         
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            StreamReader streamReader = new StreamReader("BasketNumber.txt"); //Считывание номера заказа, к которому будет относится выбранное изделие
            string basketNumber = streamReader.ReadLine();
            streamReader.Close();
            DataRowView productInfo = (DataRowView)ProductsInfoGrid.SelectedItem;
            SqlCommand command = new SqlCommand("SELECT productCode FROM Product WHERE productArticle = @article", connectionString);
            command.Parameters.AddWithValue("@article", productInfo["productArticle"].ToString());
            connectionString.Open();
            using (SqlDataReader reader = command.ExecuteReader()) //Получение кода выбранного изделия
            {
                if (reader.Read())
                {
                    StreamWriter productFile = new StreamWriter("ProductCode.txt");
                    productFile.Write(reader["productCode"].ToString());
                    productFile.Close();
                }
            }
            connectionString.Close();

            File.WriteAllText(@"ProductCount.txt", string.Empty);
            ProductCountWindow productCountWindow = new ProductCountWindow(); //Открытие окна, где будет указано количество изделий для добавления в корзину
            productCountWindow.ShowDialog();
            int count = 0;
            StreamReader countFile = new StreamReader("ProductCount.txt");
            if (countFile == null) //Проверка на пустоту файла с количеством изделий для добавления в корзину
            {
                countFile.Close();
                return;
            }
            else
            {
                string checkContent = countFile.ReadLine();
                if (!String.IsNullOrEmpty(checkContent)) //В случае, если файл не пустой
                {
                    int.TryParse(checkContent, out count);
                    countFile.Close();

                    double lenght = 0;
                    double width = 0;
                    double weight = 0;
                    int countInBox = 0;
                    double cost = 0;
                    int code = 0;
                    string selectProductInfoQuery = "SELECT productCode, productLenght, productWidth, productBoxWeight, productCountInBox, productCostCount FROM Product WHERE productArticle = '" + productInfo["productArticle"].ToString() + "'"; //Получение информации о выбранном изделии
                    using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectProductInfoQuery, connectionString))
                    {
                        DataTable table = new DataTable();
                        dataAdapter.Fill(table);
                        if (table.Rows.Count > 0)
                        {
                            code = int.Parse(table.Rows[0]["productCode"].ToString());
                            lenght = double.Parse(table.Rows[0]["productLenght"].ToString());
                            width = double.Parse(table.Rows[0]["productWidth"].ToString());
                            weight = double.Parse(table.Rows[0]["productBoxWeight"].ToString());
                            countInBox = int.Parse(table.Rows[0]["productCountInBox"].ToString());
                            cost = double.Parse(table.Rows[0]["productCostCount"].ToString());

                            string selectUniqsProductInfoQuery = "SELECT productsCount FROM Basket JOIN Product ON Basket.productCode = Product.productCode " +
                                                                 "WHERE productArticle = '" + productInfo["productArticle"].ToString() + "' AND basketNumber = '" + basketNumber + "'"; //Проверка на наличие данного изделия
                            using (SqlDataAdapter adapter = new SqlDataAdapter(selectUniqsProductInfoQuery, connectionString))
                            {
                                DataTable dataTable = new DataTable();
                                adapter.Fill(dataTable);
                                if (dataTable.Rows.Count > 0) //В случае, если изделие имеется в корзине, его количество увеличится
                                {
                                    int currentProductsCount = int.Parse(dataTable.Rows[0]["productsCount"].ToString());
                                    count = currentProductsCount + count;
                                    SqlCommand cmd = new SqlCommand();
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = "UPDATE Basket SET productCode = @productCode, basketNumber = @number, productsCount = @count, boxesCount = @boxes, productsArea = @area, productsWeight = @weight, generalSum = @sum, paymentStatus = @status " +
                                                      "WHERE productCode = @productCode AND basketNumber = @number";
                                    cmd.Parameters.Add("@number", SqlDbType.VarChar).Value = basketNumber;
                                    cmd.Parameters.Add("@count", SqlDbType.Int).Value = count;
                                    cmd.Parameters.Add("@boxes", SqlDbType.Int).Value = count / countInBox + 1;
                                    cmd.Parameters.Add("@area", SqlDbType.Float).Value = Convert.ToDouble(count * (lenght * width / 1000000));
                                    cmd.Parameters.Add("@weight", SqlDbType.Float).Value = Convert.ToDouble(weight / countInBox * count);
                                    cmd.Parameters.Add("@sum", SqlDbType.Float).Value = Convert.ToDouble(count * cost);
                                    cmd.Parameters.Add("@productCode", SqlDbType.Float).Value = code;
                                    cmd.Parameters.Add("@status", SqlDbType.Bit).Value = false;
                                    cmd.Connection = connectionString;
                                    connectionString.Open();
                                    cmd.ExecuteNonQuery();
                                    connectionString.Close();
                                }
                                else //В случае отсутсвия изделия в корзине, случится его добавление
                                {
                                    SqlCommand cmd = new SqlCommand();
                                    cmd.CommandType = CommandType.Text;
                                    cmd.CommandText = "INSERT Basket (basketNumber, productsCount, boxesCount, productsArea, productsWeight, generalSum, productCode, paymentStatus) " +
                                                      "VALUES (@number, @count, @boxes, @area, @weight, @sum, @productCode, @status)";
                                    cmd.Parameters.Add("@number", SqlDbType.VarChar).Value = basketNumber;
                                    cmd.Parameters.Add("@count", SqlDbType.Int).Value = count;
                                    cmd.Parameters.Add("@boxes", SqlDbType.Int).Value = count / countInBox + 1;
                                    cmd.Parameters.Add("@area", SqlDbType.Float).Value = Convert.ToDouble(count * (lenght * width / 1000000));
                                    cmd.Parameters.Add("@weight", SqlDbType.Float).Value = Convert.ToDouble(weight / countInBox * count);
                                    cmd.Parameters.Add("@sum", SqlDbType.Float).Value = Convert.ToDouble(count * cost);
                                    cmd.Parameters.Add("@productCode", SqlDbType.Float).Value = code;
                                    cmd.Parameters.Add("@status", SqlDbType.Bit).Value = false;
                                    cmd.Connection = connectionString;
                                    connectionString.Open();
                                    cmd.ExecuteNonQuery();
                                    connectionString.Close();
                                }    
                            }
                            MessageBox.Show("Изделие добавлено в корзину.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                }
                else
                {
                    countFile.Close();
                    return;
                }
            }
        }
    }
}