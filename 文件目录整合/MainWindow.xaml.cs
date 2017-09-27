using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace 文件目录整合
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window , IView
    {
        MainWindowViewModel viewModel;
        public MainWindow()
        {             
            InitializeComponent();
            viewModel = new MainWindowViewModel();
            viewModel.View = this;
            this.DataContext = viewModel;
            this.Loaded += MainWindow_Loaded;
            ChangeTheme();//路径选择
        }

        public Dispatcher ViewDispathcer
        {
            get => this.Dispatcher;
        }

        public void SetSnackBarMessage(string message)
        {
            SnackbarFour.MessageQueue.Enqueue(message, "知道啦", param => Trace.WriteLine("Actioned: " + param), message);
        }

        private void ChangeTheme()
        {
            var swatch = GetSwatchByName("lightblue");
            Console.WriteLine($"name = {swatch.Name}");
            var paletteHelper = new PaletteHelper();
            paletteHelper.ReplacePrimaryColor(swatch);
            try
            {
                paletteHelper.ReplaceAccentColor(swatch);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private Swatch GetSwatchByName(string name = "grey")
        {
            var swatches = new SwatchesProvider().Swatches;
            var swaList = from swa in swatches
                          where swa.Name == name
                          select new Swatch(swa.Name, swa.PrimaryHues, swa.AccentHues);
            foreach (var item in swaList)
            {
                return item;
            }
            return null;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            //var list = FileManager.GetFileListInDirectory(@"U:\cloudmusic");
            //foreach (var item in list)
            //{
            //    Console.WriteLine(item);
            //}

            //var listDir = FileManager.GetChildDirectorys(@"U:\cloudmusic");

            //foreach (var item in listDir)
            //{
            //    Console.WriteLine(item);
            //}

            cmb_NameRule.SelectedIndex = 1;
            cmb_ClassifyRule.SelectedIndex = 1;
            cmb_OldNameRule.SelectedIndex = 1;            

            //var infoList = FileManager.GetMusicInfoInDirectory(@"U:\cloudmusic");            
            //foreach (var item in infoList)
            //{
            //    //Console.WriteLine($"Name={item.FileName},\nPath={item.FileFullPath}\nDir={item.FileDirName}");
            //    Console.WriteLine($"Full={item.FileFullPath},Dir={item.FileDirName},Name={item.FileName}");
            //}

        }
    }
}
