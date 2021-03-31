using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace KeraminStore.UI.Windows
{
    public partial class BasketWindow : UserControl
    {
        readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");

        public BasketWindow()
        {
            InitializeComponent();
            File.WriteAllText(@"BasketNumber.txt", string.Empty);
            OrderNumberWindow ordr = new OrderNumberWindow();
            ordr.ShowDialog();
            connectionString.Open();
            FillDataGrid();
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

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

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
    }
}