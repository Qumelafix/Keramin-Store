using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace KeraminStore.UI.Windows
{
    public partial class ProductsListWindow : Window
    {
        static string connectionString1 = ConfigurationManager.ConnectionStrings["KeraminStore.Properties.Settings.KeraminStoreConnectionString"].ConnectionString;
        //readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");
        readonly SqlConnection connectionString = new SqlConnection(connectionString1);
        public ProductsListWindow()
        {
            InitializeComponent();
            connectionString.Open();
            FillDataGrid(); //Вызов метода для заполнения таблицы
            connectionString.Close();
        }

        private void FillDataGrid() //Метод для заполнения таблицы
        {
            string productsInfoQuery = "SELECT productName, productArticle, CONCAT(productLenght, 'x', productWidth) as 'productParametrs', productLenght, productCount, colorName, productWidth, productBoxWeight, productCountInBox, productCostCount, productCostArea, productImage, surfaceName, productTypeName, availabilityStatusName, productCollectionName " +
                                       "FROM Product " +
                                       "JOIN ProductCollection ON Product.productCollectionCode = ProductCollection.productCollectionCode " +
                                       "JOIN AvailabilityStatus On Product.availabilityStatusCode = AvailabilityStatus.availabilityStatusCode " +
                                       "JOIN Surface On Product.surfaceCode = Surface.surfaceCode " +
                                       "JOIN ProductType On Product.productTypeCode = ProductType.productTypeCode " +
                                       "JOIN Color On Product.colorCode = Color.colorCode";

            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(productsInfoQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0) ProductsInfoGrid.ItemsSource = table.DefaultView; //Заполнение таблицы
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) //Метод для удаления изделий
        {
            if (ProductsInfoGrid.SelectedItem == null)
            {
                MessageBox.Show("Невозможно удалить информацию об изделии, так как оно не было выбрано.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                DataRowView employeeInfo = (DataRowView)ProductsInfoGrid.SelectedItems[0]; //Получение данных о выбранном изделии
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "DELETE FROM Product WHERE productArticle = @article"; //запрос на удаление изделия из базы данных
                cmd.Parameters.Add("@article", SqlDbType.VarChar).Value = employeeInfo["productArticle"].ToString();
                cmd.Connection = connectionString;
                connectionString.Open();
                cmd.ExecuteNonQuery(); //Выполенние удаления изделия
                FillDataGrid(); //Обновление таблицы с изделиями
                connectionString.Close();
                MessageBox.Show("Удаление успешно выполнено.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e) //Метод для открытия окна для изменения данных изделия
        {
            string description = string.Empty;
            if (ProductsInfoGrid.SelectedItem == null)
            {
                MessageBox.Show("Невозможно изменить информацию об изделии, так как оно не было выбрано.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                DataRowView productInfo = (DataRowView)ProductsInfoGrid.SelectedItems[0];  //Получение данных о выбранном изделии
                ChangeProductInfoWindow changeProductInfoWindow = new ChangeProductInfoWindow();
                string selectProductInfoQuery = "SELECT productCode, productDescription FROM Product " + //Запрос на выборку данных для выбранного изделия
                    "JOIN ProductCollection ON Product.productCollectionCode = ProductCollection.productCollectionCode " +
                    "JOIN AvailabilityStatus On Product.availabilityStatusCode = AvailabilityStatus.availabilityStatusCode " +
                    "JOIN Surface On Product.surfaceCode = Surface.surfaceCode " +
                    "JOIN ProductType On Product.productTypeCode = ProductType.productTypeCode " +
                    "JOIN Color On Product.colorCode = Color.colorCode " +
                    "WHERE productArticle = '" + productInfo["productArticle"].ToString() + "'";
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectProductInfoQuery, connectionString))
                {
                    DataTable table = new DataTable();
                    dataAdapter.Fill(table);
                    if (table.Rows.Count > 0)
                    {
                        StreamWriter productCode = new StreamWriter("ChangingProduct.txt"); 
                        productCode.Write(table.Rows[0]["productCode"].ToString()); //Запись кода выбранного для изменения изделия
                        productCode.Close();
                        description = table.Rows[0]["productDescription"].ToString();
                    }
                }
                //Присваивание текущих данных выбранного изделия в соотсветсвующие поля (начало)
                changeProductInfoWindow.productNameField.Text = productInfo["productName"].ToString(); 
                changeProductInfoWindow.productTypeField.Text = productInfo["productTypeName"].ToString();
                changeProductInfoWindow.productCollection.Text = productInfo["productCollectionName"].ToString();
                changeProductInfoWindow.productLenght.Text = productInfo["productLenght"].ToString();
                changeProductInfoWindow.productWidth.Text = productInfo["productWidth"].ToString();
                changeProductInfoWindow.productSurfaceField.Text = productInfo["surfaceName"].ToString();
                changeProductInfoWindow.productArticleField.Text = productInfo["productArticle"].ToString();
                changeProductInfoWindow.colorField.Text = productInfo["colorName"].ToString();
                changeProductInfoWindow.countField.Text = productInfo["productCount"].ToString();
                changeProductInfoWindow.boxWeightField.Text = productInfo["productBoxWeight"].ToString();
                changeProductInfoWindow.productCountInBox.Text = productInfo["productCountInBox"].ToString();
                changeProductInfoWindow.productCostAreaField.Text = productInfo["productCostArea"].ToString();
                changeProductInfoWindow.productCostCountField.Text = productInfo["productCostCount"].ToString();
                changeProductInfoWindow.productDescriptionField.Text = description;
                changeProductInfoWindow.productStatusField.Text = productInfo["availabilityStatusName"].ToString();
                string newIPath = Environment.CurrentDirectory.ToString().Replace("\\bin", "*");
                string[] pathArray = newIPath.Split('*');
                changeProductInfoWindow.productImage.Source = new BitmapImage(new Uri(@"" + pathArray[0].ToString() + productInfo["productImage"].ToString() + ""));
                //Присваивание текущих данных выбранного изделия в соотсветсвующие поля (конец)
                this.Close();
                changeProductInfoWindow.ShowDialog(); //Открытие окна изменения данных изделия
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();

        private void SearchIcon_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) //Метод для поиска изделий по артикулу
        {
            if (searchField.Text == string.Empty) ProductsInfoGrid.SelectedIndex = -1; //Проверка поля для ввода артикула на пустоту
            else
            {
                for (int i = 0; i < ProductsInfoGrid.Items.Count; i++) //Проход по всей таблице с изделиями
                {
                    DataGridRow row = (DataGridRow)ProductsInfoGrid.ItemContainerGenerator.ContainerFromIndex(i);
                    if (row == null)
                    {
                        ProductsInfoGrid.ScrollIntoView(ProductsInfoGrid.Items[i]);
                        row = (DataGridRow)ProductsInfoGrid.ItemContainerGenerator.ContainerFromIndex(i);
                    }
                    TextBlock cellcontent = ProductsInfoGrid.Columns[8].GetCellContent(row) as TextBlock; //Данные со столбца с артикулом изделий
                    if (cellcontent != null && cellcontent.Text.ToString().Contains(searchField.Text.ToUpper())) //Проверка совпадений
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