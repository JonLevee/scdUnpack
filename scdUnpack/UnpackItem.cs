using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace scdUnpack
{
    public class UnpackItems : ObservableCollection<UnpackItem>
    {
        private static readonly string[] FileExtensions = ["*.scd", "*.sc2"];

        public string RootPath { get; set; }
        public ScdUnpackSettings Settings { get; internal set; }
        public StackPanel StackPanelPath { get; internal set; }
        public string SettingName { get; internal set; }

        public UnpackItems()
        {
        }

        public void UpdatePath(string newPath = null)
        {
            if (newPath == null)
            {
                var dialog = new OpenFolderDialog
                {
                    AddToRecent = true,
                    InitialDirectory = RootPath,
                    Multiselect = false
                };
                if (dialog.ShowDialog().GetValueOrDefault())
                {
                    newPath = dialog.FolderName;
                }
            }
            Debug.Assert(newPath != null);
            RootPath = newPath;
            Settings.SetPathByName(newPath, SettingName);
            Load();
        }

        internal void UpdateStackPanels()
        {
            foreach (TextBlock panel in StackPanelPath.Children)
            {
                panel.MouseEnter -= Panel_MouseEnter;
                panel.MouseLeave -= Panel_MouseLeave;
                panel.MouseUp -= Panel_MouseUp;
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
                    panel.MouseUp += Panel_MouseUp;
                }
                StackPanelPath.Children.Add(panel);
            }
        }

        private void Panel_MouseUp(object sender, MouseButtonEventArgs e)
        {
            switch(e.ChangedButton)
            {
                case MouseButton.Left:
                    var panel = (TextBlock)sender;
                    var panels = StackPanelPath.Children.Cast<TextBlock>().TakeWhile(p => p != panel).Select(p => p.Text).ToList();
                    panels.Add(panel.Text);
                    var path = string.Concat(panels);
                    UpdatePath(path);
                    break;
            }
        }

        private void Panel_MouseLeave(object sender, MouseEventArgs e)
        {
            var panel = sender as TextBlock;
            panel.FontWeight = FontWeights.Regular;
        }

        private void Panel_MouseEnter(object sender, MouseEventArgs e)
        {
            var panel = sender as TextBlock;
            panel.FontWeight = FontWeights.Bold;
        }

        internal void Load()
        {
            UpdateStackPanels();
            Clear();
            foreach(var item in Items)
            {
                item.DirectoryChanged -= OnDirectoryChanged;
            }
            var dirItems = FileExtensions
                .SelectMany(ext=>Directory.GetFiles(RootPath, ext))
                .Concat(Directory.GetDirectories(RootPath))
                .Order()
                .Select(file => new UnpackItem(file));
            foreach (var item in dirItems)
            {
                item.DirectoryChanged += OnDirectoryChanged;
                Add(item);
            }
        }

         private void OnDirectoryChanged(object sender, string newPath)
        {
            RootPath = newPath;
            Load();
        }
    }

    public enum UnpackKind
    {
        Directory,
        SCD,
        SC2
    }

    public class UnpackItem : INotifyPropertyChanged
    {
        private bool? unpacked;

        public UnpackItem(string path)
        {
            FullPath = path;
            Name = Path.GetFileName(path);
            if (Directory.Exists(path))
            {
                Kind = UnpackKind.Directory;
                unpacked = null;
            }
            else
            {
                var directory = Path.GetDirectoryName(path);
                var folder = Path.GetFileNameWithoutExtension(path);
                PackedZipFile = Path.Combine(FullPath, Name);
                TempZipFile = PackedZipFile + ".temp";
                BackupZipFile = PackedZipFile + ".backup";
                UnpackedFolder = Path.Combine(FullPath, Folder);
                unpacked = Directory.Exists(Path.Combine(path, Folder));
                switch (Path.GetExtension(path).ToLower())
                {
                    case ".scd":
                        Kind = UnpackKind.SCD;
                        break;
                    case ".sc2":
                        Kind = UnpackKind.SC2;
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }
        }

        public string Name { get; }
        public UnpackKind Kind { get; }
        public string FullPath { get; }
        public string PackedZipFile { get; }
        public string TempZipFile { get; } 
        public string BackupZipFile { get; }
        public string UnpackedFolder { get; }

        public bool? Unpacked
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
            switch (Kind)
            {
                case UnpackKind.Directory:
                    DirectoryChanged?.Invoke(this, FullPath);
                    break;
                case UnpackKind.SCD:
                    break;
                case UnpackKind.SC2:
                    break;
                default:
                    throw new InvalidOperationException();
            }
        }

        public void Pack()
        {
            if (File.Exists(TempZipFile))
            {
                File.Delete(TempZipFile);
            }
            ZipFile.CreateFromDirectory(UnpackedFolder, TempZipFile);
            if (!File.Exists(BackupZipFile))
            {
                File.Copy(PackedZipFile, BackupZipFile);
            }
            File.Move(TempZipFile, PackedZipFile, true);
            Directory.Delete(UnpackedFolder, true);
            Unpacked = false;
        }

        public void Unpack()
        {
            if (Directory.Exists(UnpackedFolder))
            {
                Directory.Delete(UnpackedFolder, true);
            }
            ZipFile.ExtractToDirectory(PackedZipFile, UnpackedFolder);
            if (Kind == UnpackKind.SC2)
            {
                RecursiveUnpack(UnpackedFolder);
            }
            Unpacked = true;
            var info = new ProcessStartInfo
            {
                FileName = @"C:\Users\jonle\AppData\Local\Programs\Microsoft VS Code\Code.exe",
                ArgumentList = { UnpackedFolder }
            };
            //Process.Start(info);
        }

        private void RecursiveUnpack(string folder)
        {
            var files = Directory.GetFiles(folder,"*.zip").Concat(Directory.GetFiles(folder, "*.scd"));
            foreach (var file in files)
            {
                var target = Path.ChangeExtension(file, null);
                ZipFile.ExtractToDirectory(file, target);
            }
            foreach(var dir in Directory.GetDirectories(folder))
            {
                RecursiveUnpack(dir);
            }
        }

        public EventHandler<string> DirectoryChanged;

        public event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

}