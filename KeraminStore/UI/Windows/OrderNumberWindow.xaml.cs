using KeraminStore.Data.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;

namespace KeraminStore.UI.Windows
{
    public partial class OrderNumberWindow : Window
    {
        readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");

        public OrderNumberWindow()
        {
            InitializeComponent();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (number.Text != Product.CheckCountInBox(number.Text, "Вы не указали номер заказа.", "Номер заказа не может быть отрицательным.", "Вы указали недопустимые символы в номере заказа."))
            {
                MessageBox.Show(Product.CheckCountInBox(number.Text, "Вы не указали номер заказа.", "Номер заказа не может быть отрицательным.", "Вы указали недопустимые символы в номере заказа."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string selectUniqsProductInfoQuery = "SELECT paymentStatus FROM Basket WHERE basketNumber = '" + number.Text + "'";
            using (SqlDataAdapter adapter = new SqlDataAdapter(selectUniqsProductInfoQuery, connectionString))
            {
                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);
                if (dataTable.Rows.Count > 0 && Convert.ToBoolean(dataTable.Rows[0]["paymentStatus"].ToString()) == true)
                {
                    MessageBox.Show("Данный номер принадлежит уже оплаченному заказу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            StreamWriter numberFile = new StreamWriter("BasketNumber.txt");
            numberFile.Write(number.Text);
            numberFile.Close();
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
