using KeraminStore.Data.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
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
        static string connectionString1 = ConfigurationManager.ConnectionStrings["KeraminStore.Properties.Settings.KeraminStoreConnectionString"].ConnectionString;
        //readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");
        readonly SqlConnection connectionString = new SqlConnection(connectionString1); 
        int orderNumber = 0;

        public CatalogWindow()
        {
            InitializeComponent();
            connectionString.Open();
            FillDataGrid();
            connectionString.Close();
            GetSurfaceName();
            GetProductType();
            GetCollectionName();
            GetColorName();
            string createOrderNumber = "SELECT basketNumber FROM Basket WHERE paymentStatus = " + 0 + "";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(createOrderNumber, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0) orderNumber = int.Parse(table.Rows[0]["basketNumber"].ToString());
                else
                {
                    string findOrderNumber = "SELECT TOP 1 basketNumber FROM Basket WHERE paymentStatus = " + 1 + " ORDER BY basketNumber DESC";
                    using (SqlDataAdapter orderNumberAdapter = new SqlDataAdapter(findOrderNumber, connectionString))
                    {
                        DataTable orderNumberTable = new DataTable();
                        orderNumberAdapter.Fill(orderNumberTable);
                        if (orderNumberTable.Rows.Count > 0) orderNumber = int.Parse(orderNumberTable.Rows[0]["basketNumber"].ToString()) + 1;
                        else orderNumber = 1;
                    }
                }
            }
            StreamWriter numberFile = new StreamWriter("BasketNumber.txt");
            numberFile.Write(orderNumber.ToString());
            numberFile.Close();
        }

        private void FillDataGrid()
        {
            string productsInfoQuery = "SELECT productName, productImage, productArticle, CONCAT(productLenght, 'x', productWidth) as 'productParametrs', productLenght, productWidth, productBoxWeight, productCountInBox, productCostCount, productCostArea, productDescription, surfaceName, productTypeName, availabilityStatusName, productCollectionName " +
                                       "FROM Product " +
                                       "JOIN ProductCollection ON Product.productCollectionCode = ProductCollection.productCollectionCode " +
                                       "JOIN AvailabilityStatus On Product.availabilityStatusCode = AvailabilityStatus.availabilityStatusCode " +
                                       "JOIN Surface On Product.surfaceCode = Surface.surfaceCode " +
                                       "JOIN ProductType On Product.productTypeCode = ProductType.productTypeCode WHERE availabilityStatusName = '" + "Есть в наличии" + "'";

            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(productsInfoQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0) ProductsInfoGrid.ItemsSource = table.DefaultView;
            }
        }

        private void GetColorName()
        {
            List<string> colorNames = new List<string>();
            connectionString.Open();
            string query = @"SELECT colorName FROM Color";
            SqlCommand sqlCommand = new SqlCommand(query, connectionString);
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    colorNames.Add(dataReader["colorName"].ToString());
                    var newList = from i in colorNames orderby i select i;
                    colorField.ItemsSource = newList;
                }
            }
            dataReader.Close();
            connectionString.Close();
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
            colorField.SelectedIndex = -1;
            firstCost.Clear();
            lastCost.Clear();
            connectionString.Open();
            FillDataGrid();
            connectionString.Close();
        }

        private void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            string productsInfoQuery = "SELECT productImage, productName, productArticle, CONCAT(productLenght, 'x', productWidth) as 'productParametrs', productLenght, productWidth, productBoxWeight, productCountInBox, productCostCount, productCostArea, productDescription, surfaceName, productTypeName, availabilityStatusName, productCollectionName, colorName " +
                                       "FROM Product " +
                                       "JOIN ProductCollection ON Product.productCollectionCode = ProductCollection.productCollectionCode " +
                                       "JOIN AvailabilityStatus On Product.availabilityStatusCode = AvailabilityStatus.availabilityStatusCode " +
                                       "JOIN Surface On Product.surfaceCode = Surface.surfaceCode " +
                                       "JOIN Color ON Product.colorCode = Color.colorCode " +
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
            if (productCollectionField.Text == string.Empty && productSurfaceField.Text == string.Empty && productTypeField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                MessageBox.Show("Невозможно применить фильтр, так как вы не выбрали ни одного критерия.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "'";
            }
            else if (productCollectionField.Text != string.Empty && productTypeField.Text == string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productCollectionName = '" + productCollectionField.Text + "'";
            }
            else if (productSurfaceField.Text != string.Empty && productTypeField.Text == string.Empty && productCollectionField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE surfaceName = '" + productSurfaceField.Text + "'";
            }
            else if (firstCost.Text != string.Empty && productTypeField.Text == string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + "";
            }
            else if (lastCost.Text != string.Empty && productTypeField.Text == string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + "";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "'";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "'";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productCollectionName = '" + productCollectionField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "'";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "'";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productCollectionName = '" + productCollectionField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productCollectionName = '" + productCollectionField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE surfaceName = '" + productSurfaceField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text == string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productCollectionField.Text != string.Empty && productSurfaceField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty && colorField.Text == string.Empty)
            {
                productsInfoQuery += "WHERE productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "'";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "'";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "'";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "'";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "'";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "'";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "'";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "'";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text == string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text == string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text != string.Empty && firstCost.Text == string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text == string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text == string.Empty && productCollectionField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text == string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            else if (productTypeField.Text != string.Empty && productSurfaceField.Text != string.Empty && productCollectionField.Text != string.Empty && firstCost.Text != string.Empty && lastCost.Text != string.Empty && colorField.Text != string.Empty)
            {
                productsInfoQuery += "WHERE colorName = '" + colorField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND surfaceName = '" + productSurfaceField.Text + "' AND productCollectionName = '" + productCollectionField.Text + "' AND (productCostCount >= " + int.Parse(firstCost.Text) + " OR productCostArea >= " + int.Parse(firstCost.Text) + ") AND (productCostCount <= " + int.Parse(lastCost.Text) + " OR productCostArea <= " + int.Parse(lastCost.Text) + ")";
            }
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(productsInfoQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0) ProductsInfoGrid.ItemsSource = table.DefaultView;
                else
                {
                    ProductsInfoGrid.ItemsSource = null;
                    ProductsInfoGrid.Items.Refresh();
                }
            }
            connectionString.Close();
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
            File.WriteAllText(@"ProductCode.txt", string.Empty);
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
                    double costCount = 0;
                    double costArea = 0;
                    int code = 0;
                    string selectProductInfoQuery = "SELECT productCode, productLenght, productWidth, productBoxWeight, productCountInBox, productCostCount, productCostArea, productTypeName FROM Product " +
                                                    "JOIN ProductType ON Product.productTypeCode = ProductType.productTypeCode " +
                                                    "WHERE productArticle = '" + productInfo["productArticle"].ToString() + "'"; //Получение информации о выбранном изделии
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

                            if (table.Rows[0]["productTypeName"].ToString() != "Настенная плитка" && table.Rows[0]["productTypeName"].ToString() != "Напольная плитка" && table.Rows[0]["productTypeName"].ToString() != "Бордюр")
                            {
                                costCount = double.Parse(table.Rows[0]["productCostCount"].ToString());

                                string selectUniqsProductInfoQuery = "SELECT productsCount FROM Basket JOIN Product ON Basket.productCode = Product.productCode " +
                                                                 "WHERE productArticle = '" + productInfo["productArticle"].ToString() + "' AND basketNumber = '" + basketNumber + "'"; //Проверка на наличие данного изделия
                                using (SqlDataAdapter adapter = new SqlDataAdapter(selectUniqsProductInfoQuery, connectionString))
                                {
                                    DataTable dataTable = new DataTable();
                                    adapter.Fill(dataTable);
                                    if (dataTable.Rows.Count > 0) //В случае, если изделие имеется в корзине, его количество увеличится
                                    {
                                        int currentProductsCount = int.Parse(dataTable.Rows[0]["productsCount"].ToString());
                                        currentProductsCount = currentProductsCount + count;
                                        SqlCommand cmd = new SqlCommand();
                                        cmd.CommandType = CommandType.Text;
                                        cmd.CommandText = "UPDATE Basket SET productCode = @productCode, basketNumber = @number, productsCount = @count, boxesCount = @boxes, productsArea = @area, productsWeight = @weight, generalSum = @sum, paymentStatus = @status " +
                                                          "WHERE productCode = @productCode AND basketNumber = @number";
                                        cmd.Parameters.Add("@number", SqlDbType.VarChar).Value = basketNumber;
                                        cmd.Parameters.Add("@count", SqlDbType.Int).Value = currentProductsCount;
                                        if (currentProductsCount % countInBox != 0) cmd.Parameters.Add("@boxes", SqlDbType.Int).Value = currentProductsCount / countInBox + 1;
                                        else cmd.Parameters.Add("@boxes", SqlDbType.Int).Value = currentProductsCount / countInBox;
                                        cmd.Parameters.Add("@area", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(currentProductsCount * (lenght * width / 1000000)), 2);
                                        cmd.Parameters.Add("@weight", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(weight / countInBox * currentProductsCount), 2);
                                        cmd.Parameters.Add("@sum", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(currentProductsCount * costCount), 2);
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
                                        if (count % countInBox != 0) cmd.Parameters.Add("@boxes", SqlDbType.Int).Value = count / countInBox + 1;
                                        else cmd.Parameters.Add("@boxes", SqlDbType.Int).Value = count / countInBox;
                                        cmd.Parameters.Add("@area", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(count * (lenght * width / 1000000)), 2);
                                        cmd.Parameters.Add("@weight", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(weight / countInBox * count), 2);
                                        cmd.Parameters.Add("@sum", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(count * costCount), 2);
                                        cmd.Parameters.Add("@productCode", SqlDbType.Float).Value = code;
                                        cmd.Parameters.Add("@status", SqlDbType.Bit).Value = false;
                                        cmd.Connection = connectionString;
                                        connectionString.Open();
                                        cmd.ExecuteNonQuery();
                                        connectionString.Close();
                                    }
                                }
                            }
                            else
                            {
                                costArea = double.Parse(table.Rows[0]["productCostArea"].ToString());

                                string selectUniqsProductInfoQuery = "SELECT productsCount FROM Basket JOIN Product ON Basket.productCode = Product.productCode " +
                                                                 "WHERE productArticle = '" + productInfo["productArticle"].ToString() + "' AND basketNumber = '" + basketNumber + "'"; //Проверка на наличие данного изделия
                                using (SqlDataAdapter adapter = new SqlDataAdapter(selectUniqsProductInfoQuery, connectionString))
                                {
                                    DataTable dataTable = new DataTable();
                                    adapter.Fill(dataTable);
                                    if (dataTable.Rows.Count > 0) //В случае, если изделие имеется в корзине, его количество увеличится
                                    {
                                        int currentProductsCount = int.Parse(dataTable.Rows[0]["productsCount"].ToString());
                                        currentProductsCount = currentProductsCount + count;
                                        SqlCommand cmd = new SqlCommand();
                                        cmd.CommandType = CommandType.Text;
                                        cmd.CommandText = "UPDATE Basket SET productCode = @productCode, basketNumber = @number, productsCount = @count, boxesCount = @boxes, productsArea = @area, productsWeight = @weight, generalSum = @sum, paymentStatus = @status " +
                                                          "WHERE productCode = @productCode AND basketNumber = @number";
                                        cmd.Parameters.Add("@number", SqlDbType.VarChar).Value = basketNumber;
                                        cmd.Parameters.Add("@count", SqlDbType.Int).Value = currentProductsCount;
                                        if (currentProductsCount % countInBox != 0) cmd.Parameters.Add("@boxes", SqlDbType.Int).Value = currentProductsCount / countInBox + 1;
                                        else cmd.Parameters.Add("@boxes", SqlDbType.Int).Value = currentProductsCount / countInBox;
                                        cmd.Parameters.Add("@area", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(currentProductsCount * (lenght * width / 1000000)), 2);
                                        cmd.Parameters.Add("@weight", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(weight / countInBox * currentProductsCount), 2);
                                        cmd.Parameters.Add("@sum", SqlDbType.Float).Value = Math.Round(currentProductsCount * (costArea * (lenght * width / 1000000)), 2);
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
                                        if (count % countInBox != 0) cmd.Parameters.Add("@boxes", SqlDbType.Int).Value = count / countInBox + 1;
                                        else cmd.Parameters.Add("@boxes", SqlDbType.Int).Value = count / countInBox;
                                        cmd.Parameters.Add("@area", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(count * (lenght * width / 1000000)), 2);
                                        cmd.Parameters.Add("@weight", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(weight / countInBox * count), 2);
                                        cmd.Parameters.Add("@sum", SqlDbType.Float).Value = Math.Round(count * (costArea * (lenght * width / 1000000)), 2);
                                        cmd.Parameters.Add("@productCode", SqlDbType.Float).Value = code;
                                        cmd.Parameters.Add("@status", SqlDbType.Bit).Value = false;
                                        cmd.Connection = connectionString;
                                        connectionString.Open();
                                        cmd.ExecuteNonQuery();
                                        connectionString.Close();
                                    }
                                }
                            }                     
                            MessageBox.Show("Изделие добавлено в корзину.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);

                            string updateProductCount = "SELECT productCount FROM Product WHERE productArticle = '" + productInfo["productArticle"].ToString() + "'";
                            using (SqlDataAdapter countAdapter = new SqlDataAdapter(updateProductCount, connectionString))
                            {
                                DataTable dataTable = new DataTable();
                                countAdapter.Fill(dataTable);
                                if (dataTable.Rows.Count > 0)
                                {
                                    int productCurrentCount = int.Parse(dataTable.Rows[0]["productCount"].ToString());
                                    SqlCommand updateProductInfoCmd = new SqlCommand();
                                    updateProductInfoCmd.CommandType = CommandType.Text;
                                    if (productCurrentCount - count == 0)
                                    {
                                        updateProductInfoCmd.CommandText = "UPDATE Product SET productCount = @count, availabilityStatusCode = @status WHERE productArticle = @article";
                                        updateProductInfoCmd.Parameters.Add("@count", SqlDbType.Int).Value = productCurrentCount - count;
                                        updateProductInfoCmd.Parameters.Add("@status", SqlDbType.Int).Value = 2;
                                        updateProductInfoCmd.Parameters.Add("@article", SqlDbType.VarChar).Value = productInfo["productArticle"].ToString();
                                        updateProductInfoCmd.Connection = connectionString;
                                        connectionString.Open();
                                        updateProductInfoCmd.ExecuteNonQuery();
                                        connectionString.Close();
                                    }
                                    else
                                    {
                                        updateProductInfoCmd.CommandText = "UPDATE Product SET productCount = @count WHERE productArticle = @article";
                                        updateProductInfoCmd.Parameters.Add("@count", SqlDbType.Int).Value = productCurrentCount - count;
                                        updateProductInfoCmd.Parameters.Add("@article", SqlDbType.VarChar).Value = productInfo["productArticle"].ToString();
                                        updateProductInfoCmd.Connection = connectionString;
                                        connectionString.Open();
                                        updateProductInfoCmd.ExecuteNonQuery();
                                        connectionString.Close();
                                    }
                                }
                            }
                            ProductsInfoGrid.ItemsSource = null;
                            ProductsInfoGrid.Items.Refresh();
                            connectionString.Open();
                            FillDataGrid();
                            connectionString.Close();
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

        private void PackIconMaterial_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
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
                    if (cellcontent != null && cellcontent.Text.ToString().Contains(searchField.Text.ToUpper()))
                    {
                        object item = ProductsInfoGrid.Items[i];
                        ProductsInfoGrid.SelectedItem = item;
                        break;
                    }
                    else
                    {
                        ProductsInfoGrid.ScrollIntoView(ProductsInfoGrid.Items[0]);
                        ProductsInfoGrid.SelectedIndex = -1;
                    }
                }
            }
        }
    }
}