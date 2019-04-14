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
            for (int i = 0; i < 10; i++)
            {
                FieldGrid.RowDefinitions.Add(new RowDefinition());
                FieldGrid.ColumnDefinitions.Add(new ColumnDefinition());
            }

            //заполнить поле квадратиками
            for (int i = 0; i < FieldGrid.RowDefinitions.Count(); i++)
            {
                for (int j = 0; j < FieldGrid.ColumnDefinitions.Count; j++)
                {
                    Rectangle rectangle = new Rectangle() { StrokeThickness = 1 };
                    rectangle.Stroke = new SolidColorBrush(Colors.Black);
                    rectangle.Fill = new SolidColorBrush(Colors.White);
                    FieldGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i);
                    Grid.SetColumn(rectangle, j);
                }
            }
        }

        private Point shiftPoint = new Point(double.NaN, double.NaN);

        //изменение координаты мыши
        private void MyImage_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            Image image = sender as Image;
            Pointer prt = e.Pointer;
            
            //передвижение картинки
            if (prt.PointerDeviceType == Windows.Devices.Input.PointerDeviceType.Mouse)
            {
                PointerPoint ptrPt = e.GetCurrentPoint(canvas);
                
                if (ptrPt.Properties.IsLeftButtonPressed)
                {
                    if (!double.IsNaN(shiftPoint.X))
                    {
                        Point newPoint = new Point(ptrPt.Position.X - shiftPoint.X, ptrPt.Position.Y - shiftPoint.Y);

                        Canvas.SetLeft(myImage, newPoint.X);
                        Canvas.SetTop(myImage, newPoint.Y);
                    }
                }
            }
        }


        private void MyImage_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            Pointer ptr = e.Pointer;
            PointerPoint ptrPt = e.GetCurrentPoint(canvas);

            if (ptrPt.Properties.IsLeftButtonPressed)
            {
                shiftPoint = new Point(ptrPt.Position.X - Canvas.GetLeft(myImage), ptrPt.Position.Y - Canvas.GetTop(myImage));
            }
        }

        //отпускание
        private void MyImage_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            shiftPoint = new Point(double.NaN, double.NaN);

            PointerPoint ptrPt = e.GetCurrentPoint(canvas);

            // вынеси условия из if
            // типа bool inside = !(ptrPt.Position.X > Canvas.GetLeft(FieldGrid) &&...
            // if (inside) ...

            //можно расположить только внутри поля
            if (!(ptrPt.Position.X > Canvas.GetLeft(FieldGrid) && ptrPt.Position.Y > Canvas.GetTop(FieldGrid)
               && ptrPt.Position.X < Canvas.GetLeft(FieldGrid) + FieldGrid.Width && ptrPt.Position.Y < Canvas.GetTop(FieldGrid) + FieldGrid.Height))
            {
                Canvas.SetLeft(myImage, 0);
                Canvas.SetTop(myImage, 0);
            }
            else
            {
                int column = Convert.ToInt32((ptrPt.Position.X - Canvas.GetLeft(FieldGrid)) / (FieldGrid.Width / 10)) - 1;
                int row = Convert.ToInt32((ptrPt.Position.Y - Canvas.GetTop(FieldGrid)) / (FieldGrid.Height / 10)) - 1;


            }
        }
    }
}
