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
        private Windows.UI.Color backColor;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
                player = e.Parameter as Player;

            //заполнить поле первого игрока
            CreateField(Player1Grid);
            FillFieldWithRectangle(Player1Grid);

            foreach (ClientShip ship in Model.GameField.Ships)
            {
                Image image = new Image();
                BitmapImage bitmapImage = new BitmapImage();
                var uri = new Uri(ship.Source);
                bitmapImage.UriSource = uri;
                image.Source = bitmapImage;
                //image.Width = ship.ShipWidth * (Player1Grid.Width / 10);
                //image.Height = ship.ShipHeight * (Player1Grid.Height / 10);
                image.HorizontalAlignment = HorizontalAlignment.Stretch;
                image.VerticalAlignment = VerticalAlignment.Stretch;
                Player1Grid.Children.Add(image);


                Grid.SetColumnSpan(image, (int)ship.ShipClass);
                Grid.SetRow(image, ship.Location.Y);
                Grid.SetColumn(image, ship.Location.X);
            }


            //заполнить поле второго игрока
            CreateField(Player2Grid);
            FillFieldWithRectangle(Player2Grid);
        }

        public GamePage()
        {
            this.InitializeComponent();
        }

        private void FillFieldWithRectangle(Grid grid)
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

                    rectangle.Tapped += Rectangle_Tapped;
                }
            }
        }

        private void Rectangle_Tapped(object sender, TappedRoutedEventArgs e)
        {
            (sender as Rectangle).Fill = new SolidColorBrush(Colors.Azure);
        }

        private void CreateField(Grid grid)
        {
            for (int i = 0; i < 10; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            }
        }
    }
}
