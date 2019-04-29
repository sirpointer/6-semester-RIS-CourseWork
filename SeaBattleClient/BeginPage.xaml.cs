﻿using System;
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

        List<Image> ShipsImages = new List<Image>();

        public BeginPage()
        {
            this.InitializeComponent();
            DataContext = new GameField();
            Model.Ships[0] = new Ship(0, ShipClass.OneDeck, Game.Orientation.Horizontal);
            Model.Ships[1] = new Ship(1, ShipClass.OneDeck, Game.Orientation.Horizontal);
            Model.Ships[2] = new Ship(2, ShipClass.OneDeck, Game.Orientation.Horizontal);
            Model.Ships[3] = new Ship(3, ShipClass.OneDeck, Game.Orientation.Horizontal);
            Model.Ships[4] = new Ship(4, ShipClass.TwoDeck, Game.Orientation.Horizontal);
            Model.Ships[5] = new Ship(5, ShipClass.TwoDeck, Game.Orientation.Horizontal);
            Model.Ships[6] = new Ship(6, ShipClass.TwoDeck, Game.Orientation.Horizontal);
            Model.Ships[7] = new Ship(7, ShipClass.ThreeDeck, Game.Orientation.Horizontal);
            Model.Ships[8] = new Ship(8, ShipClass.ThreeDeck, Game.Orientation.Horizontal);
            Model.Ships[9] = new Ship(9, ShipClass.FourDeck, Game.Orientation.Horizontal);
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


            //MyFrame.Navigate(typeof(BeginPage), Model);

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
                            double columnDouble = imagePoint.X / (FieldGrid.Width / 10);
                            int column = (int)columnDouble;

                            double rowDouble = (imagePoint.Y + image.ActualHeight / 2) / (FieldGrid.Height / 10);
                            int row = (int)rowDouble;

                            //ship.Location.X = column;
                            //ship.Location.Y = row;

                            bool set = Model.SetShipLocation(ship, new Location(column, row));
                            if (set)
                            {
                                int colEnd = (int)(column + image.ActualWidth / 30 - 1);
                                if (colEnd < 10 && colEnd >= 0 && row >= 0 && row < 10)
                                {
                                    int start = Convert.ToInt32(row.ToString() + column.ToString());
                                    int end = Convert.ToInt32(row.ToString() + colEnd.ToString());
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
                double columnDouble = imagePoint.X / (FieldGrid.Width / 10);
                int column = (int)columnDouble;

                double rowDouble = (imagePoint.Y + image.ActualHeight / 2) / (FieldGrid.Height / 10);
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
                    }
                } else
                { //эта херня полностью дублирует методы при срывании
                    if (ship.Location.X == -1 || ship.Location.Y == -1 || ship.Location.X + ship.ShipWidth >= 10 || ship.Location.Y + ship.ShipHeight >= 10)
                    {
                        Canvas.SetLeft(image, 0);
                        Canvas.SetTop(image, 0);

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
        }

        //поворот по правой кнопке мыши
        //странно возвращает координаты
        //происходит какая-то херня
        private void Image1_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            Image image = sender as Image;
            Ship ship = image.DataContext as Ship;

            if(ship.Orientation == Game.Orientation.Horizontal)
            {
                double x = Canvas.GetTop(image);
                double y = Canvas.GetLeft(image);

                image.RenderTransform = new CompositeTransform { Rotation = 90 };
                ship.Orientation = Game.Orientation.Vertical;
            
                Canvas.SetTop(image, x);
                Canvas.SetLeft(image, y+image.ActualHeight);
            }
        }
    }
}
