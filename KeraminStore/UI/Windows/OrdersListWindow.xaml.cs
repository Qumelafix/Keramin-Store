using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;

namespace KeraminStore.UI.Windows
{
    public partial class OrdersListWindow : UserControl
    {
        static string connectionString1 = ConfigurationManager.ConnectionStrings["KeraminStore.Properties.Settings.KeraminStoreConnectionString"].ConnectionString;
        //readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");
        readonly SqlConnection connectionString = new SqlConnection(connectionString1);
        public OrdersListWindow()
        {
            InitializeComponent();
            connectionString.Open();
            FillDataGrid();
            connectionString.Close();
        }

        private void FillDataGrid()
        {
            string productsInfoQuery = "SELECT CONCAT(customerSurname, ' ', customerName, ' ', customerPatronymic) as 'customerInfo', CustomerOrder.orderNumber as 'order', customerSurname, customerName, customerPatronymic, legalName, UTN, phone, mail, issueDate, generalSum " +
                                       "FROM CustomerOrder " +
                                       "JOIN Customer ON CustomerOrder.customerCode = Customer.customerCode " +
                                       "GROUP BY CustomerOrder.orderNumber, customerSurname, customerName, customerPatronymic, legalName, UTN, phone, mail, issueDate, generalSum";

            DataTable table = new DataTable();
            using (SqlCommand cmd = new SqlCommand(productsInfoQuery, connectionString))
            {
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    table.Load(rdr);
                }
            }
            OrdersInfoGrid.ItemsSource = table.DefaultView;
        }

        [DllImport("user32")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId0);

        private void CreateOrderButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (OrdersInfoGrid.SelectedItem == null) //Проверка выбора заказа, чек по которому будет сформирован
            {
                MessageBox.Show("Невозможно сформировать чек по заказу, так как он не был выбран.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                DataRowView orderInfoInfo = (DataRowView)OrdersInfoGrid.SelectedItems[0]; //Получание информации о заказе

                string basketNumber = orderInfoInfo["order"].ToString();

                Microsoft.Office.Interop.Excel.Workbooks excelappworkbooks;
                Microsoft.Office.Interop.Excel.Workbook excelappworkbook;
                Microsoft.Office.Interop.Excel.Sheets excelsheets;
                Microsoft.Office.Interop.Excel.Worksheet excelworksheet;
                Microsoft.Office.Interop.Excel.Range excelcells;
                string path = Environment.CurrentDirectory + "\\OrdersDocuments"; /*Environment.GetFolderPath(Environment.SpecialFolder.Desktop);*/
                Microsoft.Office.Interop.Excel.Application excelapp = new Microsoft.Office.Interop.Excel.Application();
                excelapp.Interactive = false;
                uint processId;
                GetWindowThreadProcessId((IntPtr)excelapp.Hwnd, out processId);

                if (orderInfoInfo["legalName"].ToString() == string.Empty)
                {
                    int q = 16;
                    try
                    {
                        excelappworkbooks = excelapp.Workbooks;
                        excelappworkbook = excelapp.Workbooks.Open(Path.GetFullPath(@"SalesReceiptExample.xls"));
                        excelsheets = excelappworkbook.Worksheets;
                        excelworksheet = (Microsoft.Office.Interop.Excel.Worksheet)excelsheets.get_Item(1);
                        excelcells = excelworksheet.get_Range("D5");
                        excelcells.Value = basketNumber;
                        excelcells = excelworksheet.get_Range("F5");
                        excelcells.Value = Convert.ToDateTime(orderInfoInfo["issueDate"].ToString()).ToShortDateString();
                        excelcells = excelworksheet.get_Range("C13");
                        excelcells.Value = "ОАО \"Керамин\"";
                        string infoQuery = "SELECT customerName, customerSurname, customerPatronymic, phone, mail, productName, productCostCount, productCostArea, productsCount, Basket.generalSum, CustomerOrder.generalSum, employeeSurname, employeeName, employeePatronymic FROM CustomerOrder " +
                                           "JOIN Basket ON CustomerOrder.basketCode = Basket.basketCode " +
                                           "JOIN Product ON Basket.productCode = Product.productCode " +
                                           "JOIN Customer ON CustomerOrder.customerCode = Customer.customerCode " +
                                           "JOIN Employee ON CustomerOrder.employeeCode = Employee.employeeCode " +
                                           "WHERE CustomerOrder.orderNumber = " + basketNumber + "";
                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(infoQuery, connectionString))
                        {
                            int position = 1;
                            int k = 5;
                            string ed = "шт.";
                            DataTable table = new DataTable();
                            dataAdapter.Fill(table);
                            if (table.Rows.Count > 0)
                            {
                                for (int rows = 0; rows < table.Rows.Count; ++rows, ++position)
                                {
                                    for (int j = 2; j < 9; j++)
                                    {
                                        excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, j];
                                        excelcells.Borders.ColorIndex = 0;
                                        excelcells.Font.Size = 9;
                                        excelcells.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                        if (j == 2) excelcells.Value2 = position.ToString();
                                        else if (j == 3)
                                        {
                                            excelcells.WrapText = true;
                                            excelcells.EntireRow.AutoFit();
                                            excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlLeft;
                                            excelcells.Value = table.Rows[rows][k].ToString();
                                            ++k;
                                        }
                                        else if (j == 4) excelcells.Value2 = ed.ToString();
                                        else
                                        {
                                            excelcells.Value = table.Rows[rows][k].ToString();
                                            ++k;
                                        }
                                    }
                                    ++q;
                                    k = 5;
                                }
                                excelcells = excelworksheet.get_Range("C7");
                                excelcells.Value = table.Rows[0]["customerSurname"].ToString() + " " + table.Rows[0]["customerName"].ToString() + " " + table.Rows[0]["customerPatronymic"].ToString();

                                excelcells = excelworksheet.get_Range("C9");
                                excelcells.Value = table.Rows[0]["phone"].ToString();

                                excelcells = excelworksheet.get_Range("C11");
                                excelcells.Value = table.Rows[0]["mail"].ToString();

                                for (int j = 2; j < 9; j++)
                                {
                                    excelcells.Font.Size = 9;
                                    excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, j];
                                    excelcells.Borders.ColorIndex = 0;
                                    excelcells.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                    if (j == 2)
                                    {
                                        excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlCenter;
                                        excelcells.Value2 = "ИТОГО";
                                    }
                                    else if (j == 7) { }
                                    else if (j == 8)
                                    {
                                        excelcells.Font.Size = 9;
                                        excelcells.Value2 = table.Rows[0][10].ToString();
                                    }
                                    else excelcells.Value2 = "х";
                                }

                                q = q + 2;
                                excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 1];
                                excelcells = excelworksheet.Range[excelworksheet.Cells[q, 1], excelworksheet.Cells[q, 2]];
                                excelcells.Merge(true);
                                excelcells.Font.Size = 9;
                                excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlRight;
                                excelcells.Value2 = "ФИО сотрудника:";
                                excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 6];
                                excelcells.Font.Size = 9;
                                excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlRight;
                                excelcells.Value2 = "Подпись:";
                                excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 7];
                                excelcells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                excelcells.Value2 = " ";
                                excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 3];
                                string employeeInfo = table.Rows[0]["employeeSurname"].ToString() + " " + table.Rows[0]["employeeName"].ToString() + " " + table.Rows[0]["employeePatronymic"].ToString();
                                excelcells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlLeft;
                                excelcells.Font.Size = 9;
                                excelcells.Value2 = "    " + employeeInfo;
                            }
                        }
                        path += @"\Чек №" + basketNumber.ToString() + ".xls";
                        excelappworkbooks = excelapp.Workbooks;
                        excelappworkbook = excelappworkbooks[1];
                        excelappworkbook.SaveAs(path);
                        MessageBox.Show("Чек был успешно создан.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception m) { MessageBox.Show(m.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
                    finally { Process.GetProcessById((int)processId).Kill(); }
                }
                else
                {
                    int q = 18;
                    try
                    {
                        excelappworkbooks = excelapp.Workbooks;
                        excelappworkbook = excelapp.Workbooks.Open(Path.GetFullPath(@"LegalSalesReceiptExample.xls"));
                        excelsheets = excelappworkbook.Worksheets;
                        excelworksheet = (Microsoft.Office.Interop.Excel.Worksheet)excelsheets.get_Item(1);
                        excelcells = excelworksheet.get_Range("D5");
                        excelcells.Value = basketNumber;
                        excelcells = excelworksheet.get_Range("F5");
                        excelcells.Value = Convert.ToDateTime(orderInfoInfo["issueDate"].ToString()).ToShortDateString();
                        excelcells = excelworksheet.get_Range("C15");
                        excelcells.Value = "ОАО \"Керамин\"";
                        string infoQuery = "SELECT legalName, UTN, phone, mail, productName, productCostCount, productCostArea, productsCount, Basket.generalSum, CustomerOrder.generalSum, employeeSurname, employeeName, employeePatronymic FROM CustomerOrder " +
                                           "JOIN Basket ON CustomerOrder.basketCode = Basket.basketCode " +
                                           "JOIN Product ON Basket.productCode = Product.productCode " +
                                           "JOIN Customer ON CustomerOrder.customerCode = Customer.customerCode " +
                                           "JOIN Employee ON CustomerOrder.employeeCode = Employee.employeeCode " +
                                           "WHERE CustomerOrder.orderNumber = " + basketNumber + "";
                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(infoQuery, connectionString))
                        {
                            int position = 1;
                            int k = 4;
                            string ed = "шт.";
                            DataTable table = new DataTable();
                            dataAdapter.Fill(table);
                            if (table.Rows.Count > 0)
                            {
                                for (int rows = 0; rows < table.Rows.Count; ++rows, ++position)
                                {
                                    for (int j = 2; j < 9; j++)
                                    {
                                        excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, j];
                                        excelcells.Borders.ColorIndex = 0;
                                        excelcells.Font.Size = 9;
                                        excelcells.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                        if (j == 2) excelcells.Value2 = position.ToString();
                                        else if (j == 3)
                                        {
                                            excelcells.WrapText = true;
                                            excelcells.EntireRow.AutoFit();
                                            excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlLeft;
                                            excelcells.Value = table.Rows[rows][k].ToString();
                                            ++k;
                                        }
                                        else if (j == 4) excelcells.Value2 = ed.ToString();
                                        else
                                        {
                                            excelcells.Value = table.Rows[rows][k].ToString();
                                            ++k;
                                        }
                                    }
                                    ++q;
                                    k = 4;
                                }
                                excelcells = excelworksheet.get_Range("C7");
                                excelcells.Value = table.Rows[0]["legalName"].ToString();

                                excelcells = excelworksheet.get_Range("C9");
                                excelcells.Value = table.Rows[0]["UTN"].ToString();

                                excelcells = excelworksheet.get_Range("C11");
                                excelcells.Value = table.Rows[0]["phone"].ToString();

                                excelcells = excelworksheet.get_Range("C13");
                                excelcells.Value = table.Rows[0]["mail"].ToString();

                                for (int j = 2; j < 9; j++)
                                {
                                    excelcells.Font.Size = 9;
                                    excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, j];
                                    excelcells.Borders.ColorIndex = 0;
                                    excelcells.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                    if (j == 2)
                                    {
                                        excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlCenter;
                                        excelcells.Value2 = "ИТОГО";
                                    }
                                    else if (j == 7) { }
                                    else if (j == 8)
                                    {
                                        excelcells.Font.Size = 9;
                                        excelcells.Value2 = table.Rows[0][9].ToString();
                                    }
                                    else excelcells.Value2 = "х";
                                }

                                q = q + 2;
                                excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 1];
                                excelcells = excelworksheet.Range[excelworksheet.Cells[q, 1], excelworksheet.Cells[q, 2]];
                                excelcells.Merge(true);
                                excelcells.Font.Size = 9;
                                excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlRight;
                                excelcells.Value2 = "ФИО сотрудника:";
                                excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 6];
                                excelcells.Font.Size = 9;
                                excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlRight;
                                excelcells.Value2 = "Подпись:";
                                excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 7];
                                excelcells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                excelcells.Value2 = " ";
                                excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, 3];
                                string employeeInfo = table.Rows[0]["employeeSurname"].ToString() + " " + table.Rows[0]["employeeName"].ToString() + " " + table.Rows[0]["employeePatronymic"].ToString();
                                excelcells.Borders.get_Item(Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom).LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                excelcells.HorizontalAlignment = Microsoft.Office.Interop.Excel.Constants.xlLeft;
                                excelcells.Font.Size = 9;
                                excelcells.Value2 = "    " + employeeInfo;
                            }
                        }
                        path += @"\Чек №" + basketNumber.ToString() + ".xls";
                        excelappworkbooks = excelapp.Workbooks;
                        excelappworkbook = excelappworkbooks[1];
                        excelappworkbook.SaveAs(path);
                        MessageBox.Show("Чек был успешно создан.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception m) { MessageBox.Show(m.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
                    finally { Process.GetProcessById((int)processId).Kill(); }
                }
            }
        }
    }
}