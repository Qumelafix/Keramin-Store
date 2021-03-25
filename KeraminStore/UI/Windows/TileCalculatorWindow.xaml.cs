using System;
using System.Windows;
using System.Windows.Controls;

namespace KeraminStore.UI.Windows
{
    public partial class TileCalculatorWindow : UserControl
    {
        public TileCalculatorWindow()
        {
            InitializeComponent();
        }

        private void CalculateButton_Click(object sender, RoutedEventArgs e)
        {
            if (room.IsChecked == false && floor.IsChecked == false)
            {
                MessageBox.Show("Вы не указали место, где будет класться плитка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else if (floor.IsChecked == true) surfaceHeight.Text = "1";

            if (surfaceWidth.Text == string.Empty)
            {
                MessageBox.Show("Вы не указали ширину комнаты/пола.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                char[] surfaceWidthArray = surfaceWidth.Text.ToCharArray();
                for (int i = 0; i < surfaceWidthArray.Length; i++)
                {
                    if (!char.IsDigit(surfaceWidthArray[i]))
                    {
                        MessageBox.Show("Вы указали неверные символы в ширине комнаты/пола.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                if (int.Parse(surfaceWidth.Text) < 0)
                {
                    MessageBox.Show("Ширина комнаты/пола не может быть отрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (int.Parse(surfaceWidth.Text) > 10)
                {
                    MessageBox.Show("Ширина комнаты/пола не может быть более 10 м.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (surfaceLenght.Text == string.Empty)
            {
                MessageBox.Show("Вы не указали длину комнаты/пола.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                char[] surfaceLenghtArray = surfaceLenght.Text.ToCharArray();
                for (int i = 0; i < surfaceLenghtArray.Length; i++)
                {
                    if (!char.IsDigit(surfaceLenghtArray[i]))
                    {
                        MessageBox.Show("Вы указали неверные символы в длине комнаты/пола.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                if (int.Parse(surfaceLenght.Text) < 0)
                {
                    MessageBox.Show("Длина комнаты/пола не может быть отрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (int.Parse(surfaceLenght.Text) > 10)
                {
                    MessageBox.Show("Длина комнаты/пола не может быть более 10 м.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (room.IsChecked == true)
            {
                if (surfaceHeight.Text == string.Empty)
                {
                    MessageBox.Show("Вы не указали высоту комнаты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else
                {
                    char[] surfaceHeightArray = surfaceHeight.Text.ToCharArray();
                    for (int i = 0; i < surfaceHeightArray.Length; i++)
                    {
                        if (!char.IsDigit(surfaceHeightArray[i]))
                        {
                            MessageBox.Show("Вы указали неверные символы в высоте комнаты.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }
                    if (int.Parse(surfaceHeight.Text) < 0)
                    {
                        MessageBox.Show("Высота комнаты не может быть отрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                    if (int.Parse(surfaceHeight.Text) > 10)
                    {
                        MessageBox.Show("Высота комнаты не может быть более 10 м.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }

            if (tileWidth.Text == string.Empty)
            {
                MessageBox.Show("Вы не указали ширину плитки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                char[] tileWidthArray = tileWidth.Text.ToCharArray();
                for (int i = 0; i < tileWidthArray.Length; i++)
                {
                    if (!char.IsDigit(tileWidthArray[i]))
                    {
                        MessageBox.Show("Вы указали неверные символы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                if (int.Parse(tileWidth.Text) < 0)
                {
                    MessageBox.Show("Ширина плитки не может быть отрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (int.Parse(tileWidth.Text) > 600)
                {
                    MessageBox.Show("Ширина плитки не может быть более 600 мм.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (tileLenght.Text == string.Empty)
            {
                MessageBox.Show("Вы не указали длину плитки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                char[] tileLenghtArray = tileLenght.Text.ToCharArray();
                for (int i = 0; i < tileLenghtArray.Length; i++)
                {
                    if (!char.IsDigit(tileLenghtArray[i]))
                    {
                        MessageBox.Show("Вы указали неверные символы в длине плитки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                if (int.Parse(tileLenght.Text) < 0)
                {
                    MessageBox.Show("Длина плитки не может быть отрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (int.Parse(tileLenght.Text) > 600)
                {
                    MessageBox.Show("Длина плитки не может быть более 600 мм.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (tileReserve.Text == string.Empty)
            {
                MessageBox.Show("Вы не указали запас плитки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                char[] tileReserveArray = tileReserve.Text.ToCharArray();
                for (int i = 0; i < tileReserveArray.Length; i++)
                {
                    if (!char.IsDigit(tileReserveArray[i]))
                    {
                        MessageBox.Show("Вы указали неверные символы в запасе плитки.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                if (int.Parse(tileReserve.Text) < 0)
                {
                    MessageBox.Show("Запас плитки не может быть отрицательной величиной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                if (int.Parse(tileReserve.Text) > 100)
                {
                    MessageBox.Show("Вы не можете указать более 100% запаса.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            //if (areaWidth.Text == string.Empty)
            //{
            //    MessageBox.Show("Вы не указали ширину незакладываемого участка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
            //else
            //{
            //    char[] areaWidthArray = areaWidth.Text.ToCharArray();
            //    for (int i = 0; i < areaWidthArray.Length; i++)
            //    {
            //        if (!char.IsDigit(areaWidthArray[i]))
            //        {
            //            MessageBox.Show("Вы указали неверные символы в ширине незакладываемого участка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //            return;
            //        }
            //    }
            //    if (int.Parse(areaWidth.Text) < 0)
            //    {
            //        MessageBox.Show("Ширина незакладываемого участка не может быть отрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //        return;
            //    }
            //    if (int.Parse(areaWidth.Text) > int.Parse(surfaceWidth.Text))
            //    {
            //        MessageBox.Show("Ширина незакладываемого участка не может быть больше ширины комнаты/пола.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //        return;
            //    }
            //}

            //if (areaLenght.Text == string.Empty)
            //{
            //    MessageBox.Show("Вы не указали длину незакладываемого участка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //    return;
            //}
            //else
            //{
            //    char[] areaLenghtArray = areaLenght.Text.ToCharArray();
            //    for (int i = 0; i < areaLenghtArray.Length; i++)
            //    {
            //        if (!char.IsDigit(areaLenghtArray[i]))
            //        {
            //            MessageBox.Show("Вы указали неверные символы в длине незакладываемого участка.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //            return;
            //        }
            //    }
            //    if (int.Parse(areaLenght.Text) < 0)
            //    {
            //        MessageBox.Show("Длина незакладываемого участка не может быть отрицательной.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //        return;
            //    }
            //    if (int.Parse(areaLenght.Text) > int.Parse(surfaceLenght.Text))
            //    {
            //        MessageBox.Show("Длина незакладываемого участка не может быть больше длины комнаты/пола.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            //        return;
            //    }
            //}

            if (room.IsChecked == true)
            {
                double roomSquare = (int.Parse(surfaceLenght.Text) + int.Parse(surfaceWidth.Text)) * 2 * int.Parse(surfaceHeight.Text);
                //double areaSquare = int.Parse(areaLenght.Text) * int.Parse(areaWidth.Text);
                //roomSquare = roomSquare - areaSquare;
                double tileArea = (double.Parse(tileLenght.Text) / 1000) * (double.Parse(tileWidth.Text) / 1000);
                tileSquare.Text = roomSquare.ToString();
                double tilesCount = roomSquare / tileArea;
                tilesCount += tilesCount + int.Parse(tileReserve.Text);
                tileCount.Text = ((int)tilesCount).ToString();
            }
        }

        private void room_Click(object sender, RoutedEventArgs e)
        {
            surfaceHeight.Visibility = Visibility.Visible;
        }

        private void floor_Click(object sender, RoutedEventArgs e)
        {
            surfaceHeight.Visibility = Visibility.Hidden;
        }
    }
}
