using System;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;

namespace KeraminStore.UI.Windows
{
    public partial class ProductsStatistics : UserControl
    {
        static string connectionString1 = ConfigurationManager.ConnectionStrings["KeraminStore.Properties.Settings.KeraminStoreConnectionString"].ConnectionString;
        readonly SqlConnection connectionString = new SqlConnection(connectionString1);
        public ProductsStatistics()
        {
            InitializeComponent();
            var russianCulture = new CultureInfo("ru-RU");
            foreach (var month in russianCulture.DateTimeFormat.MonthNames.Take(12)) //Загрузка месяцев в соответствующее поле
            {
                Month.Items.Add(new ListItem { Text = month });
            }
            DateTime now = DateTime.Now;
            Year.Items.Add("Все время");
            for (int i = 2020; i <= now.Year; i++) Year.Items.Add(i.ToString()); //Загрузка годов в соответствующее поле
        }

        private void FillDataGrid() //Метод для заполнения таблицы данными за все время
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

            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(productsInfoQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0)
                {
                    for (int i = 0; i < table.Rows.Count; ++i) table.Rows[i]["productImage"] = Environment.CurrentDirectory.ToString() + "\\" + table.Rows[i]["productImage"].ToString(); //Загрузка изображения изделия
                    ProductsInfoGrid.ItemsSource = table.DefaultView; //Заполнение таблицы данными
                }
            }
            ProductsInfoGrid.Items.SortDescriptions.Add(new SortDescription("sellCount", ListSortDirection.Descending));
        }

        private void FillDataGridByDate()  //Метод для заполнения таблицы данными за определенный период
        {
            var startDate = new DateTime(Convert.ToInt32(Year.Text), Month.SelectedIndex + 1, 1);
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

            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(productsInfoQuery, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0)
                {
                    for (int i = 0; i < table.Rows.Count; ++i) table.Rows[i]["productImage"] = Environment.CurrentDirectory.ToString() + "\\" + table.Rows[i]["productImage"].ToString(); //Загрузка изображения изделия
                    ProductsInfoGrid.ItemsSource = table.DefaultView; //Заполнение таблицы данными
                }
            }
            ProductsInfoGrid.Items.SortDescriptions.Add(new SortDescription("sellCount", ListSortDirection.Descending));
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e) //Метод для загрузки данных в таблицу
        {
            if (Month.Text == string.Empty && Year.Text == string.Empty) //Проверки для полей
            {
                MessageBox.Show("Для отображения статистики необходимо указать месяц и год.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (Month.Text == string.Empty && Year.Text != Year.Items[0].ToString())
            {
                MessageBox.Show("Для отображения статистики необходимо указать месяц.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (Year.Text == string.Empty)
            {
                MessageBox.Show("Для отображения статистики необходимо указать год.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (Year.Text == Year.Items[0].ToString()) //Загрузка таблицы данными за все время
            {
                ProductsInfoGrid.ItemsSource = null;
                ProductsInfoGrid.Items.Refresh();
                connectionString.Open();
                FillDataGrid();
                connectionString.Close();
                CreateReportButton.IsEnabled = true;
            }
            else //Загрузка таблицы данными за определенный период
            {
                connectionString.Open();
                FillDataGridByDate();
                connectionString.Close();
                CreateReportButton.IsEnabled = true;
            }
        }

        private void Year_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CreateReportButton.IsEnabled = false;
            if (Year.SelectedIndex == 0)
            {
                Month.SelectedIndex = -1;
                Month.IsEnabled = false;
            }
            else Month.IsEnabled = true;
        }
        private void Month_SelectionChanged(object sender, SelectionChangedEventArgs e) => CreateReportButton.IsEnabled = false;

        [DllImport("user32")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId0);

        private void CreateReportButton_Click(object sender, RoutedEventArgs e) //Метод для формирования отчета
        {
            if (ProductsInfoGrid.Items.Count == 0) //Проверка наличия данных в таблице
            {
                MessageBox.Show("Невозможно создать документ со статистикой за указанный период времени, так как в ней нет данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                Microsoft.Office.Interop.Excel.Workbooks excelappworkbooks; //Создание документа Excel с указанием пути сохранения
                Microsoft.Office.Interop.Excel.Workbook excelappworkbook;
                Microsoft.Office.Interop.Excel.Sheets excelsheets;
                Microsoft.Office.Interop.Excel.Worksheet excelworksheet;
                Microsoft.Office.Interop.Excel.Range excelcells;
                string path = Environment.CurrentDirectory + "\\ProductsStatisticsDocuments";
                Microsoft.Office.Interop.Excel.Application excelapp = new Microsoft.Office.Interop.Excel.Application();
                excelapp.Interactive = false;
                uint processId;
                GetWindowThreadProcessId((IntPtr)excelapp.Hwnd, out processId);
                int q = 6;
                if (Year.Text == Year.Items[0].ToString()) //Формирование документа за все время
                {
                    try
                    {
                        excelappworkbooks = excelapp.Workbooks;
                        excelappworkbook = excelapp.Workbooks.Open(Path.GetFullPath(@"ProductsStatisticsExample.xls")); //Открытие шаблона
                        excelsheets = excelappworkbook.Worksheets;
                        excelworksheet = (Microsoft.Office.Interop.Excel.Worksheet)excelsheets.get_Item(1);
                        excelcells = excelworksheet.get_Range("F3");
                        excelcells.Value = Year.Text;

                        string infoQuery = "SELECT productName,  productArticle, productTypeName, productCollectionName, CONCAT(productLenght, 'x', productWidth) as 'productParametrs', surfaceName, Sum(productsCount) as 'sellCount', productLenght, productWidth " +
                                       "FROM CustomerOrder " +
                                       "JOIN Basket ON CustomerOrder.basketCode = Basket.basketCode " +
                                       "JOIN Product ON Basket.productCode = Product.productCode " +
                                       "JOIN ProductCollection ON Product.productCollectionCode = ProductCollection.productCollectionCode " +
                                       "JOIN AvailabilityStatus On Product.availabilityStatusCode = AvailabilityStatus.availabilityStatusCode " +
                                       "JOIN Surface On Product.surfaceCode = Surface.surfaceCode " +
                                       "JOIN ProductType On Product.productTypeCode = ProductType.productTypeCode " +
                                       "GROUP BY productName, productImage, productArticle, productLenght, productWidth, surfaceName, productTypeName, productCollectionName";

                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(infoQuery, connectionString)) //Заполнение данными
                        {
                            int position = 1;
                            int k = 0;
                            DataTable table = new DataTable();
                            dataAdapter.Fill(table);
                            if (table.Rows.Count > 0)
                            {
                                for (int rows = 0; rows < table.Rows.Count; ++rows, ++position)
                                {
                                    for (int j = 2; j < 10; j++)
                                    {
                                        excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, j];
                                        excelcells.Borders.ColorIndex = 0;
                                        excelcells.Font.Size = 9;
                                        excelcells.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                        if (j == 2) excelcells.Value2 = position.ToString();
                                        else if (j == 3 || j == 5)
                                        {
                                            excelcells.WrapText = true;
                                            excelcells.EntireRow.AutoFit();
                                            excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlLeft;
                                            excelcells.Value = table.Rows[rows][k].ToString();
                                            ++k;
                                        }
                                        else
                                        {
                                            excelcells.Value = table.Rows[rows][k].ToString();
                                            ++k;
                                        }
                                    }
                                    ++q;
                                    k = 0;
                                }
                            }
                        }
                        path += @"\Статистика по изделиям за " + Year.Text + ".xls";
                        excelappworkbooks = excelapp.Workbooks;
                        excelappworkbook = excelappworkbooks[1];
                        excelappworkbook.SaveAs(path);
                        MessageBox.Show("Документ был успешно создан.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception m) { MessageBox.Show(m.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
                    finally { Process.GetProcessById((int)processId).Kill(); }
                }
                else //Формирование документа за определенный период
                {
                    try
                    {
                        excelappworkbooks = excelapp.Workbooks;
                        excelappworkbook = excelapp.Workbooks.Open(Path.GetFullPath(@"ProductsStatisticsExample.xls")); //Открытие шаблона
                        excelsheets = excelappworkbook.Worksheets;
                        excelworksheet = (Microsoft.Office.Interop.Excel.Worksheet)excelsheets.get_Item(1);
                        excelcells = excelworksheet.get_Range("F3");
                        excelcells.Value = Month.Text + " " + Year.Text + " г.";

                        var startDate = new DateTime(Convert.ToInt32(Year.Text), Month.SelectedIndex + 1, 1);
                        var endDate = startDate.AddMonths(1).AddDays(-1);

                        string infoQuery = "SELECT productName,  productArticle, productTypeName, productCollectionName, CONCAT(productLenght, 'x', productWidth) as 'productParametrs', surfaceName, Sum(productsCount) as 'sellCount', productLenght, productWidth " +
                                       "FROM CustomerOrder " +
                                       "JOIN Basket ON CustomerOrder.basketCode = Basket.basketCode " +
                                       "JOIN Product ON Basket.productCode = Product.productCode " +
                                       "JOIN ProductCollection ON Product.productCollectionCode = ProductCollection.productCollectionCode " +
                                       "JOIN AvailabilityStatus On Product.availabilityStatusCode = AvailabilityStatus.availabilityStatusCode " +
                                       "JOIN Surface On Product.surfaceCode = Surface.surfaceCode " +
                                       "JOIN ProductType On Product.productTypeCode = ProductType.productTypeCode " +
                                       "WHERE issueDate >= '" + startDate.ToShortDateString() + "' AND issueDate <= '" + endDate.ToShortDateString() + "' " +
                                       "GROUP BY productName, productImage, productArticle, productLenght, productWidth, surfaceName, productTypeName, productCollectionName";

                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(infoQuery, connectionString)) //Заполнение данными
                        {
                            int position = 1;
                            int k = 0;
                            DataTable table = new DataTable();
                            dataAdapter.Fill(table);
                            if (table.Rows.Count > 0)
                            {
                                for (int rows = 0; rows < table.Rows.Count; ++rows, ++position)
                                {
                                    for (int j = 2; j < 10; j++)
                                    {
                                        excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, j];
                                        excelcells.Borders.ColorIndex = 0;
                                        excelcells.Font.Size = 9;
                                        excelcells.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                        if (j == 2) excelcells.Value2 = position.ToString();
                                        else if (j == 3 || j == 5)
                                        {
                                            excelcells.WrapText = true;
                                            excelcells.EntireRow.AutoFit();
                                            excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlLeft;
                                            excelcells.Value = table.Rows[rows][k].ToString();
                                            ++k;
                                        }
                                        else
                                        {
                                            excelcells.Value = table.Rows[rows][k].ToString();
                                            ++k;
                                        }
                                    }
                                    ++q;
                                    k = 0;
                                }
                            }
                        }
                        path += @"\Статистика по изделиям за " + (Month.Text + " " + Year.Text + " г.").ToString() + ".xls";
                        excelappworkbooks = excelapp.Workbooks;
                        excelappworkbook = excelappworkbooks[1];
                        excelappworkbook.SaveAs(path);
                        MessageBox.Show("Документ был успешно создан.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception m) { MessageBox.Show(m.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
                    finally { Process.GetProcessById((int)processId).Kill(); }
                }
            }
        }
    }
}