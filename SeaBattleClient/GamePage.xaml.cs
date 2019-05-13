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
        private Color backColor;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
                player = e.Parameter as Player;

            //заполнить поле первого игрока
            CreateField(Player1Grid);
            FillFieldWithRectangle(Player1Grid, true);

            foreach (ClientShip ship in Model.GameField.Ships)
            {
                SetImage(ship.Source, (int)ship.ShipClass, ship.Location.X, ship.Location.Y, Player1Grid);
            }


            //заполнить поле второго игрока
            CreateField(Player2Grid);
            FillFieldWithRectangle(Player2Grid);
        }

        public void SetImage(string source, int lenth, int x, int y, Grid grid)
        {
            Image image = new Image(); 
            BitmapImage bitmapImage = new BitmapImage();
            var uri = new Uri(source);
            bitmapImage.UriSource = uri;
            image.Source = bitmapImage;
            image.HorizontalAlignment = HorizontalAlignment.Stretch;
            image.VerticalAlignment = VerticalAlignment.Stretch;
            grid.Children.Add(image);


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
                    if(mine)
                        rectangle.Tapped += Rectangle_Tapped;
                }
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

                JObject jObject = new JObject();
                jObject.Add(JsonStructInfo.Type, Request.EnumTypeToString(Request.RequestTypes.Shot));
                jObject.Add(JsonStructInfo.Result, Serializer<Location>.SetSerializedObject(location));

                string s = jObject.ToString() + JsonStructInfo.EndOfMessage;

                client.Send(Encoding.UTF8.GetBytes(s));

                client.Receive(resp);
            });

            string response = Encoding.UTF8.GetString(resp);

            if (true) //промах
            {
                rec.Fill = new SolidColorBrush(Colors.Black);
            }
            if (true) //ранил
            {
                SetImage("ms - appx:///Assets/Ships/ранен.jpg", 1, col, row, Player2Grid);
            }
            if (true) //убил
            {
                Ship ship = new Ship(100); // сюда передается кораблик
                KillShip(ship); 
                SetImage("ms - appx:///Assets/Ships/ранен.jpg", (int)ship.ShipClass, ship.Location.Y, ship.Location.X, Player2Grid);
            }
        }

        public void KillShip(Ship ship)
        {
            for (int i = ship.Location.X-1; i < ship.Location.X+ship.ShipWidth; i++)
            {
                for(int j=ship.Location.Y; j < ship.Location.Y + ship.ShipHeight; j++)
                {
                    Rectangle rectangle = new Rectangle();
                    Grid.SetColumn(rectangle, i);
                    Grid.SetRow(rectangle, j);

                    rectangle.Fill = new SolidColorBrush(Colors.Black);
                }
            }
        }
    }
}
