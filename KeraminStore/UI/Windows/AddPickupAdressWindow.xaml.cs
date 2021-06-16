using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;

namespace KeraminStore.UI.Windows
{
    public partial class AddPickupAdressWindow : Window
    {
        static string connectionString1 = ConfigurationManager.ConnectionStrings["KeraminStore.Properties.Settings.KeraminStoreConnectionString"].ConnectionString;
        //readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");
        readonly SqlConnection connectionString = new SqlConnection(connectionString1);
        public AddPickupAdressWindow()
        {
            InitializeComponent();
            GetTownName();
            GetStreetName();
        }

        private void GetTownName()
        {
            List<string> townNames = new List<string>();
            connectionString.Open();
            string query = @"SELECT pickupTownName FROM PickupTown";
            SqlCommand sqlCommand = new SqlCommand(query, connectionString);
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    townNames.Add(dataReader["pickupTownName"].ToString());
                    var newList = from i in townNames orderby i select i;
                    townField.ItemsSource = newList;
                }
            }
            dataReader.Close();
            connectionString.Close();
        }

        private void GetStreetName()
        {
            List<string> streetNames = new List<string>();
            connectionString.Open();
            string query = @"SELECT streetName FROM Street";
            SqlCommand sqlCommand = new SqlCommand(query, connectionString);
            SqlDataReader dataReader = sqlCommand.ExecuteReader();
            if (dataReader.HasRows)
            {
                while (dataReader.Read())
                {
                    streetNames.Add(dataReader["streetName"].ToString());
                    var newList = from i in streetNames orderby i select i;
                    streetField.ItemsSource = newList;
                }
            }
            dataReader.Close();
            connectionString.Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (townField.Text == string.Empty)
            {
                MessageBox.Show("Необходимо указать город.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (streetField.Text == string.Empty)
            {
                MessageBox.Show("Необходимо указать улицу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (buildingField.Text == string.Empty)
            {
                MessageBox.Show("Необходимо указать номер здания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                Regex regex = new Regex(@"^(\d{1,3}|\d{1,3}\/\d{1,2}|\d{1,3}(?:[а-я])|\d{1,3}(?:[а-я])\/\d{1,2}|\d{1,3}\/\d{1,2}(?:[а-я]))$"); /*new Regex(@"^\d{1,3}$");*/
                if (!regex.IsMatch(buildingField.Text))
                {
                    MessageBox.Show("Номер здания должен выглядеть как 15 | 15а | 15/1 | 15а/1 | 15/1а.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                int townCode = 0;
                string selectTownCodeQuery = "SELECT pickupTownCode FROM PickupTown WHERE pickupTownName = '" + townField.Text + "'";
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectTownCodeQuery, connectionString))
                {
                    DataTable table = new DataTable();
                    dataAdapter.Fill(table);
                    if (table.Rows.Count > 0) townCode = int.Parse(table.Rows[0]["pickupTownCode"].ToString());
                }

                string selectPickupQuery = "SELECT * FROM Pickup WHERE pickupTownCode = " + townCode + " AND streetName = '" + streetField.Text + "' AND building = '" + buildingField.Text + "'";
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectPickupQuery, connectionString))
                {
                    DataTable table = new DataTable();
                    dataAdapter.Fill(table);
                    if (table.Rows.Count > 0)
                    {
                        MessageBox.Show("Данный пункт самовывоза уже имеется в базе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    else if (table.Rows.Count == 0)
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "INSERT Pickup (streetName, pickupTownCode, building) VALUES (@street, @town, @building)";
                        cmd.Parameters.Add("@street", SqlDbType.VarChar).Value = streetField.Text;
                        cmd.Parameters.Add("@town", SqlDbType.Int).Value = townCode;
                        cmd.Parameters.Add("@building", SqlDbType.VarChar).Value = buildingField.Text;
                        cmd.Connection = connectionString;
                        connectionString.Open();
                        cmd.ExecuteNonQuery();
                        connectionString.Close();
                        MessageBox.Show("Новый пункт самовывоза успешно добавлен.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                        townField.SelectedIndex = -1;
                        streetField.SelectedIndex = -1;
                        buildingField.Clear();
                    }
                }
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            File.WriteAllText(@"ChangingPickup.txt", string.Empty);
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (townField.Text == string.Empty)
            {
                MessageBox.Show("Необходимо указать город.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (streetField.Text == string.Empty)
            {
                MessageBox.Show("Необходимо указать улицу.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (buildingField.Text == string.Empty)
            {
                MessageBox.Show("Необходимо указать номер здания.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                Regex regex = new Regex(@"^(\d{1,3}|\d{1,3}\/\d{1,2}|\d{1,3}(?:[а-я])|\d{1,3}(?:[а-я])\/\d{1,2}|\d{1,3}\/\d{1,2}(?:[а-я]))$"); /*new Regex(@"^\d{1,3}$");*/
                if (!regex.IsMatch(buildingField.Text))
                {
                    MessageBox.Show("Номер здания должен выглядеть как 15 | 15а | 15/1 | 15а/1 | 15/1а.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                StreamReader file = new StreamReader("ChangingPickup.txt");
                int pickupCode = Convert.ToInt32(file.ReadLine());
                file.Close();

                string townNameQ = string.Empty;
                string streetNameQ = string.Empty;
                string buildingQ = string.Empty;

                string selectPickupInfo = "SELECT * FROM Pickup JOIN PickupTown ON Pickup.pickupTownCode = PickupTown.pickupTownCode WHERE pickupCode = " + pickupCode + "";
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectPickupInfo, connectionString))
                {
                    DataTable table = new DataTable();
                    dataAdapter.Fill(table);
                    if (table.Rows.Count > 0)
                    {
                        townNameQ = table.Rows[0]["pickupTownName"].ToString();
                        streetNameQ = table.Rows[0]["streetName"].ToString();
                        buildingQ = table.Rows[0]["building"].ToString();
                    }
                }

                string selectUniqPickupQuery = "SELECT * FROM Pickup " +
                                               "JOIN PickupTown ON Pickup.pickupTownCode = PickupTown.pickupTownCode " +
                                               "WHERE pickupTownName= '" + townField.Text + "' AND streetName = '" + streetField.Text + "' AND building = '" + buildingField.Text + "'";
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectUniqPickupQuery, connectionString))
                {
                    DataTable table = new DataTable();
                    dataAdapter.Fill(table);
                    if (table.Rows.Count > 0 && (table.Rows[0]["pickupTownName"].ToString() != townNameQ || table.Rows[0]["streetName"].ToString() != streetNameQ || table.Rows[0]["building"].ToString() != buildingQ))
                    {
                        MessageBox.Show("Этот пункт самовывоза уже имеется в базе.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }

                string alterationQuery = "SELECT * FROM Pickup WHERE pickupCode = " + pickupCode + "";
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(alterationQuery, connectionString))
                {
                    int pickupTownCode = 0;

                    DataTable table = new DataTable();
                    dataAdapter.Fill(table);

                    string selectNTownCodeQuery = "SELECT pickupTownCode FROM PickupTown WHERE pickupTownName = '" + townField.Text + "'";
                    using (SqlDataAdapter codesDataAdapter = new SqlDataAdapter(selectNTownCodeQuery, connectionString))
                    {
                        DataTable codesTable = new DataTable();
                        codesDataAdapter.Fill(codesTable);
                        if (codesTable.Rows.Count > 0) pickupTownCode = Convert.ToInt32(codesTable.Rows[0]["pickupTownCode"].ToString());
                    }
                    //MessageBox.Show(table.Rows[0]["streetName"].ToString() + " - " +  streetField.Text);
                    //MessageBox.Show(table.Rows[0]["building"].ToString() + " - " + buildingField.Text);
                    //MessageBox.Show(table.Rows[0]["pickupTownCode"].ToString() + " - " + pickupTownCode.ToString());

                    if (table.Rows.Count > 0 && (table.Rows[0]["streetName"].ToString() != streetField.Text || table.Rows[0]["building"].ToString() != buildingField.Text || Convert.ToInt32(table.Rows[0]["pickupTownCode"].ToString()) != pickupTownCode))
                    {
                        SqlCommand cmd = new SqlCommand();
                        cmd.CommandType = CommandType.Text;
                        cmd.CommandText = "UPDATE Pickup SET streetName = @street, building = @building, pickupTownCode = @town WHERE pickupCode = @code";
                        cmd.Parameters.Add("@street", SqlDbType.VarChar).Value = streetField.Text;
                        cmd.Parameters.Add("@building", SqlDbType.VarChar).Value =buildingField.Text;
                        cmd.Parameters.Add("@town", SqlDbType.Int).Value = pickupTownCode;
                        cmd.Parameters.Add("@code", SqlDbType.Int).Value = pickupCode;
                        cmd.Connection = connectionString;
                        connectionString.Open();
                        cmd.ExecuteNonQuery();
                        connectionString.Close();
                        MessageBox.Show("Изменения сохранены.", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("Вы не внесли никаких изменений.", "Преупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                }
            }
        }
    }
}