using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web.UI.WebControls;
using System.Windows.Controls;

namespace KeraminStore.UI.Windows
{

    public partial class ProductsStatistic : UserControl
    {
        readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");

        public ProductsStatistic()
        {
            InitializeComponent();
            connectionString.Open();
            FillDataGrid();
            connectionString.Close();

            var russianCulture = new CultureInfo("ru-RU");
            var count = 1;
            Month.Items.Add("Все время");
            foreach (var month in russianCulture.DateTimeFormat.MonthNames.Take(12))
            {
                Month.Items.Add(new ListItem { Text = month });
                count++;
            }
        }

        private void FillDataGrid()
        {
            string productsInfoQuery = "SELECT productName, productImage, productArticle, CONCAT(productLenght, 'x', productWidth) as 'productParametrs', productLenght, productWidth, surfaceName, productTypeName, productCollectionName, Sum(productsCount) as 'sellCount' " +
                                       "FROM CustomerOrder " +
                                       "JOIN Basket ON CustomerOrder.basketCode = Basket.basketCode " +
                                       "JOIN Product ON Basket.productCode = Product.productCode " +
                                       "JOIN ProductCollection ON Product.productCollectionCode = ProductCollection.productCollectionCode " +
                                       "JOIN AvailabilityStatus On Product.availabilityStatusCode = AvailabilityStatus.availabilityStatusCode " +
                                       "JOIN Surface On Product.surfaceCode = Surface.surfaceCode " +
                                       "JOIN ProductType On Product.productTypeCode = ProductType.productTypeCode " +
                                       "GROUP BY productName, productImage, productArticle, productLenght, productWidth, surfaceName, productTypeName, productCollectionName";

            DataTable table = new DataTable();
            using (SqlCommand cmd = new SqlCommand(productsInfoQuery, connectionString))
            {
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    table.Load(rdr);
                }
            }
            ProductsInfoGrid.ItemsSource = table.DefaultView;
            ProductsInfoGrid.Items.SortDescriptions.Add(new SortDescription("sellCount", ListSortDirection.Descending));
        }

        private void FillDataGridByMonth()
        {
            DateTime now = DateTime.Now;
            var startDate = new DateTime(now.Year, Month.SelectedIndex, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            ProductsInfoGrid.ItemsSource = null;
            ProductsInfoGrid.Items.Refresh();

            string productsInfoQuery = "SELECT productName, productImage, productArticle, CONCAT(productLenght, 'x', productWidth) as 'productParametrs', productLenght, productWidth, surfaceName, productTypeName, productCollectionName, Sum(productsCount) as 'sellCount' " +
                                       "FROM CustomerOrder " +
                                       "JOIN Basket ON CustomerOrder.basketCode = Basket.basketCode " +
                                       "JOIN Product ON Basket.productCode = Product.productCode " +
                                       "JOIN ProductCollection ON Product.productCollectionCode = ProductCollection.productCollectionCode " +
                                       "JOIN AvailabilityStatus On Product.availabilityStatusCode = AvailabilityStatus.availabilityStatusCode " +
                                       "JOIN Surface On Product.surfaceCode = Surface.surfaceCode " +
                                       "JOIN ProductType On Product.productTypeCode = ProductType.productTypeCode " +
                                       "WHERE issueDate >= '" + startDate.ToShortDateString() + "' AND issueDate <= '" + endDate.ToShortDateString() + "' " +
                                       "GROUP BY productName, productImage, productArticle, productLenght, productWidth, surfaceName, productTypeName, productCollectionName";

            DataTable table = new DataTable();
            using (SqlCommand cmd = new SqlCommand(productsInfoQuery, connectionString))
            {
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    table.Load(rdr);
                }
            }
            ProductsInfoGrid.ItemsSource = table.DefaultView;
            ProductsInfoGrid.Items.SortDescriptions.Add(new SortDescription("sellCount", ListSortDirection.Descending));
        }

        private void Month_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Month.SelectedIndex != 0)
            {
                connectionString.Open();
                FillDataGridByMonth();
                connectionString.Close();
            }
            else
            {
                connectionString.Open();
                FillDataGrid();
                connectionString.Close();
            }
        }
    }
}