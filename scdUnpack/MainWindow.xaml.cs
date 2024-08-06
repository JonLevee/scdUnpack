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
        private bool viewsInitialized = false;

        public MainWindow()
        {
            InitializeComponent();
            settings = ScdUnpackSettings.Load();
            ModsView.ItemsSource = new UnpackItems
            {
                SettingName = "ModsPath",
                RootPath = settings.ModsPath,
                Settings = settings,
                StackPanelPath = ModsPathText
            };
            GameView.ItemsSource = new UnpackItems
            {
                SettingName = "GamePath",
                RootPath = settings.GamePath,
                Settings = settings,
                StackPanelPath = GamePathText
            };

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
                if (!viewsInitialized)
                {
                    listView.ContextMenu = new ContextMenu();
                    listView.ContextMenu.Items.Add(new MenuItem { Header = "Pack" });
                    listView.ContextMenu.Items.Add(new MenuItem { Header = "Unpack" });
                    foreach(MenuItem menuItem in listView.ContextMenu.Items)
                    {
                        menuItem.Click += HandleMenuClick;
                    }
                    ((CollectionView)CollectionViewSource.GetDefaultView(listView.ItemsSource))
                        .SortDescriptions
                        .Add(new SortDescription("Name", ListSortDirection.Ascending));
                }
                items.Load();
            }
            viewsInitialized = true;
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

        private void HandleMenuClick(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var item = menuItem.Tag as UnpackItem;
            Debug.Assert(item != null);
            switch (menuItem.Header)
            {
                case "Pack":
                    item.Pack();
                    break;
                case "Unpack":
                    item.Unpack();
                    break;
            }
        }
        private void View_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                var listView = (ListView)sender;
                var selected = (UnpackItem)listView.SelectedItem;
                foreach (MenuItem menuItem in listView.ContextMenu.Items)
                {
                    menuItem.Tag = selected;
                    switch (menuItem.Header)
                    {
                        case "Pack":
                            menuItem.IsEnabled = selected.Kind == UnpackKind.Directory;
                            break;
                        case "Unpack":
                            menuItem.IsEnabled = selected.Kind != UnpackKind.Directory;
                            break;
                    }
                }
            }
        }
    }
}