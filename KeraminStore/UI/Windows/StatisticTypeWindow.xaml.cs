using System.IO;
using System.Windows;
using System.Windows.Input;

namespace KeraminStore.UI.Windows
{
    public partial class StatisticTypeWindow : Window
    {
        public StatisticTypeWindow()
        {
            InitializeComponent();
        }

        private void ViewButton_Click(object sender, RoutedEventArgs e)
        {
            int windowCode = 0;
            if (bestEmployees.IsChecked == false && bestProducts.IsChecked == false)
            {
                MessageBox.Show("Вы не выбрали статистику для просмотра.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (bestEmployees.IsChecked == true)
            {
                windowCode = 1;
                StreamWriter windowCodeFile = new StreamWriter("WindowCode.txt");
                windowCodeFile.Write(windowCode.ToString());
                windowCodeFile.Close();
            }
            else if (bestProducts.IsChecked == true)
            {
                windowCode = 2;
                StreamWriter windowCodeFile = new StreamWriter("WindowCode.txt");
                windowCodeFile.Write(windowCode.ToString());
                windowCodeFile.Close();
            }
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => this.DragMove();
    }
}