using SeaBattleClassLibrary.Game;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace SeaBattleClient
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class StartPage : Page
    {

        public Player Model
        {
            get
            {
                return this.DataContext as Player;
            }
        }

        Player player = null;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
                player=e.Parameter as Player;
        }

        public StartPage()
        {
            this.InitializeComponent();

        }

        private void BtnCreateGame_Click(object sender, RoutedEventArgs e)
        {
            (Parent as Frame).Navigate(typeof(CreateGamePage), Model);
        }

        private void BtnJoinGame_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
