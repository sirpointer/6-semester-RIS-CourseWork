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
using Newtonsoft.Json;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace SeaBattleClient
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class GamePage : Page
    {
        public Player Model
        {
            get
            {
                return this.DataContext as Player;
            }
        }

        Player player = null;

        private Color backColor = Colors.CornflowerBlue;
        public Color killColor = Colors.IndianRed;

        public static EnemyGameField EnemyGameField = new EnemyGameField();
        public static GameField MyGameField;
        
        #region init

        public GamePage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
                player = e.Parameter as Player;

            EnemyGameField.EnemyShot += EnemyGameField_EnemyShot;

            MyGameField = player.GameField;
            MyGameField.ShotMyField += MyGameField_ShotMyField;

            //заполнить поле первого игрока
            CreateField(Player1Grid);
            FillFieldWithRectangle(Player1Grid);

            foreach (ClientShip ship in Model.GameField.Ships)
            {
                SetImage(ship.Source, ship.ShipWidth, ship.ShipHeight, ship.Location.X, ship.Location.Y, Player1Grid);
            }

            //заполнить поле второго игрока
            CreateField(Player2Grid);
            FillFieldWithRectangle(Player2Grid, true);

            if (Model.CanShot == false)
            {
                tbWait.Visibility = Visibility.Visible;

                Socket socket = Model.PlayerSocket;

                byte[] resp = new byte[1024];

                Task.Run(() =>
                {
                    pingDone.Reset();

                    Socket client = socket;

                    StateObject state = new StateObject();
                    state.workSocket = client;
                    state.obj = EnemyGameField;

                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallbackEnemyShot), state);
                });
            } else
            {
                tbWait.Visibility = Visibility.Collapsed;
                tbGo.Visibility = Visibility.Visible;
                progressRing.IsActive = false;

                StateObject state = new StateObject();
                state.workSocket = Model.PlayerSocket;
            }
        }

        #endregion

        /// <summary>
        /// Выстрел по врагу.
        /// </summary>
        private async void Rectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Model.CanShot == false)
            {
                return;
            }

            Rectangle rec = sender as Rectangle;
            int col = Grid.GetColumn(rec);
            int row = Grid.GetRow(rec);
            Location location = new Location(col, row);

            Socket socket = Model.PlayerSocket;

            byte[] resp = new byte[1024];

            await Task.Run(() =>
            {
                pingDone.Reset();

                Socket client = socket;

                StateObject state = new StateObject();
                state.workSocket = client;

                JObject jObject = new JObject();
                jObject.Add(JsonStructInfo.Type, Request.EnumTypeToString(Request.RequestTypes.Shot));
                jObject.Add(JsonStructInfo.Result, Serializer<Location>.Serialize(location));

                string s = jObject.ToString() + JsonStructInfo.EndOfMessage;

                Send(state, s);

                pingDone.WaitOne();
            });
        }

        #region ShotMyField

        /// <summary>
        /// Выстрел по мне.
        /// </summary>
        private async void MyGameField_ShotMyField(object sender, ShotEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Rectangle rectangle = new Rectangle();
                Player1Grid.Children.Add(rectangle);
                Grid.SetColumn(rectangle, e.Hits[0].X);
                Grid.SetRow(rectangle, e.Hits[0].Y);

                if (e.ShotResult == Game.ShotResult.Miss) //промах
                {
                    rectangle.Fill = new SolidColorBrush(killColor);
                    Model.CanShot = true;
                    
                }
                else if (e.ShotResult == Game.ShotResult.Damage) //ранил
                {

                    SetImage("ms-appx:///Assets/Ships/Hurt.png", 1, 1, e.Hits[0].X, e.Hits[0].Y, Player1Grid);
                    Model.CanShot = false;
                }
                else if (e.ShotResult == Game.ShotResult.Kill) //убил
                {
                    Ship s = (Ship)e.Ship.Clone();
                    ClientShip ship = new ClientShip(s.Id, s.ShipClass, s.Orientation, s.Location); // сюда передается кораблик
                    
                    KillShip(ship, Player1Grid);
                    SetImage("ms-appx:///Assets/Ships/Hurt.png", ship.ShipWidth, ship.ShipHeight, ship.Location.X, ship.Location.Y, Player1Grid);
                    Model.CanShot = false;
                }
            });

            if (MyGameField.IsGameOver)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Canvas.SetZIndex(panel, 7);
                    panel.Background = new SolidColorBrush(Colors.White);
                    tbGameResult.Text = "Вы проиграли";
                    btnStartNewGame.Visibility = Visibility.Visible;
                });
            }

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (Model.CanShot == true)
                {
                    tbWait.Visibility = Visibility.Collapsed;
                    tbGo.Visibility = Visibility.Visible;
                    progressRing.IsActive = false;
                }
                else
                {
                    StateObject state = new StateObject();
                    state.workSocket = Model.PlayerSocket;
                    state.obj = MyGameField;

                    Model.PlayerSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallbackEnemyShot), state);
                }
            });
        }

        /// <summary>
        /// Удар по мне.
        /// </summary>
        private static void ReceiveCallbackEnemyShot(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                int bytesRead = client.EndReceive(ar);

                state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));

                if (state.sb.Length > 1)
                {
                    response = state.sb.ToString();
                }

                response = response.Remove(response.LastIndexOf(JsonStructInfo.EndOfMessage));

                JObject jObject = null;

                Answer.AnswerTypes type = Answer.AnswerTypes.ShotOfTheEnemy;

                try
                {
                    jObject = JObject.Parse(response);
                } catch (JsonReaderException e)
                {
                    Console.WriteLine(e);
                }

                type = Answer.JsonTypeToEnum((string)jObject[JsonStructInfo.Type]);
                Location location = Serializer<Location>.Deserialize((string)jObject[JsonStructInfo.Result]);

                MyGameField.Shot(location);
                pingDone.Reset();
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion

        #region ShotEnemyField

        /// <summary>
        /// выстрел по противнику
        /// </summary>
        private async void EnemyGameField_EnemyShot(object sender, ShotEventArgs e)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                Rectangle rectangle = new Rectangle();
                Player2Grid.Children.Add(rectangle);
                Grid.SetColumn(rectangle, e.Hits[0].X);
                Grid.SetRow(rectangle, e.Hits[0].Y);

                if (e.ShotResult == Game.ShotResult.Miss) //промах
                {
                    rectangle.Fill = new SolidColorBrush(killColor);
                    Model.CanShot = false;
                }
                else if (e.ShotResult == Game.ShotResult.Damage) //ранил
                {
                    SetImage("ms-appx:///Assets/Ships/Hurt.png", 1, 1, e.Hits[0].X, e.Hits[0].Y, Player2Grid);
                    Model.CanShot = true;
                }
                else if (e.ShotResult == Game.ShotResult.Kill) //убил
                {
                    Ship s = (Ship)e.Ship.Clone();
                    ClientShip ship = new ClientShip(s.Id, s.ShipClass, s.Orientation, s.Location);

                    KillShip(ship, Player2Grid);
                    SetImage(ship.Source, ship.ShipWidth, ship.ShipHeight, ship.Location.X, ship.Location.Y, Player2Grid);
                    Model.CanShot = true;
                }
            });

            if (EnemyGameField.IsGameOver)
            {
                await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    Canvas.SetZIndex(panel, 7);
                    panel.Background = new SolidColorBrush(Colors.White);
                    tbGameResult.Text = "Вы победили";
                    btnStartNewGame.Visibility = Visibility.Visible;
                });
            }

            await AwaitEnemyView();
        }

        private async Task AwaitEnemyView()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (Model.CanShot == false)
                {
                    tbWait.Visibility = Visibility.Visible;
                    tbGo.Visibility = Visibility.Collapsed;
                    //progressRing.IsActive = true;

                    StateObject state = new StateObject();
                    state.workSocket = Model.PlayerSocket;
                    state.obj = MyGameField;

                    Model.PlayerSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallbackEnemyShot), state);
                }
            });
        }
        
        public static string response = string.Empty;

        /// <summary>
        /// Удар по врагу.
        /// </summary>
        private static void ReceiveCallbackMyShot(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                
                int bytesRead = client.EndReceive(ar);

                state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));

                if (state.sb.Length > 1)
                {
                    response = state.sb.ToString();
                }

                response = response.Remove(response.LastIndexOf(JsonStructInfo.EndOfMessage));

                JObject jObject = null;

                Answer.AnswerTypes type = Answer.AnswerTypes.ShotOfTheEnemy;
                SeaBattleClassLibrary.DataProvider.ShotResult.ShotResultType result = SeaBattleClassLibrary.DataProvider.ShotResult.ShotResultType.Miss;
                Ship ship = null;

                try
                {
                    jObject = JObject.Parse(response);
                }
                catch (JsonReaderException e)
                {
                    Console.WriteLine(e);
                }

                type = Answer.JsonTypeToEnum((string)jObject[JsonStructInfo.Type]);
                result = SeaBattleClassLibrary.DataProvider.ShotResult.JsonTypeToEnum((string)jObject[JsonStructInfo.Result]);
                ship = result == SeaBattleClassLibrary.DataProvider.ShotResult.ShotResultType.Kill ?
                    Serializer<Ship>.Deserialize((string)jObject[JsonStructInfo.AdditionalContent]) : null;
                Location location = Serializer<Location>.Deserialize((string)jObject[JsonStructInfo.Content]);

                Game.ShotResult shotResult = Game.ShotResult.Miss;

                if (result != SeaBattleClassLibrary.DataProvider.ShotResult.ShotResultType.Miss)
                {
                    shotResult = result == SeaBattleClassLibrary.DataProvider.ShotResult.ShotResultType.Damage ? 
                        Game.ShotResult.Damage : Game.ShotResult.Kill;
                }

                EnemyGameField.Shot(location, shotResult, ship);
                pingDone.Reset();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        #endregion

        #region ConnectServer

        /// <summary>
        /// Начать ожидание сообщения от сервера.
        /// </summary>
        private static void Receive(StateObject state)
        {
            try
            {
                Socket client = state.workSocket;

                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallbackMyShot), state);
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

            byte[] byteData = Encoding.UTF8.GetBytes(data);
            
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), state);
        }
        
        private static StateObject stateObject = new StateObject();

        /// <summary>
        /// Сообщение доставлено.
        /// </summary>
        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);
                
                stateObject = state;
                Receive(state);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        /// <summary>
        /// Определение типа ответа от сервера
        /// </summary>
        public static Answer.AnswerTypes GetAnswerType(string jsonRequest)
        {
            JObject jObject;

            try
            {
                jObject = JObject.Parse(jsonRequest);
            } catch (JsonReaderException e)
            {
                Console.WriteLine();
                Console.WriteLine(e);
                Console.WriteLine(jsonRequest);
                Console.WriteLine();
                return Answer.AnswerTypes.Error;
            }

            if (jObject.ContainsKey(JsonStructInfo.Type))
            {
                string requestType = (string)jObject[JsonStructInfo.Type];
                return Answer.JsonTypeToEnum(requestType);
            } else
            {
                return Answer.AnswerTypes.Error;
            }
        }

        #endregion

        #region View

        /// <summary>
        /// Отметить на поле, что корабль убит
        /// </summary>
        /// <param name="ship">Корабль</param>
        /// <param name="grid">Поле игрока</param>
        public void KillShip(Ship ship, Grid grid)
        {
            for (int i = ship.Location.X - 1; i <= ship.Location.X + ship.ShipWidth; i++)
            {
                for (int j = ship.Location.Y - 1; j <= ship.Location.Y + ship.ShipHeight; j++)
                {
                    if (i < 10 && j < 10 && i > -1 && j > -1)
                    {
                        Rectangle rectangle = new Rectangle();
                        grid.Children.Add(rectangle);
                        Grid.SetColumn(rectangle, i);
                        Grid.SetRow(rectangle, j);

                        rectangle.Fill = new SolidColorBrush(killColor);
                    }
                }
            }
        }

        /// <summary>
        /// Установка картинки
        /// </summary>
        public void SetImage(string source, int width, int height, int x, int y, Grid grid)
        {
            Image image = new Image();
            BitmapImage bitmapImage = new BitmapImage();
            var uri = new Uri(source, UriKind.Absolute);
            bitmapImage.UriSource = uri;
            image.Source = bitmapImage;
            image.HorizontalAlignment = HorizontalAlignment.Stretch;
            image.VerticalAlignment = VerticalAlignment.Stretch;
            image.Stretch = Stretch.Fill;
            grid.Children.Add(image);
            var index = (uint)grid.Children.IndexOf(image);
            grid.Children.Move(index, (uint)grid.Children.Count - 1);

            Grid.SetColumnSpan(image, width);
            Grid.SetRowSpan(image, height);
            Grid.SetRow(image, y);
            Grid.SetColumn(image, x);
        }

        /// <summary>
        /// Заполнить поле сеткой квадратов
        /// </summary>
        private void FillFieldWithRectangle(Grid grid, bool mine = false)
        {
            for (int i = 0; i < grid.RowDefinitions.Count; i++)
            {
                for (int j = 0; j < grid.ColumnDefinitions.Count; j++)
                {
                    Rectangle rectangle = new Rectangle() { StrokeThickness = 1 };
                    rectangle.Stroke = new SolidColorBrush(Colors.Black);
                    rectangle.Fill = new SolidColorBrush(backColor);
                    rectangle.Name = i.ToString() + j.ToString();
                    grid.Children.Add(rectangle);
                    rectangle.HorizontalAlignment = HorizontalAlignment.Stretch;
                    rectangle.VerticalAlignment = VerticalAlignment.Stretch;
                    Grid.SetRow(rectangle, i);
                    Grid.SetColumn(rectangle, j);
                    if (mine)
                    {
                        rectangle.Tapped += Rectangle_Tapped;
                        rectangle.PointerEntered += Rectangle_PointerEntered;
                        rectangle.PointerExited += Rectangle_PointerExited;
                    }
                }
            }
        }

        /// <summary>
        /// Наведение курсора на клетку
        /// </summary>
        private void Rectangle_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Rectangle rec = sender as Rectangle;
            rec.Fill = new SolidColorBrush(backColor);
        }

        /// <summary>
        /// Отведение курсора с клетки
        /// </summary>
        private void Rectangle_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (Model.CanShot == false)
                return;

            if (Model.CanShot == true)
            {
                Rectangle rec = sender as Rectangle;
                rec.Fill = new SolidColorBrush(Colors.Red);
            }
        }

        /// <summary>
        /// Создание поля 10х10
        /// </summary>
        private void CreateField(Grid grid)
        {
            for (int i = 0; i < 10; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }
        }

        #endregion


        private void BtnStartNewGame_Click(object sender, RoutedEventArgs e)
        {
            MainPage.MainFrame?.Navigate(typeof(StartPage));
        }
    }
}
