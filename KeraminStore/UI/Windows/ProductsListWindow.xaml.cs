using System;
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
        readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");

        public ProductsListWindow()
        {
            InitializeComponent();
            connectionString.Open();
            FillDataGrid();
            connectionString.Close();
        }

        private void FillDataGrid()
        {
            string productsInfoQuery = "SELECT productName, productArticle, CONCAT(productLenght, 'x', productWidth) as 'productParametrs', productLenght, productCount, colorName, productWidth, productBoxWeight, productCountInBox, productCostCount, productCostArea, productImage, surfaceName, productTypeName, availabilityStatusName, productCollectionName " +
                                       "FROM Product " +
                                       "JOIN ProductCollection ON Product.productCollectionCode = ProductCollection.productCollectionCode " +
                                       "JOIN AvailabilityStatus On Product.availabilityStatusCode = AvailabilityStatus.availabilityStatusCode " +
                                       "JOIN Surface On Product.surfaceCode = Surface.surfaceCode " +
                                       "JOIN ProductType On Product.productTypeCode = ProductType.productTypeCode " +
                                       "JOIN Color On Product.colorCode = Color.colorCode";

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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsInfoGrid.SelectedItem == null)
            {
                MessageBox.Show("Невозможно удалить информацию об изделии, так как оно не было выбрано.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                DataRowView employeeInfo = (DataRowView)ProductsInfoGrid.SelectedItems[0];
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "DELETE FROM Product WHERE productArticle = @article";
                cmd.Parameters.Add("@article", SqlDbType.VarChar).Value = employeeInfo["productArticle"].ToString();
                cmd.Connection = connectionString;
                connectionString.Open();
                cmd.ExecuteNonQuery();
                FillDataGrid();
                connectionString.Close();
                MessageBox.Show("Удаление успешно выполнено.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
            }
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
                    TextBlock cellcontent = ProductsInfoGrid.Columns[8].GetCellContent(row) as TextBlock;
                    if (cellcontent != null && cellcontent.Text.ToString().Contains(searchField.Text.ToUpper()))
                    {
                        object item = ProductsInfoGrid.Items[i];
                        ProductsInfoGrid.SelectedItem = item;
                        break;
                    }
                }
            }
        }
        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            string description = string.Empty;
            if (ProductsInfoGrid.SelectedItem == null)
            {
                MessageBox.Show("Невозможно изменить информацию об изделии, так как оно не было выбрано.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                DataRowView productInfo = (DataRowView)ProductsInfoGrid.SelectedItems[0];
                ChangeProductInfoWindow changeProductInfoWindow = new ChangeProductInfoWindow();
                string selectEmployeeInfoQuery = "SELECT productCode, productDescription FROM Product " +
                    "JOIN ProductCollection ON Product.productCollectionCode = ProductCollection.productCollectionCode " +
                    "JOIN AvailabilityStatus On Product.availabilityStatusCode = AvailabilityStatus.availabilityStatusCode " +
                    "JOIN Surface On Product.surfaceCode = Surface.surfaceCode " +
                    "JOIN ProductType On Product.productTypeCode = ProductType.productTypeCode " +
                    "JOIN Color On Product.colorCode = Color.colorCode " +
                    "WHERE productArticle = '" + productInfo["productArticle"].ToString() + "'";
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectEmployeeInfoQuery, connectionString))
                {
                    DataTable table = new DataTable();
                    dataAdapter.Fill(table);
                    if (table.Rows.Count > 0)
                    {
                        StreamWriter productCode = new StreamWriter("ChangingProduct.txt");
                        productCode.Write(table.Rows[0]["productCode"].ToString());
                        productCode.Close();
                        description = table.Rows[0]["productDescription"].ToString();
                    }
                }
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
                changeProductInfoWindow.productImage.Source = new BitmapImage(new Uri(@"" + productInfo["productImage"].ToString() + ""));
                this.Close();
                changeProductInfoWindow.ShowDialog();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();
    }
}