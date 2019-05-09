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

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace SeaBattleClient
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public Player Model
        {
            get
            {
                return this.DataContext as Player;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.DataContext = new Player();

            List<Ship> ships = new List<Ship>(10);

            for (int i = 1; i <= 4; i++)
            {
                ships.Add(new ClientShip(i, ShipClass.OneDeck));
            }
            for (int i = 5; i <= 7; i++)
            {
                ships.Add(new ClientShip(i, ShipClass.TwoDeck));
            }
            for (int i = 8; i <= 9; i++)
            {
                ships.Add(new ClientShip(i, ShipClass.ThreeDeck));
            }
            ships.Add(new ClientShip(10, ShipClass.FourDeck));

            GameField gameField = new GameField(ships);

            Model.GameField = gameField;

            MyFrame.Navigate(typeof(StartPage), Model);
        }

        
    }
}
