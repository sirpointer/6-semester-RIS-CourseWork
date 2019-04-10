using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace SeaBattleClient
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            //сделать поле
            for(int i=0; i < 10; i++)
            {
                FieldGrid.RowDefinitions.Add(new RowDefinition());
                FieldGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            //заполнить поле квадратиками
            for(int i = 0; i < FieldGrid.RowDefinitions.Count(); i++)
            {
                for(int j=0; j < FieldGrid.ColumnDefinitions.Count; j++)
                {
                    Rectangle rectangle = new Rectangle() { StrokeThickness = 1 };
                    rectangle.Stroke = new SolidColorBrush(Colors.Black);
                    FieldGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i);
                    Grid.SetColumn(rectangle, j);
                }
            }

            /*Rectangle rec = new Rectangle() { StrokeThickness = 1 };
            rec.Stroke = new SolidColorBrush(Colors.Red);
            rec.Fill = new SolidColorBrush(Colors.Azure);
            rectangleGrid.Children.Add(rec);
            Grid.SetRow(rec, 0);
            Grid.SetColumn(rec, 0);*/

            //Rectangle rec = new Rectangle() { StrokeThickness = 1 };
            //rec.Stroke = new SolidColorBrush(Colors.Red);
            //rec.Margin = new Thickness(960, 350, 510, 620);
            //rec.Height = 30;
            //rec.Width = 30;
        }

    }
}
