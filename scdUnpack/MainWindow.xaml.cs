using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace scdUnpack
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ScdUnpackSettings settings;

        public MainWindow()
        {
            InitializeComponent();
            settings = ScdUnpackSettings.Load();
            ModsView.ItemsSource = new UnpackItems
            {
                RootPath = settings.ModsPath,
                Ext = "*.sc2",
                Settings = settings,
                StackPanelPath = ModsPathText
            };
            GameView.ItemsSource = new UnpackItems
            {
                RootPath = settings.GamePath,
                Ext = "*.scd",
                Settings = settings,
                StackPanelPath = GamePathText
            };


            ((CollectionView)CollectionViewSource.GetDefaultView(ModsView.ItemsSource)).SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            ((CollectionView)CollectionViewSource.GetDefaultView(GameView.ItemsSource)).SortDescriptions.Add(new SortDescription("Name", ListSortDirection.Ascending));
            LoadItems();
        }

        private void ModsPathButton_Click(object sender, RoutedEventArgs e)
        {
            ((UnpackItems)ModsView.ItemsSource).UpdatePath();
        }

        private void GamePathButton_Click(object sender, RoutedEventArgs e)
        {
            ((UnpackItems)GameView.ItemsSource).UpdatePath();
        }

        private void LoadItems()
        {
            foreach (var listView in new[] { ModsView, GameView })
            {
                var items = (UnpackItems)listView.ItemsSource;
                items.Load();
            }
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            settings.Save();
        }

        private void UnpackView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = ((ListView)sender).SelectedValue as UnpackItem;
            Debug.Assert(item != null);
            item.Toggle();
        }
    }
}