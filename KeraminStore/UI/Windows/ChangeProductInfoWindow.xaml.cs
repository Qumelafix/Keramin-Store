using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System;
using KeraminStore.Data.Models;
using System.IO;
using System.Data;

namespace KeraminStore.UI.Windows
{
    public partial class ChangeProductInfoWindow : Window
    {
        readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");
        private string imagePath = string.Empty;

        public ChangeProductInfoWindow()
        {
            InitializeComponent();
            GetSurfaceName();
            GetProductType();
            GetAvailabilityStatus();
            GetCollectionName();
            GetColorName();
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
                    productCollection.ItemsSource = newList;
                }
            }
            dataReader.Close();
            connectionString.Close();
        }

        private void GetAvailabilityStatus()
        {
            List<string> availabilityStatusNames = new List<string>();
            connectionString.Open();
            string query = @"SELECT availabilityStatusName FROM AvailabilityStatus";
            SqlCommand sqlCommand = new SqlCommand(query, connectionString);
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    availabilityStatusNames.Add(dataReader["availabilityStatusName"].ToString());
                    var newList = from i in availabilityStatusNames orderby i select i;
                    productStatusField.ItemsSource = newList;
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

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            ProductsListWindow productsListWindow = new ProductsListWindow();
            this.Close();
            productsListWindow.ShowDialog();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            StreamReader file = new StreamReader("ChangingProduct.txt");
            int productCode = Convert.ToInt32(file.ReadLine());
            file.Close();

            string productNameQ = string.Empty;
            string productTypeQ = string.Empty;
            string productArticleQ = string.Empty;
            string productCollectionQ = string.Empty;

            string selectProductInfoQuery = "SELECT productName, productCollectionName, productTypeName, productArticle FROM Product " +
                                             "JOIN ProductCollection ON Product.productCollectionCode = ProductCollection.productCollectionCode " +
                                             "JOIN ProductType ON Product.productTypeCode = ProductType.productTypeCode " +
                                             "WHERE productCode = " + productCode + "";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectProductInfoQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0)
                {
                    productNameQ = table.Rows[0]["productName"].ToString();
                    productCollectionQ = table.Rows[0]["productCollectionName"].ToString();
                    productTypeQ = table.Rows[0]["productTypeName"].ToString();
                    productArticleQ = table.Rows[0]["productArticle"].ToString();
                }
            }

            if (productNameField.Text != Product.CheckProductName(productNameField.Text, "Наименование изделия не может быть пустым.", "Наименование изделия содержит недопустимые символы.", "Длина наименования изделия может составлять 10-100 символов."))
            {
                MessageBox.Show(Product.CheckProductName(productNameField.Text, "Наименование изделия не может быть пустым.", "Наименование изделия содержит недопустимые символы.", "Длина наименования изделия может составлять 10-100 символов."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (productTypeField.Text == string.Empty)
            {
                MessageBox.Show("Необходимо указать тип изделия.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (productCollection.Text == string.Empty)
            {
                MessageBox.Show("Необходимо указать коллекцию, к которой относится изделие.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (productLenght.Text != Product.CheckProductLenght(productLenght.Text, "Вы не указали длину изделия.", "Длина изделия может составлять от 98 до 900 мм.", "Вы указали неверную длину изделия."))
            {
                MessageBox.Show(Product.CheckProductLenght(productLenght.Text, "Вы не указали длину изделия.", "Длина изделия может составлять от 98 до 900 мм.", "Вы указали неверную длину изделия."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (productWidth.Text != Product.CheckProductWidth(productWidth.Text, "Вы не указали ширину изделия.", "Ширина изделия может составлять от 9 до 400 мм.", "Вы указали неверную ширину изделия."))
            {
                MessageBox.Show(Product.CheckProductWidth(productWidth.Text, "Вы не указали ширину изделия.", "Ширина изделия может составлять от 9 до 400 мм.", "Вы указали неверную ширину изделия."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (colorField.Text == string.Empty)
            {
                MessageBox.Show("Необходимо указать цвет изделия.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (productSurfaceField.Text == string.Empty)
            {
                MessageBox.Show("Необходимо указать поверхность изделия.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (productArticleField.Text != Product.CheckProductArticle(productArticleField.Text, "Вы не указали артикул изделия.", "Артикул должен быть указан в формате YYY12345678."))
            {
                MessageBox.Show(Product.CheckProductArticle(productArticleField.Text, "Вы не указали артикул изделия.", "Артикул должен быть указан в формате YYY12345678."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (countField.Text != Product.CheckCountInBox(countField.Text, "Вы не указали количество изделий.", "Количество изделий не может быть отрицательным.", "Вы указали неверное количество изделий."))
            {
                MessageBox.Show(Product.CheckCountInBox(countField.Text, "Вы не указали количество изделий.", "Количество изделий не может быть отрицательным.", "Вы указали неверное количество изделий."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (boxWeightField.Text == string.Empty)
            {
                MessageBox.Show("Вы не указали вес ящика.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (boxWeightField.Text != Product.CheckProductCostOrWeight(boxWeightField.Text, "Вес ящика не может быть отрицательным.", "Вы указали недопустимые символы в весе ящика."))
            {
                MessageBox.Show(Product.CheckProductCostOrWeight(boxWeightField.Text, "Вес ящика не может быть отрицательным.", "Вы указали недопустимые символы в весе ящика."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (productCountInBox.Text != Product.CheckCountInBox(productCountInBox.Text, "Вы не указали количество изделий в ящике.", "Количество изделий в ящике не может быть отрицательным.", "Вы указали неверное количество изделий."))
            {
                MessageBox.Show(Product.CheckCountInBox(productCountInBox.Text, "Вы не указали количество изделий в ящике.", "Количество изделий в ящике не может быть отрицательным.", "Вы указали неверное количество изделий."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (productCostAreaField.Text == string.Empty && productCostCountField.Text == string.Empty)
            {
                MessageBox.Show("Вы должны указать стоимость изделия за штуку.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (productCostCountField.Text != string.Empty)
            {
                if (productCostCountField.Text != Product.CheckProductCostOrWeight(productCostCountField.Text, "Стоимость изделия не может быть отрицательной.", "Вы указали недопустимые символы в стоимости изделия."))
                {
                    MessageBox.Show(Product.CheckProductCostOrWeight(productCostCountField.Text, "Стоимость изделия не может быть отрицательной.", "Вы указали недопустимые символы в стоимости изделия."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (productStatusField.Text == string.Empty)
            {
                MessageBox.Show("Необходимо указать статус наличия изделия.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (productDescriptionField.Text != Product.CheckProductDescription(productDescriptionField.Text, "Длина описания не может быть более 300 символов."))
            {
                MessageBox.Show(Product.CheckProductDescription(productDescriptionField.Text, "Длина описания не может быть более 300 символов."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (productImage.Source == null)
            {
                MessageBox.Show("Необходимо загрузить изображение изделия.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string selectUniqProductQuery = "SELECT * FROM Product " +
                                            "JOIN ProductCollection ON Product.productCollectionCode = ProductCollection.productCollectionCode " +
                                            "JOIN ProductType ON Product.productTypeCode = ProductType.productTypeCode " +
                                            "WHERE productName= '" + productNameField.Text + "' AND productCollectionName = '" + productCollection.Text + "' AND productTypeName = '" + productTypeField.Text + "'";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectUniqProductQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0 && (table.Rows[0]["productName"].ToString() != productNameQ || table.Rows[0]["productCollectionName"].ToString() != productCollectionQ || table.Rows[0]["productTypeName"].ToString() != productTypeQ))
                {
                    MessageBox.Show("Это изделие уже имеется в базе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    string selectUniqArticleQuery = "SELECT * FROM Product WHERE productArticle = '" + productArticleField.Text + "'";
                    using (SqlDataAdapter articleAdapter = new SqlDataAdapter(selectUniqArticleQuery, connectionString))
                    {
                        DataTable articleTable = new DataTable();
                        articleAdapter.Fill(articleTable);
                        if (articleTable.Rows.Count > 0 && articleTable.Rows[0]["productArticle"].ToString() != productArticleQ)
                        {
                            MessageBox.Show("Этот артикул принаджет другому изделию. Проверьте, не допустили ли вы ошибку.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }                    
                }
            }

            string alterationQuery = "SELECT * FROM Product WHERE productCode = '" + productCode + "'";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(alterationQuery, connectionString))
            {
                int surfaceCode = 0;
                int typeCode = 0;
                int collectionCode = 0;
                int statusCode = 0;
                int colorCode = 0;

                DataTable table = new DataTable();
                dataAdapter.Fill(table);

                string selectNamesQuery = "SELECT surfaceCode, productTypeCode, availabilityStatusCode, productCollectionCode, colorCode FROM ProductCollection, AvailabilityStatus, ProductType, Surface, Color " +
                                          "WHERE surfaceName = '" + productSurfaceField.Text + "' AND productTypeName = '" + productTypeField.Text + "' AND availabilityStatusName = '" + productStatusField.Text + "' " +
                                          "AND productCollectionName = '" + productCollection.Text + "' AND colorName = '" + colorField.Text + "'";
                using (SqlDataAdapter codesDataAdapter = new SqlDataAdapter(selectNamesQuery, connectionString))
                {
                    DataTable codesTable = new DataTable();
                    codesDataAdapter.Fill(codesTable);
                    if (codesTable.Rows.Count > 0)
                    {
                        surfaceCode = Convert.ToInt32(codesTable.Rows[0]["surfaceCode"].ToString());
                        typeCode = Convert.ToInt32(codesTable.Rows[0]["productTypeCode"].ToString());
                        collectionCode = Convert.ToInt32(codesTable.Rows[0]["productCollectionCode"].ToString());
                        statusCode = Convert.ToInt32(codesTable.Rows[0]["availabilityStatusCode"].ToString());
                        colorCode = Convert.ToInt32(codesTable.Rows[0]["colorCode"].ToString());
                    }
                }
                if (productCostAreaField.IsEnabled == false)
                {
                    if (table.Rows.Count > 0 && (table.Rows[0]["productName"].ToString() != productNameField.Text
                        || table.Rows[0]["productArticle"].ToString() != productArticleField.Text
                        || Convert.ToDouble(table.Rows[0]["productWidth"].ToString()) != Convert.ToDouble(productWidth.Text)
                        || Convert.ToDouble(table.Rows[0]["productLenght"].ToString()) != Convert.ToDouble(productLenght.Text)
                        || Convert.ToDouble(table.Rows[0]["productBoxWeight"].ToString()) != Convert.ToDouble(boxWeightField.Text)
                        || Convert.ToInt32(table.Rows[0]["productCount"].ToString()) != Convert.ToInt32(countField.Text)
                        || Convert.ToInt32(table.Rows[0]["productCountInBox"].ToString()) != Convert.ToInt32(productCountInBox.Text)
                        //|| Convert.ToDouble(table.Rows[0]["productCostCount"].ToString()) != Convert.ToDouble(productCostCountField.Text)
                        || table.Rows[0]["productDescription"].ToString() != productDescriptionField.Text
                        || table.Rows[0]["productImage"].ToString() != productImage.Source.ToString()
                        || Convert.ToInt32(table.Rows[0]["productCollectionCode"].ToString()) != collectionCode
                        || Convert.ToInt32(table.Rows[0]["availabilityStatusCode"].ToString()) != statusCode
                        || Convert.ToInt32(table.Rows[0]["productTypeCode"].ToString()) != typeCode
                        || Convert.ToInt32(table.Rows[0]["colorCode"].ToString()) != colorCode
                        || Convert.ToInt32(table.Rows[0]["surfaceCode"].ToString()) != surfaceCode)
                        || (table.Rows[0]["productCostCount"].ToString() != string.Empty && Convert.ToDouble(table.Rows[0]["productCostCount"].ToString()) != Convert.ToDouble(productCostCountField.Text)))
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "UPDATE Product SET productName = @name, productArticle = @article, productWidth = @width, productLenght = @lenght, productBoxWeight = @boxWeight, productCountInBox = @countInBox, productCostCount = @countCost, productCostArea = @areaCost, productImage = @image, productDescription = @description, productCollectionCode = @collectionCode, availabilityStatusCode = @statusCode, productTypeCode = @typeCode, surfaceCode = @surfaceCode, colorCode = @color, productCount = @count WHERE productCode = @code";
                        cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = productNameField.Text;
                        cmd.Parameters.Add("@article", SqlDbType.VarChar).Value = productArticleField.Text;
                        cmd.Parameters.Add("@width", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(productWidth.Text), 2);
                        cmd.Parameters.Add("@lenght", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(productLenght.Text), 2);
                        cmd.Parameters.Add("@boxWeight", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(boxWeightField.Text), 2);
                        cmd.Parameters.Add("@count", SqlDbType.Int).Value = Convert.ToInt32(countField.Text);
                        cmd.Parameters.Add("@countInBox", SqlDbType.Int).Value = Convert.ToInt32(productCountInBox.Text);
                        cmd.Parameters.Add("@image", SqlDbType.VarChar).Value = productImage.Source.ToString();
                        cmd.Parameters.Add("@description", SqlDbType.VarChar).Value = productDescriptionField.Text;
                        cmd.Parameters.Add("@collectionCode", SqlDbType.Int).Value = collectionCode;
                        cmd.Parameters.Add("@statusCode", SqlDbType.Int).Value = statusCode;
                        cmd.Parameters.Add("@typeCode", SqlDbType.Int).Value = typeCode;
                        cmd.Parameters.Add("@surfaceCode", SqlDbType.Int).Value = surfaceCode;
                        cmd.Parameters.Add("@code", SqlDbType.Int).Value = productCode;
                        cmd.Parameters.Add("@color", SqlDbType.Int).Value = colorCode;
                        cmd.Parameters.Add("@countCost", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(productCostCountField.Text), 2);
                        cmd.Parameters.Add("@areaCost", SqlDbType.Float).Value = DBNull.Value;
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
                else
                {
                    if (table.Rows.Count > 0 && (table.Rows[0]["productName"].ToString() != productNameField.Text
                        || table.Rows[0]["productArticle"].ToString() != productArticleField.Text
                        || Convert.ToDouble(table.Rows[0]["productWidth"].ToString()) != Convert.ToDouble(productWidth.Text)
                        || Convert.ToDouble(table.Rows[0]["productLenght"].ToString()) != Convert.ToDouble(productLenght.Text)
                        || Convert.ToDouble(table.Rows[0]["productBoxWeight"].ToString()) != Convert.ToDouble(boxWeightField.Text)
                        || Convert.ToInt32(table.Rows[0]["productCount"].ToString()) != Convert.ToInt32(countField.Text)
                        || Convert.ToInt32(table.Rows[0]["productCountInBox"].ToString()) != Convert.ToInt32(productCountInBox.Text)
                        //|| Convert.ToDouble(table.Rows[0]["productCostArea"].ToString()) != Convert.ToDouble(productCostAreaField.Text)
                        || table.Rows[0]["productDescription"].ToString() != productDescriptionField.Text
                        || table.Rows[0]["productImage"].ToString() != productImage.Source.ToString()
                        || Convert.ToInt32(table.Rows[0]["productCollectionCode"].ToString()) != collectionCode
                        || Convert.ToInt32(table.Rows[0]["availabilityStatusCode"].ToString()) != statusCode
                        || Convert.ToInt32(table.Rows[0]["productTypeCode"].ToString()) != typeCode
                        || Convert.ToInt32(table.Rows[0]["colorCode"].ToString()) != colorCode
                        || Convert.ToInt32(table.Rows[0]["surfaceCode"].ToString()) != surfaceCode)
                        || (table.Rows[0]["productCostArea"].ToString() != string.Empty && Convert.ToDouble(table.Rows[0]["productCostArea"].ToString()) != Convert.ToDouble(productCostAreaField.Text)))
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "UPDATE Product SET productName = @name, productArticle = @article, productWidth = @width, productLenght = @lenght, productBoxWeight = @boxWeight, productCountInBox = @countInBox, productCostCount = @countCost, productCostArea = @areaCost, productImage = @image, productDescription = @description, productCollectionCode = @collectionCode, availabilityStatusCode = @statusCode, productTypeCode = @typeCode, surfaceCode = @surfaceCode, colorCode = @color, productCount = @count WHERE productCode = @code";
                        cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = productNameField.Text;
                        cmd.Parameters.Add("@article", SqlDbType.VarChar).Value = productArticleField.Text;
                        cmd.Parameters.Add("@width", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(productWidth.Text), 2);
                        cmd.Parameters.Add("@lenght", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(productLenght.Text), 2);
                        cmd.Parameters.Add("@boxWeight", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(boxWeightField.Text), 2);
                        cmd.Parameters.Add("@count", SqlDbType.Int).Value = Convert.ToInt32(countField.Text);
                        cmd.Parameters.Add("@countInBox", SqlDbType.Int).Value = Convert.ToInt32(productCountInBox.Text);
                        cmd.Parameters.Add("@countCost", SqlDbType.Float).Value = DBNull.Value;
                        cmd.Parameters.Add("@areaCost", SqlDbType.Float).Value = Math.Round(Convert.ToDouble(productCostAreaField.Text), 2);
                        cmd.Parameters.Add("@image", SqlDbType.VarChar).Value = productImage.Source.ToString();
                        cmd.Parameters.Add("@description", SqlDbType.VarChar).Value = productDescriptionField.Text;
                        cmd.Parameters.Add("@collectionCode", SqlDbType.Int).Value = collectionCode;
                        cmd.Parameters.Add("@statusCode", SqlDbType.Int).Value = statusCode;
                        cmd.Parameters.Add("@typeCode", SqlDbType.Int).Value = typeCode;
                        cmd.Parameters.Add("@surfaceCode", SqlDbType.Int).Value = surfaceCode;
                        cmd.Parameters.Add("@code", SqlDbType.Int).Value = productCode;
                        cmd.Parameters.Add("@color", SqlDbType.Int).Value = colorCode;
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

        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                if ((filePath[filePath.Length - 1] == 'g' && filePath[filePath.Length - 2] == 'p' && filePath[filePath.Length - 3] == 'j') || (filePath[filePath.Length - 1] == 'g' && filePath[filePath.Length - 2] == 'n' && filePath[filePath.Length - 3] == 'p'))
                {
                    productImage.Source = new BitmapImage(new Uri(@"" + filePath + ""));
                    imagePath = filePath;
                }
                else
                {
                    MessageBox.Show("Вы выбрали неверный файл.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void productTypeField_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (productTypeField.SelectedIndex != -1)
            {
                if (productTypeField.SelectedItem.ToString() == "Настенная плитка" || productTypeField.SelectedItem.ToString() == "Напольная плитка")
                {
                    productCostCountField.IsEnabled = false;
                    productCostCountField.Clear();
                    productCostAreaField.IsEnabled = true;
                }
                else
                {
                    productCostCountField.IsEnabled = true;
                    productCostAreaField.Clear();
                    productCostAreaField.IsEnabled = false;
                }
            }
        }
    }
}