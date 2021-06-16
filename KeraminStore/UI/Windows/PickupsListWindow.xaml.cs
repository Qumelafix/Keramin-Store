using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace KeraminStore.UI.Windows
{
    public partial class PickupsListWindow : UserControl
    {
        static string connectionString1 = ConfigurationManager.ConnectionStrings["KeraminStore.Properties.Settings.KeraminStoreConnectionString"].ConnectionString;
        //readonly SqlConnection connectionString = new SqlConnection(@"Data Source=(local)\SQLEXPRESS; Initial Catalog=KeraminStore; Integrated Security=True");
        readonly SqlConnection connectionString = new SqlConnection(connectionString1);
        public PickupsListWindow()
        {
            InitializeComponent();

            connectionString.Open();
            FillDataGrid();
            connectionString.Close();
        }

        private void FillDataGrid()
        {
            string pickupsInfoQuery = "SELECT pickupTownName, Pickup.streetName, Pickup.building FROM Pickup JOIN PickupTown ON Pickup.pickupTownCode = PickupTown.pickupTownCode";

            DataTable table = new DataTable();
            using (SqlCommand cmd = new SqlCommand(pickupsInfoQuery, connectionString))
            {
                using (IDataReader rdr = cmd.ExecuteReader())
                {
                    table.Load(rdr);
                }
            }
            PickupsInfoGrid.ItemsSource = table.DefaultView;
        }

        private void ChangeButton_Click(object sender, RoutedEventArgs e)
        {
            if (PickupsInfoGrid.SelectedItem == null)
            {
                MessageBox.Show("Невозможно изменить информацию о пункте самовывоза, так как он не был выбран.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                DataRowView pickupInfo = (DataRowView)PickupsInfoGrid.SelectedItems[0];
                AddPickupAdressWindow changePickupInfo = new AddPickupAdressWindow();
                string selectEmployeeInfoQuery = "SELECT pickupCode FROM Pickup JOIN PickupTown ON Pickup.pickupTownCode = PickupTown.pickupTownCode " +
                                                 "WHERE pickupTownName = '" + pickupInfo["pickupTownName"].ToString() + "' AND streetName = '" + pickupInfo["streetName"].ToString() + "' AND building = '" + pickupInfo["building"].ToString() + "'";
                using (SqlDataAdapter dataAdapter = new SqlDataAdapter(selectEmployeeInfoQuery, connectionString))
                {
                    DataTable table = new DataTable();
                    dataAdapter.Fill(table);
                    if (table.Rows.Count > 0)
                    {
                        StreamWriter pickupCode = new StreamWriter("ChangingPickup.txt");
                        pickupCode.Write(table.Rows[0]["pickupCode"].ToString());
                        pickupCode.Close();
                    }
                }
                changePickupInfo.townField.Text = pickupInfo["pickupTownName"].ToString();
                changePickupInfo.streetField.Text = pickupInfo["streetName"].ToString();
                changePickupInfo.buildingField.Text = pickupInfo["building"].ToString();
                changePickupInfo.windowName.Content = "Изменение пункта самовывоза";
                changePickupInfo.windowDescription.Content = "Внесите изменения в необходимых полях";
                changePickupInfo.AddButton.Visibility = Visibility.Hidden;
                changePickupInfo.SaveButton.Visibility = Visibility.Visible;
                changePickupInfo.ShowDialog();
                PickupsInfoGrid.ItemsSource = null;
                PickupsInfoGrid.Items.Refresh();
                connectionString.Open();
                FillDataGrid();
                connectionString.Close();
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (PickupsInfoGrid.SelectedItem == null) //Проверка выбора пункта самовывоза, который будет удален
            {
                MessageBox.Show("Невозможно удалить информацию о пункте самовывоза, так как он не был выбран.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                DataRowView pickupInfo = (DataRowView)PickupsInfoGrid.SelectedItems[0]; //Получание информации о пункте самовывоза из таблицы
                SqlCommand cmd = new SqlCommand();
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = "DELETE Pickup FROM Pickup INNER JOIN PickupTown ON Pickup.pickupTownCode = PickupTown.pickupTownCode " +
                                  "WHERE pickupTownName = @town AND streetName = @street AND building = @building"; //Запрос на удаление выбранного пункта самовывоза
                cmd.Parameters.Add("@street", SqlDbType.VarChar).Value = pickupInfo["streetName"].ToString();
                cmd.Parameters.Add("@town", SqlDbType.VarChar).Value = pickupInfo["pickupTownName"].ToString(); ;
                cmd.Parameters.Add("@building", SqlDbType.VarChar).Value = pickupInfo["building"].ToString();
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