using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;

namespace scdUnpack
{
    public class UnpackItems : ObservableCollection<UnpackItem>
    {
        public string RootPath { get; set; }
        public string Ext { get; set; }
        public ScdUnpackSettings Settings { get; internal set; }
        public StackPanel StackPanelPath { get; internal set; }

        public UnpackItems()
        {
        }


        public void UpdatePath()
        {
            var dialog = new OpenFolderDialog
            {
                AddToRecent = true,
                InitialDirectory = RootPath,
                Multiselect = false
            };
            if (dialog.ShowDialog().GetValueOrDefault())
            {
                RootPath = dialog.FolderName;
                Settings.SetPathByExt(RootPath, Ext);
                UpdateStackPanels();
            }
        }

        internal void UpdateStackPanels()
        {
            foreach (TextBlock panel in StackPanelPath.Children)
            {
                panel.MouseEnter -= Panel_MouseEnter;
                panel.MouseLeave -= Panel_MouseLeave;
            }
            StackPanelPath.Children.Clear();
            var rootPathParts = RootPath.Split('\\');
            for (int i = 0; i < rootPathParts.Length; i++)
            {
                var part = rootPathParts[i];
                var panel = new TextBlock { Text = (i == 0 ? string.Empty : "\\") + part };
                if (i > 0 && i < rootPathParts.Length - 1)
                {
                    panel.MouseEnter += Panel_MouseEnter;
                    panel.MouseLeave += Panel_MouseLeave;
                }
                StackPanelPath.Children.Add(panel);
            }
        }

        private void Panel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var panel = sender as TextBlock;
            panel.FontWeight = FontWeights.Regular;
        }

        private void Panel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            var panel = sender as TextBlock;
            panel.FontWeight = FontWeights.Bold;
        }

        internal void Load()
        {
            UpdateStackPanels();
            Clear();
            var dirItems = Directory.GetFiles(RootPath, "*" + Ext)
                .Concat(Directory.GetDirectories(RootPath))
                .Order()
                .Select(file => new UnpackItem(file));
            foreach (var file in dirItems)
            {
                Add(file);
            }
        }
    }

    public class UnpackItem : INotifyPropertyChanged
    {
        private string path;
        private string folder;
        private string unpacked;

        public UnpackItem(string path)
        {
            this.path = path;
            Name = Path.GetFileName(path);
            if (Directory.Exists(path))
            {
                Kind = "Directory";
                unpacked = string.Empty;
            }
            else
            {
                Kind = "File";
                folder = Path.GetFileNameWithoutExtension(path);
                unpacked = Directory.Exists(Path.Combine(path, folder)) ? "Yes" : "No";
            }
        }

        public string Name { get; }
        public string Folder { get; }
        public string Kind { get; set; }

        public string Unpacked
        {
            get => unpacked;
            set
            {
                if (unpacked != value)
                {
                    unpacked = value;
                    NotifyPropertyChanged("Unpacked");
                }
            }

        }

        public void Toggle()
        {
            if ()
                var zipFile = Path.Combine(settings.ModsPath, item.Name);
            var zipDir = Path.Combine(settings.ModsPath, item.Folder);
            if (Directory.Exists(zipDir) && !item.Unpacked)
                throw new InvalidOperationException();
            if (item.Unpacked)
            {
                var tempZipFile = zipFile + ".temp";
                var backupZipFile = zipFile + ".backup";
                if (File.Exists(tempZipFile))
                {
                    File.Delete(tempZipFile);
                }
                ZipFile.CreateFromDirectory(zipDir, tempZipFile);
                if (!File.Exists(backupZipFile))
                {
                    File.Copy(zipFile, backupZipFile);
                }
                File.Move(tempZipFile, zipFile, true);
                Directory.Delete(zipDir, true);
                item.Unpacked = false;
            }
            else
            {
                if (Directory.Exists(zipDir))
                {
                    Directory.Delete(zipDir, true);
                }
                ZipFile.ExtractToDirectory(zipFile, zipDir);
                item.Unpacked = true;
                var info = new ProcessStartInfo
                {
                    FileName = @"C:\Users\jonle\AppData\Local\Programs\Microsoft VS Code\Code.exe",
                    ArgumentList = { zipDir }
                };
                Process.Start(info);

            }

        }
        public event PropertyChangedEventHandler? PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

}