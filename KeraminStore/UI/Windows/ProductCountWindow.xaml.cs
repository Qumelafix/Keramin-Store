using KeraminStore.Data.Models;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace KeraminStore.UI.Windows
{
    public partial class ProductCountWindow : Window
    {
        readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");

        public ProductCountWindow()
        {
            InitializeComponent();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            int prdctCnt = 0;
            if (countButton.IsChecked == false && areaButton.IsChecked == false)
            {
                MessageBox.Show("Вы не выбрали удобные для вас единицы измерения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (countButton.IsChecked == true)
            {
                if (countField.Text != Product.CheckCountInBox(countField.Text, "Вы не указали количество изделий для добавления.", "Количество изделий для добавления не может быть отрицательным.", "Вы указали недопустимые символы в количестве изделий."))
                {
                    MessageBox.Show(Product.CheckCountInBox(countField.Text, "Вы не указали количество изделий для добавления.", "Количество изделий для добавления не может быть отрицательным.", "Вы указали недопустимые символы в количестве изделий."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                prdctCnt = Convert.ToInt32(countField.Text);
            }
            else if (areaButton.IsChecked == true)
            {
                if (areaField.Text == string.Empty)
                {
                    MessageBox.Show("Вы не указали количество квадратных метров изделия для добавления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (areaField.Text != Product.CheckProductCostOrWeight(areaField.Text, "Количество квадратных метров изделия не может быть отрицательным.", "Вы указали недопустимые символы в количестве квадратных метров изделия."))
                {
                    MessageBox.Show(Product.CheckProductCostOrWeight(areaField.Text, "Количество квадратных метров изделия не может быть отрицательным.", "Вы указали недопустимые символы в количестве квадратных метров изделия."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else if (double.Parse(areaField.Text) == 0)
                {
                    MessageBox.Show("Количество квадратных метров изделия не может быть равно нулю.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                StreamReader reader = new StreamReader("ProductCode.txt");
                int prdctCode = int.Parse(reader.ReadLine());
                reader.Close();

                double lenght = 0;
                double width = 0;
                double weight = 0;
                int countInBox = 0;
                string selectProductInfoQuery = "SELECT productLenght, productWidth, productBoxWeight, productCountInBox FROM Product WHERE productCode = " + prdctCode + "";
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectProductInfoQuery, connectionString))
                {
                    DataTable table = new DataTable();
                    dataAdapter.Fill(table);
                    if (table.Rows.Count > 0)
                    {
                        lenght = double.Parse(table.Rows[0]["productLenght"].ToString());
                        width = double.Parse(table.Rows[0]["productWidth"].ToString());
                        weight = double.Parse(table.Rows[0]["productBoxWeight"].ToString());
                        countInBox = int.Parse(table.Rows[0]["productCountInBox"].ToString());
                    }
                }
                prdctCnt = Convert.ToInt32(Convert.ToDouble(areaField.Text) / (lenght * width / 1000000));
            }
            File.WriteAllText(@"ProductCode.txt", string.Empty);
            StreamWriter productFile = new StreamWriter("ProductCount.txt");
            productFile.Write(prdctCnt.ToString());
            productFile.Close();
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(@"ProductCode.txt", string.Empty);
            this.Close();
        }

        private void countButton_Click(object sender, RoutedEventArgs e)
        {
            if (countButton.IsChecked == true)
            {
                countField.IsEnabled = true;
                areaField.Clear();
                areaField.IsEnabled = false;
            }
        }

        private void areaButton_Click(object sender, RoutedEventArgs e)
        {
            if (areaButton.IsChecked == true)
            {
                countField.IsEnabled = false;
                countField.Clear();
                areaField.IsEnabled = true;
            }
        }

        private void areaField_MouseLeave(object sender, MouseEventArgs e)
        {
            if (areaField.Text != string.Empty)
            {
                if (areaField.Text != Product.CheckProductCostOrWeight(areaField.Text, "Количество квадратных метров изделия не может быть отрицательным.", "Вы указали недопустимые символы в количестве квадратных метров изделия."))
                {
                    MessageBox.Show(Product.CheckProductCostOrWeight(areaField.Text, "Количество квадратных метров изделия не может быть отрицательным.", "Вы указали недопустимые символы в количестве квадратных метров изделия."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    areaField.Clear();
                    return;
                }

                StreamReader reader = new StreamReader("ProductCode.txt");
                int prdctCode = int.Parse(reader.ReadLine());
                reader.Close();

                double lenght = 0;
                double width = 0;
                double weight = 0;
                int countInBox = 0;
                string selectProductInfoQuery = "SELECT productLenght, productWidth, productBoxWeight, productCountInBox FROM Product WHERE productCode = " + prdctCode + "";
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectProductInfoQuery, connectionString))
                {
                    DataTable table = new DataTable();
                    dataAdapter.Fill(table);
                    if (table.Rows.Count > 0)
                    {
                        lenght = double.Parse(table.Rows[0]["productLenght"].ToString());
                        width = double.Parse(table.Rows[0]["productWidth"].ToString());
                        weight = double.Parse(table.Rows[0]["productBoxWeight"].ToString());
                        countInBox = int.Parse(table.Rows[0]["productCountInBox"].ToString());
                    }
                }
                double realArea = Convert.ToInt32(Convert.ToDouble(areaField.Text) / (lenght * width / 1000000)) * (lenght * width / 1000000);
                areaField.Text = realArea.ToString();
            }
        }

        private void countField_MouseLeave(object sender, MouseEventArgs e)
        {
            if (countField.Text != string.Empty)
            {
                if (countField.Text != Product.CheckProductCostOrWeight(countField.Text, "Количество изделий не может быть отрицательным.", "Вы указали недопустимые символы в количестве изделий."))
                {
                    MessageBox.Show(Product.CheckProductCostOrWeight(countField.Text, "Количество изделий не может быть отрицательным.", "Вы указали недопустимые символы в количестве изделий."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    countField.Clear();
                    return;
                }
            }
        }
    }
}