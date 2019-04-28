using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using SeaBattleClient.ViewModels;
using SeaBattleClassLibrary.Game;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace SeaBattleClient
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public GameField Model = new GameField();

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = Model;
            myImage.DataContext = Model.Ships[8];
            MyFrame.Navigate(typeof(BeginPage), Model);

            //сделать поле
            for (int i = 0; i < 10; i++)
            {
                FieldGrid.RowDefinitions.Add(new RowDefinition());
                FieldGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            //заполнить поле квадратиками
            for (int i = 0; i < FieldGrid.RowDefinitions.Count; i++)
            {
                for (int j = 0; j < FieldGrid.ColumnDefinitions.Count; j++)
                {
                    Rectangle rectangle = new Rectangle() { StrokeThickness = 1 };
                    rectangle.Stroke = new SolidColorBrush(Colors.Black);
                    rectangle.Fill = new SolidColorBrush(Colors.White);
                    rectangle.Name = i.ToString() + j.ToString();
                    FieldGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i);
                    Grid.SetColumn(rectangle, j);
                }
            }
        }

        private Point shiftPoint = new Point(double.NaN, double.NaN);

        //изменение координаты мыши
        private void MyImage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Image image = sender as Image;
            Ship ship = image.DataContext as Ship;
            Pointer prt = e.Pointer;
            
            //передвижение картинки
            if (prt.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                PointerPoint ptrPt = e.GetCurrentPoint(canvas);

                PointerPoint pntrPt = e.GetCurrentPoint(FieldGrid);
                PointerPoint imagePtrPt = e.GetCurrentPoint(sender as Image);
                Point imagePoint = new Point(pntrPt.Position.X - imagePtrPt.Position.X, pntrPt.Position.Y - imagePtrPt.Position.Y);

                if (ptrPt.Properties.IsLeftButtonPressed)
                {
                    if (!double.IsNaN(shiftPoint.X))
                    {
                        Point newPoint = new Point(ptrPt.Position.X - shiftPoint.X, ptrPt.Position.Y - shiftPoint.Y);

                        Canvas.SetLeft(image, newPoint.X);
                        Canvas.SetTop(image, newPoint.Y);

                        bool inside = imagePoint.X >= 0 && imagePoint.Y >= 0 && imagePoint.X <= FieldGrid.Width && imagePoint.Y <= FieldGrid.Height;

                        //можно расположить только внутри поля
                        if (inside)
                        {
                            //жалкие попытки подсветки
                            //подходит только для горизонтальных корабликов, для вертикальных не прокатит
                            //не закрашивается правая (9) клетка
                            double columnDouble = imagePoint.X / (FieldGrid.Width / 10);
                            int column = (int)columnDouble;

                            double rowDouble = (imagePoint.Y + image.Height / 2) / (FieldGrid.Height / 10);
                            int row = (int)rowDouble;

                            ship.Location.X = column;
                            ship.Location.Y = row;

                            bool set = Model.SetShipLocation(ship, new Location(column, row));
                            
                            int colEnd = (int)(column + image.Width / 30 - 1);
                            if(colEnd<10 && colEnd >= 0 && row >= 0 && row <10)
                            {
                                int start = Convert.ToInt32(row.ToString() + column.ToString());
                                int end = Convert.ToInt32(row.ToString() + colEnd.ToString());
                                if(start>=0 && start<FieldGrid.Height && end>=0 && end < FieldGrid.Width)
                                {
                                    for(int i = 0; i < 100; i++)
                                    {
                                       Rectangle rect = (Rectangle)FieldGrid.Children[i];
                                        rect.Fill = new SolidColorBrush(Colors.White);
                                    }
                                    for (int i=start; i<=end; i++)
                                    {
                                        Rectangle rect = (Rectangle)FieldGrid.Children[i];
                                        rect.Fill = new SolidColorBrush(Colors.Red);
                                    }
                                }
                            }   
                        }
                    }
                }
            }
        }


        private void MyImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Image image = sender as Image;
            Ship ship = image.DataContext as Ship;

            Pointer ptr = e.Pointer;
            PointerPoint ptrPt = e.GetCurrentPoint(canvas);

            if (ptrPt.Properties.IsLeftButtonPressed)
            {
                shiftPoint = new Point(ptrPt.Position.X - Canvas.GetLeft(image), ptrPt.Position.Y - Canvas.GetTop(image));
            }
        }

        //отпускание
        private void MyImage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Image image = sender as Image;
            Ship ship = image.DataContext as Ship;

            shiftPoint = new Point(double.NaN, double.NaN);
            
            PointerPoint ptrPt = e.GetCurrentPoint(FieldGrid);
            PointerPoint imagePtrPt = e.GetCurrentPoint(sender as Image);
            Point imagePoint = new Point(ptrPt.Position.X - imagePtrPt.Position.X, ptrPt.Position.Y - imagePtrPt.Position.Y);

            bool inside = imagePoint.X > 0 && imagePoint.Y > 0 && imagePoint.X < FieldGrid.Width && imagePoint.Y < FieldGrid.Height;

            //можно расположить только внутри поля
            if (inside)
            {
                double columnDouble = imagePoint.X / (FieldGrid.Width / 10);
                int column = (int)columnDouble;

                double rowDouble = (imagePoint.Y+image.Height/2) / (FieldGrid.Height / 10);
                int row = (int)rowDouble;

                //int r = Convert.ToInt32(row.ToString() + column.ToString());
                //var rect = FieldGrid.Children[r];

                double X = Canvas.GetLeft(FieldGrid) + FieldGrid.Width/10 * column;
                double Y = Canvas.GetTop(FieldGrid) + FieldGrid.Height/10 * row;

                //проверяет, не выходит ли картинка за края
                bool x = X <= FieldGrid.Width - (image.Width/3*2+1) + Canvas.GetLeft(FieldGrid);
                bool y = Y <= FieldGrid.Height - (image.Height/3*2+1) + Canvas.GetTop(FieldGrid);
                if (x && y)
                {
                    Row.Text = row.ToString();
                    Column.Text = column.ToString();

                    Canvas.SetLeft(image, X);
                    Canvas.SetTop(image, Y);

                    ship.Location.X = column;
                    ship.Location.Y = row;

                } 
                else
                {
                    Canvas.SetLeft(image, 0);
                    Canvas.SetTop(image, 0);
                }

                

                /*Rectangle w = new Rectangle
                {
                    Height = 30,
                    Width = 30,
                    Fill = new SolidColorBrush(Colors.Red)
                };
                canvas.Children.Add(w);
                Canvas.SetZIndex(w, 2);
                Canvas.SetLeft(w, X);
                Canvas.SetTop(w, Y); */

                //double b = Canvas.GetLeft(FieldGrid);

                //добавление картинки на грид намертво
                /*Image image = new Image();
                BitmapImage bitmapImage = new BitmapImage();
                var uri = new Uri("ms-appx:///Assets/Ships/ship.jpg", UriKind.Absolute);
                bitmapImage.UriSource = uri;
                image.Source = bitmapImage;
                image.Width = 90;
                image.Height = 30;
                FieldGrid.Children.Add(image);


                Grid.SetColumnSpan(image, 3);
                Grid.SetRow(image, row);
                Grid.SetColumn(image, column);*/
            }
            else
            {
                Canvas.SetLeft(image, 0);
                Canvas.SetTop(image, 0);
            }
            //при отпускании снимает подсветку с поля
            for (int i = 0; i < 100; i++)
            {
               Rectangle rect = (Rectangle)FieldGrid.Children[i];
                rect.Fill = new SolidColorBrush(Colors.White);
            }
        }

        //при срывнии мыши
        //изменить потом цифры на Size
        private void MyImage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Image image = sender as Image;
            Ship ship = image.DataContext as Ship;


            if(ship.Location.X == -1 || ship.Location.Y == -1 || ship.Location.X+3 >= 10 || ship.Location.Y+1 >= 10)
            {
                Canvas.SetLeft(image, 0);
                Canvas.SetTop(image, 0);

                for (int i = 0; i < 100; i++)
                {
                    Rectangle rect = (Rectangle)FieldGrid.Children[i];
                    rect.Fill = new SolidColorBrush(Colors.White);
                }
            }
            else
            {
                double X = (ship.Location.X) * (FieldGrid.Width / 10) + Canvas.GetLeft(FieldGrid);
                double Y = (ship.Location.Y) * (FieldGrid.Height / 10) + Canvas.GetTop(FieldGrid);

                Canvas.SetLeft(image, X);
                Canvas.SetTop(image, Y);
                for (int i = 0; i < 100; i++)
                {
                    Rectangle rect = (Rectangle)FieldGrid.Children[i];
                    rect.Fill = new SolidColorBrush(Colors.White);
                }
            }

        }
    }
}
