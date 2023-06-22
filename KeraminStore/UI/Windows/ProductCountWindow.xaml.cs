using KeraminStore.Data.Models;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace KeraminStore.UI.Windows
{
    public partial class ProductCountWindow : Window
    {
        static string connectionString1 = ConfigurationManager.ConnectionStrings["KeraminStore.Properties.Settings.KeraminStoreConnectionString"].ConnectionString;
        //readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");
        readonly SqlConnection connectionString = new SqlConnection(connectionString1);
        int prdctCode = 0;

        public ProductCountWindow()
        {
            InitializeComponent();

            StreamReader reader = new StreamReader("ProductCode.txt");
            prdctCode = int.Parse(reader.ReadLine()); //Считывание кода изделия для добавления в корзину
            reader.Close();

            string chooseItemType = "SELECT productTypeName FROM Product " +
                                    "JOIN ProductType ON Product.productTypeCode = ProductType.productTypeCode " +
                                    "WHERE productCode = " + prdctCode + "";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(chooseItemType, connectionString))
            {
                DataTable table = new DataTable();
                dataAdapter.Fill(table);

                if (table.Rows[0]["productTypeName"].ToString() != "Настенная плитка" && table.Rows[0]["productTypeName"].ToString() != "Напольная плитка" && table.Rows[0]["productTypeName"].ToString() != "Бордюр") //Проверка типа изделия
                {
                    description.Content = "Введите количество изделий в штуках";
                    septum.Visibility = Visibility.Hidden;
                    areaButton.Visibility = Visibility.Hidden;
                    areaField.Visibility = Visibility.Hidden;
                    countButton.Margin = new Thickness(162, 105, 0, 0);
                    countField.Margin = new Thickness(93, 86, 40, 119);
                }
            }
        }

        private void AddToBasketButton_Click(object sender, RoutedEventArgs e) //Метод добавления изделия в корзину
        {
            int prdctCnt = 0; //Проверка полей на корректность введенных данных
            if (countButton.IsChecked == false && areaButton.IsChecked == false)
            {
                MessageBox.Show("Вы не выбрали удобные для вас единицы измерения.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (countButton.IsChecked == true)
            {
                //if (countField.Text != Product.CheckCountInBox(countField.Text, "Вы не указали количество изделий для добавления.", "Количество изделий для добавления не может быть отрицательным.", "Вы указали недопустимые символы в количестве изделий."))
                //{
                //    MessageBox.Show(Product.CheckCountInBox(countField.Text, "Вы не указали количество изделий для добавления.", "Количество изделий для добавления не может быть отрицательным.", "Вы указали недопустимые символы в количестве изделий."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}
                if (countField.Text == string.Empty)
                {
                    MessageBox.Show("Вы не указали количество изделий для добавления.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (countField.Text != CheckCount(countField.Text, "Количество изделий не может быть меньше 1 штуки и больше 5000 штук.", "Вы указали недопустимые символы в количестве изделий."))
                {
                    MessageBox.Show(CheckCount(countField.Text, "Количество изделий не может быть меньше 1 штуки и больше 5000 штук.", "Вы указали недопустимые символы в количестве изделий."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
                //else if (areaField.Text != Product.CheckProductCostOrWeight(areaField.Text, "Количество квадратных метров изделия указано неверно.", "Вы указали недопустимые символы в количестве квадратных метров изделия."))
                //{
                //    MessageBox.Show(Product.CheckProductCostOrWeight(areaField.Text, "Количество квадратных метров изделия указано неверно.", "Вы указали недопустимые символы в количестве квадратных метров изделия."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}
                if (areaField.Text != CheckArea(areaField.Text, "Количество квадратных метров изделия не может быть меньше 0.01 м² и больше 150 м².", "Вы указали недопустимые символы в количестве квадратных метров изделия."))
                {
                    MessageBox.Show(CheckArea(areaField.Text, "Количество квадратных метров изделия не может быть меньше 0.01 м² и больше 150 м².", "Вы указали недопустимые символы в количестве квадратных метров изделия."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                //else if (double.Parse(areaField.Text) == 0)
                //{
                //    MessageBox.Show("Количество квадратных метров изделия не может быть равно нулю.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                //    return;
                //}

                StreamReader reader = new StreamReader("ProductCode.txt");
                int prdctCode = int.Parse(reader.ReadLine()); //Считывание кода изделия для добавления в корзину
                reader.Close();

                double lenght = 0;
                double width = 0;
                string selectProductInfoQuery = "SELECT productLenght, productWidth FROM Product WHERE productCode = " + prdctCode + "";
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectProductInfoQuery, connectionString)) //Получение размеров изделия
                {
                    DataTable table = new DataTable();
                    dataAdapter.Fill(table);
                    if (table.Rows.Count > 0)
                    {
                        lenght = double.Parse(table.Rows[0]["productLenght"].ToString());
                        width = double.Parse(table.Rows[0]["productWidth"].ToString());
                    }
                }
                prdctCnt = Convert.ToInt32(Convert.ToDouble(areaField.Text) / (lenght * width / 1000000)); //Вычисление количества изделий в штуках
            }

            string selectProductCount = "SELECT productCount FROM Product WHERE productCode = " + prdctCode + "";
            using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectProductCount, connectionString)) //Проверка наличия необходимого количества изделий на складе
            {
                int currentProductCount = 0;
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                if (table.Rows.Count > 0)
                {
                    currentProductCount = int.Parse(table.Rows[0]["productCount"].ToString());
                    if (currentProductCount < prdctCnt)
                    {
                        MessageBox.Show("Данного количества изделий нет в наличии.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else
                    {
                        StreamWriter productFile = new StreamWriter("ProductCount.txt");
                        productFile.Write(prdctCnt.ToString()); //Запись необходимого количества изделий для добавления
                        productFile.Close();
                    }
                }
            }
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();

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
                areaField.IsEnabled = false;
            }
        }

        private void areaButton_Click(object sender, RoutedEventArgs e)
        {
            if (areaButton.IsChecked == true)
            {
                countField.IsEnabled = false;
                areaField.IsEnabled = true;
            }
        }

        private void areaField_MouseLeave(object sender, MouseEventArgs e) //Метод для динамического расчета количества изделий в квадратных метрах и штуках
        {
            if (areaField.Text != string.Empty) //Проверка поля на пустоту
            {
                if (areaField.Text != CheckArea(areaField.Text, "Количество квадратных метров изделия не может быть меньше 0.01 м² и больше 150 м².", "Вы указали недопустимые символы в количестве квадратных метров изделия."))
                {
                    MessageBox.Show(CheckArea(areaField.Text, "Количество квадратных метров изделия не может быть меньше 0.01 м² и больше 150 м².", "Вы указали недопустимые символы в количестве квадратных метров изделия."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    areaField.Clear();
                    countField.Clear();
                    return;
                }

                StreamReader reader = new StreamReader("ProductCode.txt");
                int prdctCode = int.Parse(reader.ReadLine()); //Считывание кода изделия
                reader.Close();

                double lenght = 0;
                double width = 0;
                string selectProductInfoQuery = "SELECT productLenght, productWidth FROM Product WHERE productCode = " + prdctCode + "";
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectProductInfoQuery, connectionString)) //Получение размеров изделия
                {
                    DataTable table = new DataTable();
                    dataAdapter.Fill(table);
                    if (table.Rows.Count > 0)
                    {
                        lenght = double.Parse(table.Rows[0]["productLenght"].ToString());
                        width = double.Parse(table.Rows[0]["productWidth"].ToString());
                    }
                }
                double realArea = Convert.ToInt32(Convert.ToDouble(areaField.Text) / (lenght * width / 1000000)) * (lenght * width / 1000000); //Расчет квадратных метров изделий
                if (Convert.ToDouble(areaField.Text) > Math.Round(realArea, 4)) //Корректировка полученных данных
                {
                    realArea = Convert.ToInt32(Convert.ToDouble(areaField.Text) / (lenght * width / 1000000) + 1) * (lenght * width / 1000000);
                    areaField.Text = realArea.ToString();
                    countField.Text = Convert.ToInt32(realArea / (lenght * width / 1000000)).ToString();
                }
                else
                {
                    areaField.Text = realArea.ToString();
                    countField.Text = Convert.ToInt32(Convert.ToDouble(realArea) / (lenght * width / 1000000)).ToString();
                }
            }
            else countField.Clear();
        }

        private void countField_MouseLeave(object sender, MouseEventArgs e) //Метод для динамического расчета количества изделий в квадратных метрах
        {
            if (countField.Text != string.Empty) //Проверка поля на пустоту
            {
                if (countField.Text != CheckCount(countField.Text, "Количество изделий не может быть меньше 1 штуки и больше 5000 штук.", "Вы указали недопустимые символы в количестве изделий."))
                {
                    MessageBox.Show(CheckCount(countField.Text, "Количество изделий не может быть меньше 1 штуки и больше 5000 штук.", "Вы указали недопустимые символы в количестве изделий."), "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    countField.Clear();
                    areaField.Clear();
                    return;
                }

                StreamReader reader = new StreamReader("ProductCode.txt");
                int prdctCode = int.Parse(reader.ReadLine()); //Считывание кода изделия
                reader.Close();

                double lenght = 0;
                double width = 0;
                string selectProductInfoQuery = "SELECT productLenght, productWidth FROM Product WHERE productCode = " + prdctCode + "";
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectProductInfoQuery, connectionString)) //Получение размеров изделия
                {
                    DataTable table = new DataTable();
                    dataAdapter.Fill(table);
                    if (table.Rows.Count > 0)
                    {
                        lenght = double.Parse(table.Rows[0]["productLenght"].ToString());
                        width = double.Parse(table.Rows[0]["productWidth"].ToString());
                    }
                }
                double realArea = Convert.ToInt32(countField.Text) * (lenght * width / 1000000);
                areaField.Text = realArea.ToString();
            }
            else areaField.Clear();
        }

        private string CheckCount(string count, string wrongValue, string invalidSymbols) //Метод для проверки введенных данных
        {
            int inputCount = 0;
            bool isNum = int.TryParse(count, out inputCount);
            if (isNum)
            {
                if (inputCount < 1 || inputCount > 5000) return wrongValue;
            }
            else return invalidSymbols;
            return count;
        }

        private string CheckArea(string area, string wrongValue, string invalidSymbols) //Метод для проверки введенных данных
        {
            double inputArea = 0;
            bool isNum = double.TryParse(area, out inputArea);
            if (isNum)
            {
                if (inputArea < 0.01 || inputArea > 150) return wrongValue;
            }
            else return invalidSymbols;
            return area;
        }
    }
}