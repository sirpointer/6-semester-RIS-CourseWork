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
                    rectangle.Fill = new SolidColorBrush(Colors.White);
                    //rectangle.CanDrag = true;
                    //rectangle.IsHitTestVisible = false;
                    FieldGrid.Children.Add(rectangle);
                    Grid.SetRow(rectangle, i);
                    Grid.SetColumn(rectangle, j);
                }
            }

            Image image = new Image();
            BitmapImage bitmapImage = new BitmapImage();//new Uri("ms-appx:///SeaBsttleClient/Assets/Ships/ship.jpg"));
            //var uri = new Uri("/Assets/hips/ship.jpg");
            //var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            //bitmapImage.BeginInit();
            var uri = new Uri("ms-appx:///Assets/Ships/ship.jpg", UriKind.Absolute);
            bitmapImage.UriSource = uri; // new Uri(@"/Assets/Ships/ship.jpg", UriKind.RelativeOrAbsolute);
            image.Source = bitmapImage;
            image.Width = 100;
            FieldGrid.Children.Add(image);
            Grid.SetRow(image, 0);
            Grid.SetColumn(image, 0);

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

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
             Image image = (Image)sender;
             /*Windows.ApplicationModel.DataTransfer.DragDrop.DoDragDrop(image, image.DataContext, DragDropEffects.Copy);
             e.AcceptedOperation = DataPackageOperation.Copy;*/
        
        }

        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            
            
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();
                if (items.Count > 0)
                {
                    var storageFile = items[0] as StorageFile;
                    var bitmapImage = new BitmapImage();
                    bitmapImage.SetSource(await storageFile.OpenAsync(FileAccessMode.Read));
                    // Set the image on the main page to the dropped image
                    //Image.Source = bitmapImage;
                }
            }
            

            /*Rectangle rectangle = new Rectangle() { StrokeThickness = 1 };
            rectangle.Stroke = new SolidColorBrush(Colors.Red);
            rectangle.Fill = new SolidColorBrush(Colors.Red);
            rectangle.CanDrag = true;
            //rectangle.IsHitTestVisible = false;
            FieldGrid.Children.Add(rectangle);
            Grid.SetRow(rectangle, 0);
            Grid.SetColumn(rectangle, 0);*/


            /*Image image = new Image();
            BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/Ships/ship.jpg"));
            //var uri = new Uri("/Assets/hips/ship.jpg");
            //var file = await StorageFile.GetFileFromApplicationUriAsync(uri);
            image.Source = bitmapImage;
            image.Width = 100;
            FieldGrid.Children.Add(image);
            Grid.SetRow(image, 5);
            Grid.SetColumn(image, 5);
            Grid.SetColumnSpan(image, 2);*/
        }

        private void MyImage_DragStarting(UIElement sender, DragStartingEventArgs args)
        {
           
        }

        private void FieldGrid_DropCompleted(UIElement sender, DropCompletedEventArgs args)
        {
            
        }
    }
}
