﻿using System;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using SeaBattleClassLibrary.Game;
using Game = SeaBattleClassLibrary.Game;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;
using static SeaBattleClient.JoinGamePage;
using Newtonsoft.Json.Linq;
using System.Threading;
using SeaBattleClassLibrary.DataProvider;
using System.Text;

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
                return Player?.GameField;
            }
        }

        public Player Player
        {
            get
            {
                return this.DataContext as Player;
            }
            set
            {
                DataContext = value;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter == null)
                return;

            if (Player != null)
            {
                Player = e.Parameter as Player;
                
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
        Color backColor = Colors.DarkSeaGreen;
        Color lightColor = Colors.Goldenrod;

        public BeginPage()
        {
            this.InitializeComponent();

            //сделать поле
            CreateField();

            //заполнить поле квадратиками
            FillFieldWithRectangle();
        }

        #region Field

        private void FillFieldWithRectangle()
        {
            for (int i = 0; i < FieldGrid.RowDefinitions.Count; i++)
            {
                for (int j = 0; j < FieldGrid.ColumnDefinitions.Count; j++)
                {
                    Rectangle rectangle = new Rectangle() { StrokeThickness = 1 };
                    rectangle.Stroke = new SolidColorBrush(Colors.Black);
                    rectangle.Fill = new SolidColorBrush(backColor);
                    rectangle.Name = i.ToString() + j.ToString();
                    FieldGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i);
                    Grid.SetColumn(rectangle, j);
                }
            }
        }

        private void CreateField()
        {
            for (int i = 0; i < 10; i++)
            {
                FieldGrid.RowDefinitions.Add(new RowDefinition());
                FieldGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }
        }

        /// <summary>
        /// Убирание подсветки
        /// </summary>
        private void FillBackField()
        {
            for (int i = 0; i < 100; i++)
            {
                Rectangle rect = (Rectangle)FieldGrid.Children[i];
                rect.Fill = new SolidColorBrush(backColor);
            }
        }

        #endregion

        #region Events

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
                            //подсветка
                            double columnDouble = (imagePoint.X +FieldGrid.Width/20)/ (FieldGrid.Width / 10);
                            int column = (int)columnDouble;

                            double rowDouble = (imagePoint.Y +FieldGrid.Height/20) / (FieldGrid.Height / 10);
                            int row = (int)rowDouble;

                            bool set = Model.SetShipLocation(ship, new Location(column, row, true));
                            if (set)
                            {
                                int colEnd = (int)(column + ship.ShipWidth - 1);
                                if (colEnd < 10 && colEnd >= 0 && row >= 0 && row < 10)
                                {
                                    if(ship.Orientation == Game.Orientation.Horizontal)
                                    {
                                        int start = Convert.ToInt32(row.ToString() + column.ToString());
                                        int end = start + ship.ShipWidth - 1;
                                        if (start >= 0 && start < FieldGrid.Height && end >= 0 && end < FieldGrid.Width)
                                        {
                                            FillBackField();
                                            for (int i = start; i <= end; i++)
                                            {
                                                Rectangle rect = (Rectangle)FieldGrid.Children[i];
                                                rect.Fill = new SolidColorBrush(lightColor);
                                            }
                                        }
                                    } else
                                    {
                                        int start = Convert.ToInt32(row.ToString() + (column).ToString());
                                        int end = start + (ship.ShipHeight-1)*10;
                                        if (start >= 0 && start < FieldGrid.Height && end >= 0 && end < FieldGrid.Width)
                                        {
                                            FillBackField();

                                            for (int i = start; i <= end; i+=10)
                                            {
                                                Rectangle rect = (Rectangle)FieldGrid.Children[i];
                                                rect.Fill = new SolidColorBrush(lightColor);
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
                    double X = Canvas.GetLeft(FieldGrid) + (FieldGrid.Width / 10) * column;
                    double Y = Canvas.GetTop(FieldGrid) + (FieldGrid.Height / 10) * row;

                    //проверяет, не выходит ли картинка за края
                    bool x = X <= FieldGrid.Width - (image.ActualWidth / 3 * 2 + 1) + Canvas.GetLeft(FieldGrid);
                    bool y = Y <= FieldGrid.Height - (image.ActualHeight / 3 * 2 + 1) + Canvas.GetTop(FieldGrid);
                    if (x && y)
                    {
                        Canvas.SetLeft(image, X);
                        Canvas.SetTop(image, Y);
                        Canvas.SetZIndex(image, 4);

                    } else
                    {
                        Canvas.SetLeft(image, 0);
                        Canvas.SetTop(image, 0);
                        ship.Location = new Location();
                    }
                } 
                else
                {
                    if (ship.Location.X == -1 || ship.Location.Y == -1 || ship.Location.X + ship.ShipWidth >= 10 || ship.Location.Y + ship.ShipHeight >= 10)
                    {
                        Canvas.SetLeft(image, 0);
                        Canvas.SetTop(image, 0);
                        ship.Location = new Location();

                        FillBackField();
                    } else
                    {
                        double X = (ship.Location.X) * (FieldGrid.Width / 10) + Canvas.GetLeft(FieldGrid);
                        double Y = (ship.Location.Y) * (FieldGrid.Height / 10) + Canvas.GetTop(FieldGrid);

                        Canvas.SetLeft(image, X);
                        Canvas.SetTop(image, Y);

                        FillBackField();
                    }
                }

                for (int i = 0; i < ShipsImages.Count; i++)
                {
                    ShipsImages[i].IsHitTestVisible = true;
                }

                Canvas.SetZIndex(image, 2);
            } else
            {
                Canvas.SetLeft(image, 0);
                Canvas.SetTop(image, 0);
                ship.Location = new Location();
            }

            FillBackField();
        }

        //при срывнии мыши
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
                Canvas.SetZIndex(image, 3);

                FillBackField();
            } 
            else
            {
                double X = (ship.Location.X) * (FieldGrid.Width / 10) + Canvas.GetLeft(FieldGrid);
                double Y = (ship.Location.Y) * (FieldGrid.Height / 10) + Canvas.GetTop(FieldGrid);

                Canvas.SetLeft(image, X);
                Canvas.SetTop(image, Y);
                FillBackField();
            }

            for (int i = 0; i < ShipsImages.Count; i++)
            {
                ShipsImages[i].IsHitTestVisible = true;
            }

            Canvas.SetZIndex(image, 2);
        }
        
        /// <summary>
        /// Переворачивание картинки
        /// </summary>
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

        #endregion

        #region ConnectServer

        public static ManualResetEvent pingDone = new ManualResetEvent(false);

        private static String response = String.Empty;
        private static StateObject stateObject = new StateObject();

        /// <summary>
        /// Начало игры
        /// </summary>
        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            foreach (var ship in Model.Ships)
            {
                if (!ship.IsSet)
                    return;
            }

            progressRing.IsActive = true;
            btnStartGame.IsEnabled = false;
            FieldGrid.IsTapEnabled = false;
            IPEndPoint remoteEP = Player.IPEndPoint;

            List<Ship> ships = new List<Ship>(10);
            foreach (ClientShip ship in Model.Ships)
                ships.Add(ship.Clone() as Ship);

            string fieldGame = Serializer<List<Ship>>.Serialize(ships);
            Socket socket = Player.PlayerSocket;

            await Task.Run(() =>
            {
                pingDone.Reset();

                // Create a TCP/IP socket.  
                Socket client = socket;

                StateObject state = new StateObject();
                state.workSocket = client;
                state.obj = fieldGame;

                JObject jObject = new JObject();
                jObject.Add(JsonStructInfo.Type, Request.EnumTypeToString(Request.RequestTypes.SetField));
                jObject.Add(JsonStructInfo.Result, state.obj.ToString());

                string s = jObject.ToString() + JsonStructInfo.EndOfMessage;
                
                Send(state, s);
                
                pingDone.WaitOne();
            });

            WaitOtherPlayer(socket);
            progressRing.IsActive = false;
            btnStartGame.IsEnabled = true;
            FieldGrid.IsTapEnabled = true;
        }

        private void WaitOtherPlayer(Socket socket)
        {
            response = response.Remove(response.LastIndexOf(JsonStructInfo.EndOfMessage));
            Answer.AnswerTypes dataType = GetAnswerType(response);
            StateObject so = new StateObject();
            so.workSocket = socket;

            if (dataType == Answer.AnswerTypes.GameReady)
            {
                Answer.AnswerTypes result = Answer.JsonTypeToEnum(GetJsonRequestResult(response));
                Player.CanShot = result == Answer.AnswerTypes.Yes ? true : false;
                MainPage.MainFrame?.Navigate(typeof(GamePage), Player);
                return;
            } 
            else
            {
                pingDone.Reset();
                Receive(so);
                pingDone.WaitOne();
                WaitOtherPlayer(socket);
                return;
            }
        }

        /// <summary>
        /// попытка соединения
        /// </summary>
        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                
                Socket client = state.workSocket;

                // Complete the connection.  
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}", client.RemoteEndPoint.ToString());


                JObject jObject = new JObject();
                jObject.Add(JsonStructInfo.Type, Request.EnumTypeToString(Request.RequestTypes.SetField));
                jObject.Add(JsonStructInfo.Result, state.obj.ToString());

                string s = jObject.ToString() + JsonStructInfo.EndOfMessage;

                // Send test data to the remote device.  
                Send(state, s);
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Отправка сообщения на сервер.
        /// </summary>
        private static void Send(StateObject state, String data)
        {
            Socket client = state.workSocket;
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), state);
        }

        /// <summary>
        /// Сообщение доставлено.
        /// </summary>
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Complete sending the data to the remote device.  
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.  
                stateObject = state;
                Receive(state);

            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Начать ожидание сообщения от сервера.
        /// </summary>
        private static void Receive(StateObject state)
        {
            try
            {
                Socket client = state.workSocket;
                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Получение ответа от сервера
        /// </summary>
        private static void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);
                
                state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                // All the data has arrived; put it in response.  
                if (state.sb.Length > 1)
                {
                    response = state.sb.ToString();
                }

                pingDone.Set();
            } 
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
        #endregion
    }
}
