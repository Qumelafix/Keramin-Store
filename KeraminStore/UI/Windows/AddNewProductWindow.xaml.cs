using KeraminStore.Data.Models;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Data;

namespace KeraminStore.UI.Windows
{ 
    public partial class AddNewProductWindow : UserControl
    {
        readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");
        private string imagePath = string.Empty;
        public AddNewProductWindow()
        {
            InitializeComponent();
            GetSurfaceName();
            GetProductType();
            GetAvailabilityStatus();
            GetCollectionName();
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

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //Проверки вводимых данных на корректность (начало)
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

            if (productLenght.Text != Product.CheckProductLenght(productLenght.Text, "Вы не указали длину изделия.", "Длина изделия может составлять от 98 до 900 мм.", "В длине изделия указаны недопустимые символы."))
            {
                MessageBox.Show(Product.CheckProductLenght(productLenght.Text, "Вы не указали длину изделия.", "Длина изделия может составлять от 98 до 900 мм.", "В длине изделия указаны недопустимые символы."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (productWidth.Text != Product.CheckProductWidth(productWidth.Text, "Вы не указали ширину изделия.", "Ширина изделия может составлять от 20 до 400 мм.", "В ширине изделия указаны недопустимые символы."))
            {
                MessageBox.Show(Product.CheckProductWidth(productWidth.Text, "Вы не указали ширину изделия.", "Ширина изделия может составлять от 20 до 400 мм.", "В ширине изделия указаны недопустимые символы."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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

            if (productCountInBox.Text != Product.CheckCountInBox(productCountInBox.Text, "Вы не указали количество изделий в ящике.", "Количество изделий в ящике не может быть отрицательным.", "Вы указали недопустимые символы в количестве изделий."))
            {
                MessageBox.Show(Product.CheckCountInBox(productCountInBox.Text, "Вы не указали количество изделий в ящике.", "Количество изделий в ящике не может быть отрицательным.", "Вы указали недопустимые символы в количестве изделий."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                MessageBox.Show("Необходимо загрузить картинку изделия.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //Проверки вводимых данных на корректность (конец)

            int typeCode = 0;
            int collectionCode = 0;
            int surfaceCode = 0;
            int statusCode = 0;
            string selectPostCodeQuery = "SELECT productCollectionCode, availabilityStatusCode, productTypeCode, surfaceCode FROM Surface, ProductType, AvailabilityStatus, ProductCollection " +
                                         "WHERE productCollectionName = '" + productCollection.Text + "' AND availabilityStatusName = '" + productStatusField.Text + "' AND productTypeName = '" + productTypeField.Text + "'" +
                                         "AND surfaceName = '" + productSurfaceField.Text + "'";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectPostCodeQuery, connectionString)) //Получение кодов коллекции, статуса наличия, типа и повехности изделия для последующей работы с ними
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0)
                {
                    typeCode = int.Parse(table.Rows[0]["productTypeCode"].ToString());
                    collectionCode = int.Parse(table.Rows[0]["productCollectionCode"].ToString());
                    surfaceCode = int.Parse(table.Rows[0]["surfaceCode"].ToString());
                    statusCode = int.Parse(table.Rows[0]["availabilityStatusCode"].ToString());
                }
            }

            string selectUniqProductQuery = "SELECT * FROM Product " +
                                            "JOIN ProductCollection ON Product.productCollectionCode = ProductCollection.productCollectionCode " +
                                            "JOIN ProductType ON Product.productTypeCode = ProductType.productTypeCode " +
                                            "WHERE productName= '" + productNameField.Text + "' AND productCollectionName = '" + productCollection.Text + "' AND productTypeName = '" + productTypeField.Text + "'";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectUniqProductQuery, connectionString)) //Проверка уникальности товара, то есть его наличия в базе
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0) //В случае, если таковой имеется, отобразится соответсвующее предупреждение
                {
                    MessageBox.Show("Это изделие уже имеется в базе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (table.Rows.Count == 0)
                {
                    string selectUniqArticleQuery = "SELECT * FROM Product WHERE productArticle= '" + productArticleField.Text + "'";
                    using (SqlDataAdapter articleAdapter = new SqlDataAdapter(selectUniqArticleQuery, connectionString))
                    {
                        DataTable articleTable = new DataTable();
                        articleAdapter.Fill(articleTable);
                        if (articleTable.Rows.Count > 0) //Проверка уникальности артикула
                        {
                            MessageBox.Show("Этот артикул принаджет другому изделию. Проверьте, не допустили ли вы ошибку.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    SqlCommand cmd = new SqlCommand(); //Добавление изделия в базу
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = "INSERT Product (productName, productArticle, productWidth, productLenght, productBoxWeight, productCountInBox, productCostCount, productCostArea, productImage, productDescription, productCollectionCode, availabilityStatusCode, productTypeCode, surfaceCode) " +
                           "VALUES (@name, @article, @width, @lenght, @boxWeight, @countInBox, @countCost, @areaCost, @image, @description, @collectionCode, @statusCode, @typeCode, @surfaceCode)";
                    cmd.Parameters.Add("@name", SqlDbType.VarChar).Value = productNameField.Text;
                    cmd.Parameters.Add("@article", SqlDbType.VarChar).Value = productArticleField.Text;
                    cmd.Parameters.Add("@width", SqlDbType.Float).Value = Convert.ToDouble(productWidth.Text);
                    cmd.Parameters.Add("@lenght", SqlDbType.Float).Value = Convert.ToDouble(productLenght.Text);
                    cmd.Parameters.Add("@boxWeight", SqlDbType.Float).Value = Convert.ToDouble(boxWeightField.Text);
                    cmd.Parameters.Add("@countInBox", SqlDbType.Int).Value = Convert.ToInt32(productCountInBox.Text);
                    cmd.Parameters.Add("@countCost", SqlDbType.Float).Value = Convert.ToDouble(productCostCountField.Text);
                    cmd.Parameters.Add("@areaCost", SqlDbType.Float).Value = Convert.ToDouble(productCostAreaField.Text);
                    cmd.Parameters.Add("@image", SqlDbType.VarChar).Value = productImage.Source.ToString();
                    cmd.Parameters.Add("@description", SqlDbType.VarChar).Value = productDescriptionField.Text;
                    cmd.Parameters.Add("@collectionCode", SqlDbType.Int).Value = collectionCode;
                    cmd.Parameters.Add("@statusCode", SqlDbType.Int).Value = statusCode;
                    cmd.Parameters.Add("@typeCode", SqlDbType.Int).Value = typeCode;
                    cmd.Parameters.Add("@surfaceCode", SqlDbType.Int).Value = surfaceCode;
                    cmd.Connection = connectionString;
                    connectionString.Open();
                    cmd.ExecuteNonQuery();
                    connectionString.Close();
                    MessageBox.Show("Добавление изделия прошло успешно.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    ClearFields();
                }
            }
        }

        private void ClearFields()
        {
            productNameField.Clear();
            productTypeField.SelectedIndex = -1;
            productCollection.SelectedIndex = -1;
            productLenght.Clear();
            productWidth.Clear();
            productSurfaceField.SelectedIndex = -1;
            productArticleField.Clear();
            boxWeightField.Clear();
            productCountInBox.Clear();
            productCostCountField.Clear();
            productCostAreaField.Clear();
            productDescriptionField.Clear();
            productStatusField.SelectedIndex = -1;
            productImage.Source = null;
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

        private void productCostCountField_TextChanged(object sender, TextChangedEventArgs e)
        {
            //if (productCostCountField.Text != string.Empty) productCostAreaField.IsEnabled = false;
            //else productCostAreaField.IsEnabled = true;
            if (productCostCountField.Text != string.Empty)
            {
                if (productLenght.Text != Product.CheckProductLenght(productLenght.Text, "Вы не указали длину изделия.", "Длина изделия может составлять от 98 до 900 мм.", "В длине изделия указаны недопустимые символы."))
                {
                    MessageBox.Show(Product.CheckProductLenght(productLenght.Text, "Вы не указали длину изделия.", "Длина изделия может составлять от 98 до 900 мм.", "В длине изделия указаны недопустимые символы."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    productCostCountField.Clear();
                    return;
                }
                if (productWidth.Text != Product.CheckProductWidth(productWidth.Text, "Вы не указали ширину изделия.", "Ширина изделия может составлять от 20 до 400 мм.", "В ширине изделия указаны недопустимые символы."))
                {
                    MessageBox.Show(Product.CheckProductWidth(productWidth.Text, "Вы не указали ширину изделия.", "Ширина изделия может составлять от 20 до 400 мм.", "В ширине изделия указаны недопустимые символы."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    productCostCountField.Clear();
                    return;
                }
                if (productCostCountField.Text != Product.CheckProductCostOrWeight(productCostCountField.Text, "Стоимость изделия не может быть отрицательной.", "Вы указали недопустимые символы в стоимости изделия."))
                {
                    MessageBox.Show(Product.CheckProductCostOrWeight(productCostCountField.Text, "Стоимость изделия не может быть отрицательной.", "Вы указали недопустимые символы в стоимости изделия."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    productCostCountField.Clear();
                    return;
                }
                if (productCostCountField.Text != string.Empty)
                {
                    double areaCost = Math.Round(1 / (Convert.ToDouble(productWidth.Text) * Convert.ToDouble(productLenght.Text) / 1000000) * Convert.ToDouble(productCostCountField.Text), 2);
                    if (areaCost == 0) productCostAreaField.Clear();
                    else productCostAreaField.Text = areaCost.ToString();
                }
            }    
        }

        //private void productCostAreaField_TextChanged(object sender, TextChangedEventArgs e)
        //{
        //    if (productCostAreaField.Text != string.Empty) productCostCountField.IsEnabled = false;
        //    else productCostCountField.IsEnabled = true;
        //    if (productCostAreaField.Text != string.Empty)
        //    {
        //        if (productLenght.Text != Product.CheckProductLenght(productLenght.Text, "Вы не указали длину изделия.", "Длина изделия может составлять от 98 до 900 мм.", "В длине изделия указаны недопустимые символы."))
        //        {
        //            MessageBox.Show(Product.CheckProductLenght(productLenght.Text, "Вы не указали длину изделия.", "Длина изделия может составлять от 98 до 900 мм.", "В длине изделия указаны недопустимые символы."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //            productCostAreaField.Clear();
        //            return;
        //        }
        //        if (productWidth.Text != Product.CheckProductWidth(productWidth.Text, "Вы не указали ширину изделия.", "Ширина изделия может составлять от 20 до 400 мм.", "В ширине изделия указаны недопустимые символы."))
        //        {
        //            MessageBox.Show(Product.CheckProductWidth(productWidth.Text, "Вы не указали ширину изделия.", "Ширина изделия может составлять от 20 до 400 мм.", "В ширине изделия указаны недопустимые символы."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //            productCostAreaField.Clear();
        //            return;
        //        }
        //        if (productCostAreaField.Text != Product.CheckProductCostOrWeight(productCostAreaField.Text, "Стоимость изделия не может быть отрицательной.", "Вы указали недопустимые символы в стоимости изделия."))
        //        {
        //            MessageBox.Show(Product.CheckProductCostOrWeight(productCostAreaField.Text, "Стоимость изделия не может быть отрицательной.", "Вы указали недопустимые символы в стоимости изделия."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        //            productCostAreaField.Clear();
        //            return;
        //        }
        //        if (productCostAreaField.Text != string.Empty)
        //        {
        //            double countCost = Math.Round(Convert.ToDouble(productCostAreaField.Text) / (1 / (Convert.ToDouble(productWidth.Text) * Convert.ToDouble(productLenght.Text) / 1000000)), 2);
        //            if (countCost == 0) productCostCountField.Clear();
        //            else productCostAreaField.Text = countCost.ToString();
        //        }
        //    }
        //}
    }
}