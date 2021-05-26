using System;
using System.ComponentModel;
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
    public partial class EmpoyeesStatistic : UserControl
    {
        readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");

        public EmpoyeesStatistic()
        {
            InitializeComponent();
            //connectionString.Open();
            //FillDataGrid();
            //connectionString.Close();

            var russianCulture = new CultureInfo("ru-RU");
            foreach (var month in russianCulture.DateTimeFormat.MonthNames.Take(12))
            {
                Month.Items.Add(new ListItem { Text = month });
            }
            DateTime now = DateTime.Now;
            Year.Items.Add("Все время");
            for (int i = 2020; i <= now.Year; i++) Year.Items.Add(i.ToString());         
        }

        private void FillDataGrid()
        {
            string employeesInfoQuery = "SELECT CONCAT(employeeSurname, ' ', employeeName, ' ', employeePatronymic) as 'employeeInfo', employeeName, employeeSurname, employeePatronymic, postName, Sum(Basket.productsCount) as 'sellCount', Sum(CustomerOrder.generalSum - deliveryCost) as 'sellCost' " +
                                        "FROM CustomerOrder " +
                                        "JOIN Employee ON CustomerOrder.employeeCode = Employee.employeeCode " +
                                        "JOIN Post ON Employee.postCode = Post.postCode " +
                                        "JOIN Basket ON CustomerOrder.basketCode = Basket.basketCode " +
                                        "GROUP BY employeeSurname, employeeName, employeePatronymic, postName";

            DataTable table = new DataTable();
            using (SqlCommand cmd = new SqlCommand(employeesInfoQuery, connectionString))
            {
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    table.Load(rdr);
                }
            }
            EmployeesInfoGrid.ItemsSource = table.DefaultView;
            EmployeesInfoGrid.Items.SortDescriptions.Add(new SortDescription("sellCount", ListSortDirection.Descending));
        }

        private void FillDataGridByDate()
        {
            var startDate = new DateTime(Convert.ToInt32(Year.Text), Month.SelectedIndex + 1, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            EmployeesInfoGrid.ItemsSource = null;
            EmployeesInfoGrid.Items.Refresh();

            string employeesInfoQuery = "SELECT CONCAT(employeeSurname, ' ', employeeName, ' ', employeePatronymic) as 'employeeInfo', employeeName, employeeSurname, employeePatronymic, postName, Sum(Basket.productsCount) as 'sellCount', Sum(CustomerOrder.generalSum - deliveryCost) as 'sellCost' " +
                                        "FROM CustomerOrder " +
                                        "JOIN Employee ON CustomerOrder.employeeCode = Employee.employeeCode " +
                                        "JOIN Post ON Employee.postCode = Post.postCode " +
                                        "JOIN Basket ON CustomerOrder.basketCode = Basket.basketCode " +
                                        "WHERE issueDate >= '" + startDate.ToShortDateString() + "' AND issueDate <= '" + endDate.ToShortDateString() + "' GROUP BY employeeSurname, employeeName, employeePatronymic, postName";

            DataTable table = new DataTable();
            using (SqlCommand cmd = new SqlCommand(employeesInfoQuery, connectionString))
            {
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    table.Load(rdr);
                }
            }

            EmployeesInfoGrid.ItemsSource = table.DefaultView;
            EmployeesInfoGrid.Items.SortDescriptions.Add(new SortDescription("sellCount", ListSortDirection.Descending));
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            if (Month.Text == string.Empty && Year.Text == string.Empty)
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
            else if (Year.Text == Year.Items[0].ToString())
            {
                EmployeesInfoGrid.ItemsSource = null;
                EmployeesInfoGrid.Items.Refresh();
                connectionString.Open();
                FillDataGrid();
                connectionString.Close();
                CreateReportButton.IsEnabled = true;
            }
            else
            {
                connectionString.Open();
                FillDataGridByDate();
                connectionString.Close();
                CreateReportButton.IsEnabled = true;
            }
        }

        private void Month_SelectionChanged(object sender, SelectionChangedEventArgs e) => CreateReportButton.IsEnabled = false;

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

        [DllImport("user32")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId0);

        private void CreateReportButton_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesInfoGrid.Items.Count == 0)
            {
                MessageBox.Show("Невозможно создать документ со статистикой за указанный период времени, так как в ней нет данных.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                Microsoft.Office.Interop.Excel.Workbooks excelappworkbooks;
                Microsoft.Office.Interop.Excel.Workbook excelappworkbook;
                Microsoft.Office.Interop.Excel.Sheets excelsheets;
                Microsoft.Office.Interop.Excel.Worksheet excelworksheet;
                Microsoft.Office.Interop.Excel.Range excelcells;
                string path = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                Microsoft.Office.Interop.Excel.Application excelapp = new Microsoft.Office.Interop.Excel.Application();
                excelapp.Interactive = false;
                uint processId;
                GetWindowThreadProcessId((IntPtr)excelapp.Hwnd, out processId);
                int q = 6;
                if (Year.Text == Year.Items[0].ToString())
                {
                    try
                    {
                        excelappworkbooks = excelapp.Workbooks;
                        excelappworkbook = excelapp.Workbooks.Open(Path.GetFullPath(@"EmployeesStatisticExample.xls"));
                        excelsheets = excelappworkbook.Worksheets;
                        excelworksheet = (Microsoft.Office.Interop.Excel.Worksheet)excelsheets.get_Item(1);
                        excelcells = excelworksheet.get_Range("D3");
                        excelcells.Value = Year.Text;

                        string infoQuery = "SELECT employeeName, employeeSurname, employeePatronymic, CONCAT(employeeSurname, ' ', employeeName, ' ', employeePatronymic) as 'employeeInfo', postName, Sum(Basket.productsCount) as 'sellCount', Sum(CustomerOrder.generalSum - deliveryCost) as 'sellCost' " +
                                                    "FROM CustomerOrder " +
                                                    "JOIN Employee ON CustomerOrder.employeeCode = Employee.employeeCode " +
                                                    "JOIN Post ON Employee.postCode = Post.postCode " +
                                                    "JOIN Basket ON CustomerOrder.basketCode = Basket.basketCode " +
                                                    "GROUP BY employeeSurname, employeeName, employeePatronymic, postName";

                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(infoQuery, connectionString))
                        {
                            int position = 1;
                            int k = 3;
                            DataTable table = new DataTable();
                            dataAdapter.Fill(table);
                            if (table.Rows.Count > 0)
                            {
                                for (int rows = 0; rows < table.Rows.Count; ++rows, ++position)
                                {
                                    for (int j = 2; j < 7; j++)
                                    {
                                        excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, j];
                                        excelcells.Borders.ColorIndex = 0;
                                        excelcells.Font.Size = 9;
                                        excelcells.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                        if (j == 2) excelcells.Value2 = position.ToString();
                                        else if (j == 3 || j == 4)
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
                                    k = 3;
                                }
                            }
                        }
                        path += @"\Статистика по сотрудникам за " + Year.Text + ".xls";
                        excelappworkbooks = excelapp.Workbooks;
                        excelappworkbook = excelappworkbooks[1];
                        excelappworkbook.SaveAs(path);
                        MessageBox.Show("Документ был успешно создан.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception m) { MessageBox.Show(m.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error); }
                    finally { Process.GetProcessById((int)processId).Kill(); }
                }
                else
                {
                    try
                    {
                        excelappworkbooks = excelapp.Workbooks;
                        excelappworkbook = excelapp.Workbooks.Open(Path.GetFullPath(@"EmployeesStatisticExample.xls"));
                        excelsheets = excelappworkbook.Worksheets;
                        excelworksheet = (Microsoft.Office.Interop.Excel.Worksheet)excelsheets.get_Item(1);
                        excelcells = excelworksheet.get_Range("D3");
                        excelcells.Value = Month.Text + " " + Year.Text + " г.";

                        var startDate = new DateTime(Convert.ToInt32(Year.Text), Month.SelectedIndex + 1, 1);
                        var endDate = startDate.AddMonths(1).AddDays(-1);

                        string infoQuery = "SELECT employeeName, employeeSurname, employeePatronymic, CONCAT(employeeSurname, ' ', employeeName, ' ', employeePatronymic) as 'employeeInfo', postName, Sum(Basket.productsCount) as 'sellCount', Sum(CustomerOrder.generalSum - deliveryCost) as 'sellCost' " +
                                                    "FROM CustomerOrder " +
                                                    "JOIN Employee ON CustomerOrder.employeeCode = Employee.employeeCode " +
                                                    "JOIN Post ON Employee.postCode = Post.postCode " +
                                                    "JOIN Basket ON CustomerOrder.basketCode = Basket.basketCode " +
                                                    "WHERE issueDate >= '" + startDate.ToShortDateString() + "' AND issueDate <= '" + endDate.ToShortDateString() + "' GROUP BY employeeSurname, employeeName, employeePatronymic, postName";

                        using (SqlDataAdapter dataAdapter = new SqlDataAdapter(infoQuery, connectionString))
                        {
                            int position = 1;
                            int k = 3;
                            DataTable table = new DataTable();
                            dataAdapter.Fill(table);
                            if (table.Rows.Count > 0)
                            {
                                for (int rows = 0; rows < table.Rows.Count; ++rows, ++position)
                                {
                                    for (int j = 2; j < 7; j++)
                                    {
                                        excelcells = (Microsoft.Office.Interop.Excel.Range)excelworksheet.Cells[q, j];
                                        excelcells.Borders.ColorIndex = 0;
                                        excelcells.Font.Size = 9;
                                        excelcells.Borders.LineStyle = Microsoft.Office.Interop.Excel.XlLineStyle.xlContinuous;
                                        if (j == 2) excelcells.Value2 = position.ToString();
                                        else if (j == 3 || j == 4)
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
                                    k = 3;
                                }
                            }
                        }
                        path += @"\Статистика по сотрудникам за " + (Month.Text + " " + Year.Text + " г.").ToString() + ".xls";
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