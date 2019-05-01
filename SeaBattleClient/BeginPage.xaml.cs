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
using SeaBattleClassLibrary.Game;
using Game = SeaBattleClassLibrary.Game;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace SeaBattleClient
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class BeginPage : Page
    {
        public GameField Model
        {
            get
            {
                return this.DataContext as GameField;
            }
        }

        public Player player = null;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter == null)
                return;
                //player = e.Parameter as Player;

            if (player == null)
            {
                player = e.Parameter as Player;
                
                DataContext = player.GameField;
                
                Image1.DataContext = Model.Ships[0];
                Image2.DataContext = Model.Ships[1];
                Image3.DataContext = Model.Ships[2];
                Image4.DataContext = Model.Ships[3];
                Image5.DataContext = Model.Ships[4];
                Image6.DataContext = Model.Ships[5];
                Image7.DataContext = Model.Ships[6];
                Image8.DataContext = Model.Ships[7];
                Image9.DataContext = Model.Ships[8];
                Image10.DataContext = Model.Ships[9];
                ShipsImages.Add(Image1);
                ShipsImages.Add(Image2);
                ShipsImages.Add(Image3);
                ShipsImages.Add(Image4);
                ShipsImages.Add(Image5);
                ShipsImages.Add(Image6);
                ShipsImages.Add(Image7);
                ShipsImages.Add(Image8);
                ShipsImages.Add(Image9);
                ShipsImages.Add(Image10);
            }
        }

        List<Image> ShipsImages = new List<Image>();

        public BeginPage()
        {
            this.InitializeComponent();

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
                    rectangle.Fill = new SolidColorBrush(Colors.DarkSeaGreen);
                    rectangle.Name = i.ToString() + j.ToString();
                    FieldGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i);
                    Grid.SetColumn(rectangle, j);
                }
            }
        }

        /// <summary>
        /// для определения координат
        /// </summary>
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
                            double columnDouble = (imagePoint.X +FieldGrid.Width/20)/ (FieldGrid.Width / 10);
                            int column = (int)columnDouble;

                            double rowDouble = (imagePoint.Y +FieldGrid.Height/20) / (FieldGrid.Height / 10);
                            int row = (int)rowDouble;

                            //ship.Location.X = column;
                            //ship.Location.Y = row;

                            bool set = Model.SetShipLocation(ship, new Location(column, row, true));
                            if (set)
                            {
                                int colEnd = (int)(column + ship.ShipWidth - 1);
                                if (colEnd < 10 && colEnd >= 0 && row >= 0 && row < 10)
                                {
                                    if(ship.Orientation == Game.Orientation.Horizontal)
                                    {
                                        int start = Convert.ToInt32(row.ToString() + column.ToString());
                                        int end = start + ship.ShipWidth - 1;  // Convert.ToInt32(row.ToString() + colEnd.ToString());
                                        if (start >= 0 && start < FieldGrid.Height && end >= 0 && end < FieldGrid.Width)
                                        {
                                            for (int i = 0; i < 100; i++)
                                            {
                                                Rectangle rect = (Rectangle)FieldGrid.Children[i];
                                                rect.Fill = new SolidColorBrush(Colors.White);
                                            }
                                            for (int i = start; i <= end; i++)
                                            {
                                                Rectangle rect = (Rectangle)FieldGrid.Children[i];
                                                rect.Fill = new SolidColorBrush(Colors.Red);
                                            }
                                        }
                                    } else
                                    {
                                        int start = Convert.ToInt32(row.ToString() + (column).ToString());
                                        int end = start + (ship.ShipHeight-1)*10;  // Convert.ToInt32(row.ToString() + colEnd.ToString());
                                        if (start >= 0 && start < FieldGrid.Height && end >= 0 && end < FieldGrid.Width)
                                        {
                                            for (int i = 0; i < 100; i++)
                                            {
                                                Rectangle rect = (Rectangle)FieldGrid.Children[i];
                                                rect.Fill = new SolidColorBrush(Colors.White);
                                            }
                                            for (int i = start; i <= end; i+=10)
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
            }
        }

        //нажатие
        private void MyImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Image image = sender as Image;
            Canvas.SetZIndex(image, 5);
            Ship ship = image.DataContext as Ship;
            for (int i = 0; i < ShipsImages.Count; i++)
            {
                if (ShipsImages[i].Name != image.Name)
                    ShipsImages[i].IsHitTestVisible = false;
            }

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
                double columnDouble = (imagePoint.X+FieldGrid.Width/20) / (FieldGrid.Width / 10);
                int column = (int)columnDouble;

                double rowDouble = (imagePoint.Y+FieldGrid.Height/20) / (FieldGrid.Height / 10);
                int row = (int)rowDouble;

                bool set = Model.SetShipLocation(ship, new Location(column, row));
                if (set)
                {
                    //int r = Convert.ToInt32(row.ToString() + column.ToString());
                    //var rect = FieldGrid.Children[r];

                    double X = Canvas.GetLeft(FieldGrid) + (FieldGrid.Width / 10) * column;
                    double Y = Canvas.GetTop(FieldGrid) + (FieldGrid.Height / 10) * row;

                    //проверяет, не выходит ли картинка за края
                    bool x = X <= FieldGrid.Width - (image.ActualWidth / 3 * 2 + 1) + Canvas.GetLeft(FieldGrid);
                    bool y = Y <= FieldGrid.Height - (image.ActualHeight / 3 * 2 + 1) + Canvas.GetTop(FieldGrid);
                    if (x && y)
                    {
                        Canvas.SetLeft(image, X);
                        Canvas.SetTop(image, Y);

                        //ship.Location.X = column;
                        //ship.Location.Y = row;

                    } else
                    {
                        Canvas.SetLeft(image, 0);
                        Canvas.SetTop(image, 0);
                        ship.Location = new Location();
                    }
                } else
                { //эта херня полностью дублирует методы при срывании
                    if (ship.Location.X == -1 || ship.Location.Y == -1 || ship.Location.X + ship.ShipWidth >= 10 || ship.Location.Y + ship.ShipHeight >= 10)
                    {
                        Canvas.SetLeft(image, 0);
                        Canvas.SetTop(image, 0);
                        ship.Location = new Location();

                        for (int i = 0; i < 100; i++)
                        {
                            Rectangle rect = (Rectangle)FieldGrid.Children[i];
                            rect.Fill = new SolidColorBrush(Colors.White);
                        }
                    } else
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

                for (int i = 0; i < ShipsImages.Count; i++)
                {
                    ShipsImages[i].IsHitTestVisible = true;
                }

                Canvas.SetZIndex(image, 2);



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
            } else
            {
                Canvas.SetLeft(image, 0);
                Canvas.SetTop(image, 0);
                ship.Location = new Location();
            }
            //при отпускании снимает подсветку с поля
            for (int i = 0; i < 100; i++)
            {
                Rectangle rect = (Rectangle)FieldGrid.Children[i];
                rect.Fill = new SolidColorBrush(Colors.White);
            }
        }

        //при срывнии мыши
        //если пронести над начальной картинкой, скинет ее в 0
        private void MyImage_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Image image = sender as Image;
            Ship ship = image.DataContext as Ship;

            if (Canvas.GetTop(image)==0)
            {
                
            }
            else if (ship.Location.X == -1 || ship.Location.Y == -1 || ship.Location.X + ship.ShipWidth > 10 || ship.Location.Y + ship.ShipHeight > 10)
            {
                Canvas.SetLeft(image, 0);
                Canvas.SetTop(image, 0);
                ship.Location = new Location();

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

            for (int i = 0; i < ShipsImages.Count; i++)
            {
                ShipsImages[i].IsHitTestVisible = true;
            }

            Canvas.SetZIndex(image, 2);
        }

        //поворот по правой кнопке мыши
        //странно возвращает координаты
        //происходит какая-то херня
        private void Image1_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Image image = sender as Image;
            Ship ship = image.DataContext as Ship;
            
            double x = Canvas.GetTop(image);
            double y = Canvas.GetLeft(image);

            if (ship.Orientation == Game.Orientation.Horizontal)
            {
                switch (ship.ShipClass)
                {
                case ShipClass.OneDeck:
                    image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Ships/5.jpg", UriKind.Absolute));
                    image.Height = 30;
                    image.Width = 30;
                    break;
                case ShipClass.TwoDeck:
                    image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Ships/6.jpg", UriKind.Absolute));
                    image.Height = 60;
                    image.Width = 30;
                    break;
                case ShipClass.ThreeDeck:
                    image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Ships/7.jpg", UriKind.Absolute));
                    image.Height = 90;
                    image.Width = 30;
                    break;
                case ShipClass.FourDeck:
                    image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Ships/8.jpg", UriKind.Absolute));
                    image.Height = 120;
                    image.Width = 30;
                    break;
                }
                
                ship.Orientation = Game.Orientation.Vertical;

                bool set = Model.SetShipLocation(ship, new Location(ship.Location.X, ship.Location.Y, true));
                if (!set)
                {
                    Canvas.SetLeft(image, 0);
                    Canvas.SetTop(image, 0);
                    ship.Location = new Location();
                }

                Canvas.SetTop(image, y);
                Canvas.SetLeft(image, x+image.ActualWidth);
            } else
            {
                switch (ship.ShipClass)
                {
                case ShipClass.OneDeck:
                    image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Ships/1.jpg", UriKind.Absolute));
                    image.Height = 30;
                    image.Width = 30;
                    break;
                case ShipClass.TwoDeck:
                    image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Ships/2.jpg", UriKind.Absolute));
                    image.Height = 30;
                    image.Width = 60;
                    break;
                case ShipClass.ThreeDeck:
                    image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Ships/3.jpg", UriKind.Absolute));
                    image.Height = 30;
                    image.Width = 90;
                    break;
                case ShipClass.FourDeck:
                    image.Source = new BitmapImage(new Uri("ms-appx:///Assets/Ships/4.jpg", UriKind.Absolute));
                    image.Height = 30;
                    image.Width = 120;
                    break;
                }

                ship.Orientation = Game.Orientation.Horizontal;

                bool set = Model.SetShipLocation(ship, new Location(ship.Location.X, ship.Location.Y, true));
                if (!set)
                {
                    Canvas.SetLeft(image, 0);
                    Canvas.SetTop(image, 0);
                    ship.Location = new Location();
                }

                Canvas.SetTop(image, y);
                Canvas.SetLeft(image, x + image.ActualWidth);
            }


        }
    }
}
