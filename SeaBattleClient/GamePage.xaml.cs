using System;
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
        private Color backColor = Colors.White;

        public static EnemyGameField EnemyGameField = new EnemyGameField();
        public static GameField MyGameField;

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
                SetImage(ship.Source, (int)ship.ShipClass, ship.Location.X, ship.Location.Y, Player1Grid);
            }

            //заполнить поле второго игрока
            CreateField(Player2Grid);
            FillFieldWithRectangle(Player2Grid, true);

            if (Model.CanShot == false)
            {
                tbWait.Visibility = Visibility.Visible;

                Socket socket = Model.PlayerSocket;

                byte[] resp = new byte[1024];

                //добавить надпись ждите хода

                Task.Run(() =>
                {
                    pingDone.Reset();

                    // Create a TCP/IP socket.  
                    Socket client = socket;

                    StateObject state = new StateObject();
                    state.workSocket = client;
                    state.obj = EnemyGameField;

                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallbackEnemyShot), state);
                });
            } 
            else
            {
                tbWait.Visibility = Visibility.Collapsed;
                tbGo.Visibility = Visibility.Visible;

                StateObject state = new StateObject();
                state.workSocket = Model.PlayerSocket;

                Model.PlayerSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallbackMyShot), state);
            }
        }

        //выстрел по мне
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
                    rectangle.Fill = new SolidColorBrush(Colors.Black);
                    Model.CanShot = true;
                    
                }
                if (e.ShotResult == Game.ShotResult.Damage) //ранил
                {

                    SetImage("ms-appx:///Assets/Ships/hurt.jpg", 1, e.Hits[0].X, e.Hits[0].Y, Player1Grid);
                    Model.CanShot = false;
                }
                if (e.ShotResult == Game.ShotResult.Kill) //убил
                {
                    //Location loc = new Location(1, 1);
                    Ship s = (Ship)e.Ship.Clone();//new Ship(100, ShipClass.TwoDeck, Game.Orientation.Horizontal, loc);
                    ClientShip ship = new ClientShip(s.Id, s.ShipClass, s.Orientation, s.Location); // сюда передается кораблик
                    
                    KillShip(ship);
                    SetImage("ms-appx:///Assets/Ships/hurt.jpg", (int)ship.ShipClass, ship.Location.X, ship.Location.Y, Player1Grid);
                    Model.CanShot = false;
                }
                
            });

            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (Model.CanShot == true)
                {
                    tbWait.Visibility = Visibility.Collapsed;
                    tbGo.Visibility = Visibility.Visible;
                } 
                else
                {
                    StateObject state = new StateObject();
                    state.workSocket = Model.PlayerSocket;
                    state.obj = EnemyGameField;

                    Model.PlayerSocket.BeginReceive(state.buffer, 0, StateObject.BufferSize, SocketFlags.None, new AsyncCallback(ReceiveCallbackEnemyShot), state);

                }

                //Model.CanShot = true;
            });
        }

        //выстрел по противнику
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
                    rectangle.Fill = new SolidColorBrush(Colors.Black);
                    Model.CanShot = false;
                }
                if (e.ShotResult == Game.ShotResult.Damage) //ранил
                {

                    SetImage("ms-appx:///Assets/Ships/hurt.jpg", 1, e.Hits[0].X, e.Hits[0].Y, Player2Grid);
                    Model.CanShot = true;



                }
                if (e.ShotResult == Game.ShotResult.Kill) //убил
                {
                    //Location loc = new Location(1, 1);
                    Ship s = (Ship)e.Ship.Clone();//new Ship(100, ShipClass.TwoDeck, Game.Orientation.Horizontal, loc);
                    ClientShip ship = new ClientShip(s.Id, s.ShipClass, s.Orientation, s.Location); // сюда передается кораблик

                    KillShip(ship);
                    SetImage(ship.Source, (int)ship.ShipClass, ship.Location.X, ship.Location.Y, Player2Grid);
                    Model.CanShot = true;
                }
            });

            
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
                }   //Model.CanShot = false;




            });


        }

        public static async Task<Location> AwaitReceive(Socket socket)
        {
            byte[] resp = new byte[1024];
            await new Task(() => socket.Receive(resp));

            Location location = new Location();

            return location;
        }


        public void SetImage(string source, int lenth, int x, int y, Grid grid)
        {
            Image image = new Image(); 
            BitmapImage bitmapImage = new BitmapImage();
            var uri = new Uri(source, UriKind.Absolute);
            bitmapImage.UriSource = uri;
            image.Source = bitmapImage;
            image.HorizontalAlignment = HorizontalAlignment.Stretch;
            image.VerticalAlignment = VerticalAlignment.Stretch;
            grid.Children.Add(image);
            var index = (uint)grid.Children.IndexOf(image);
            grid.Children.Move(index, (uint)grid.Children.Count - 1);

            Grid.SetColumnSpan(image, lenth);
            Grid.SetRow(image, y);
            Grid.SetColumn(image, x);
        }

        public GamePage()
        {
            this.InitializeComponent();


        }

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

        private void Rectangle_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (Model.CanShot == true)
            {
                Rectangle rec = sender as Rectangle;
                rec.Fill = new SolidColorBrush(Colors.White);
            }
        }

        private void Rectangle_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (Model.CanShot == true)
            {
                Rectangle rec = sender as Rectangle;
                rec.Fill = new SolidColorBrush(Colors.Red);
            }
        }

        private void CreateField(Grid grid)
        {
            for (int i = 0; i < 10; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }
        }

        private async void Rectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (Model.CanShot==false)
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

                // Create a TCP/IP socket.  
                Socket client = socket;//new Socket(remoteEP.Address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                StateObject state = new StateObject();
                state.workSocket = client;
                //state.obj = EnemyGameField;

                JObject jObject = new JObject();
                jObject.Add(JsonStructInfo.Type, Request.EnumTypeToString(Request.RequestTypes.Shot));
                jObject.Add(JsonStructInfo.Result, Serializer<Location>.SetSerializedObject(location));

                string s = jObject.ToString() + JsonStructInfo.EndOfMessage;

                Send(state, s);
                //client.Send(Encoding.UTF8.GetBytes(s));
                //pingDone.Set();

                //Receive(state);
                pingDone.WaitOne();
                //client.Receive(resp);
            });

            //string response = Encoding.UTF8.GetString(resp);

            
        }

        public static string response = string.Empty;

        //удар по мне
        private static void ReceiveCallbackEnemyShot(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                int bytesRead = client.EndReceive(ar);

                state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                // All the data has arrived; put it in response.  
                if (state.sb.Length > 1)
                {
                    response = state.sb.ToString();
                }

                response = response.Remove(response.LastIndexOf(JsonStructInfo.EndOfMessage));

                JObject jObject = null;

                Answer.AnswerTypes type = Answer.AnswerTypes.ShotOfTheEnemy;
                //SeaBattleClassLibrary.DataProvider.ShotResult.ShotResultType result = SeaBattleClassLibrary.DataProvider.ShotResult.ShotResultType.Miss;

                try
                {
                    jObject = JObject.Parse(response);
                } 
                catch (JsonReaderException e)
                {
                    Console.WriteLine(e);
                }

                type = Answer.JsonTypeToEnum((string)jObject[JsonStructInfo.Type]);
                Location location = Serializer<Location>.GetSerializedObject((string)jObject[JsonStructInfo.Result]);

                MyGameField.Shot(location);
                pingDone.Reset();
            } 
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        //удар по врагу
        private static void ReceiveCallbackMyShot(IAsyncResult ar)
        {
            try
            {
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                // Retrieve the state object and the client socket   
                // from the asynchronous state object.  
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;
                EnemyGameField enemyGameField = (EnemyGameField)state.obj;

                // Read data from the remote device.  
                int bytesRead = client.EndReceive(ar);

                state.sb.Append(Encoding.UTF8.GetString(state.buffer, 0, bytesRead));
                // All the data has arrived; put it in response.  
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
                    Serializer<Ship>.GetSerializedObject((string)jObject[JsonStructInfo.AdditionalContent]) : null;
                Location location = Serializer<Location>.GetSerializedObject((string)jObject[JsonStructInfo.Content]);

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

        public static Answer.AnswerTypes GetAnswerType(string jsonRequest)
        {
            JObject jObject;

            try
            {
                jObject = JObject.Parse(jsonRequest);
            } 
            catch (JsonReaderException e)
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

        public void KillShip(Ship ship)
        {
            for (int i = ship.Location.X-1; i <= ship.Location.X+ship.ShipWidth; i++)
            {
                for(int j=ship.Location.Y-1; j <= ship.Location.Y + ship.ShipHeight; j++)
                {
                    if(i<10 && j < 10 && i>-1 && j>-1)
                    {
                        Rectangle rectangle = new Rectangle();
                        Player2Grid.Children.Add(rectangle);
                        Grid.SetColumn(rectangle, i);
                        Grid.SetRow(rectangle, j);
                        
                        rectangle.Fill = new SolidColorBrush(Colors.Black);
                    }
                }
            }
        }

        private static void Send(StateObject state, String data)
        {
            Socket client = state.workSocket;
            // Convert the string data to byte data using ASCII encoding.  
            byte[] byteData = Encoding.UTF8.GetBytes(data);

            // Begin sending the data to the remote device.  
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), state);
        }


        private static StateObject stateObject = new StateObject();

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

        private static void Receive(StateObject state)
        {
            try
            {
                Socket client = state.workSocket;
                // Begin receiving the data from the remote device.  
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallbackMyShot), state);
            } catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }
    }
}
