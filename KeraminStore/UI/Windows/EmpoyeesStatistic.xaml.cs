using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
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
            string employeesInfoQuery = "SELECT employeeName, employeeSurname, employeePatronymic, postName, Sum(Basket.productsCount) as 'sellCount', Sum(CustomerOrder.generalSum - deliveryCost) as 'sellCost' " +
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

        private void FillDataGridByMonth()
        {
            DateTime now = DateTime.Now;
            var startDate = new DateTime(now.Year, Month.SelectedIndex, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            EmployeesInfoGrid.ItemsSource = null;
            EmployeesInfoGrid.Items.Refresh();

            string employeesInfoQuery = "SELECT employeeName, employeeSurname, employeePatronymic, postName, Sum(Basket.productsCount) as 'sellCount', Sum(CustomerOrder.generalSum - deliveryCost) as 'sellCost' " +
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