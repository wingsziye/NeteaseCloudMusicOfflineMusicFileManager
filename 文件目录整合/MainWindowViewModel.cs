using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 文件目录整合
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public IView View { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private Action<PropertyChangedEventArgs> RaisePropertyChanged()
        {
            return args => PropertyChanged?.Invoke(this, args);
        }

        #region Properties

        public ObservableCollection<MoveRecordClass> MoveRecordCollection { get; set; }//用于列表显示移动记录
        public ObservableCollection<string> NameRuleCollection { get; set; }//用于列表显示移动记录
        public ObservableCollection<string> ClassifyRuleCollection { get; set; }//用于列表显示移动记录

        private FileNameRules oldSelectedNameRule;
        private FileNameRules selectedNameRule;
        private FileClassifyRules selectedClassifyRule;
        private string selectedPath;
        private bool isNotStartingClassify = true;
        private int dealProgress = 0;
        private int maxProgress = 0;

        public string SelectedPath
        {
            get => selectedPath;
            set
            {
                selectedPath = value;
                OnPropertyChanged(nameof(SelectedPath));
                MoveFilesCommand.RaiseCanExecuteChanged();
            }
        }

        public FileNameRules SelectedNameRules
        {
            get => selectedNameRule;
            set
            {
                selectedNameRule = value;
                OnPropertyChanged(nameof(SelectedNameRules));
            }
        }

        public FileNameRules OldSelectedNameRules
        {
            get => oldSelectedNameRule;
            set
            {
                oldSelectedNameRule = value;
                OnPropertyChanged(nameof(OldSelectedNameRules));
            }
        }

        public FileClassifyRules SelectedClassifyRule
        {
            get => selectedClassifyRule;
            set
            {
                selectedClassifyRule = value;
                OnPropertyChanged(nameof(SelectedClassifyRule));
                MoveFilesCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsNotStartingClassify
        {
            get => isNotStartingClassify;
            set
            {
                isNotStartingClassify = value;
                OnPropertyChanged(nameof(IsNotStartingClassify));
                MoveFilesCommand.RaiseCanExecuteChanged();
            }
        }

        public int DealProgress
        {
            get => dealProgress;
            set
            {
                dealProgress = value;
                OnPropertyChanged(nameof(DealProgress));
            }
        }

        public int MaxProgress
        {
            get => maxProgress;
            set
            {
                maxProgress = value;
                OnPropertyChanged(nameof(MaxProgress));
            }
        }

        #endregion Properties

        #region Constructor and private functions

        public MainWindowViewModel()
        {
            MoveRecordCollection = new ObservableCollection<MoveRecordClass>();
            initCommands();//初始化Commands
            initStringRuleCollection();
        }

        private void initStringRuleCollection()
        {
            NameRuleCollection = new ObservableCollection<string>();
            ClassifyRuleCollection = new ObservableCollection<string>();
            var fnc = System.Enum.GetNames(typeof(FileNameRules));
            var fcc = System.Enum.GetNames(typeof(FileClassifyRules));
            foreach (var item in fnc)
            {
                NameRuleCollection.Add(EnumRuleToString(item));
            }
            foreach (var item in fcc)
            {
                ClassifyRuleCollection.Add(EnumRuleToString(item));
            }
        }

        private void initCommands()
        {
            SelectDirCommand = new DelegateCommand(SelectDir, () => true);
            MoveFilesCommand = new DelegateCommand(MoveFiles, CanMoveFiles);
        }

        private bool CanMoveFiles()
        {
            if (!IsNotStartingClassify)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(SelectedPath))
            {
                return false;
            }

            switch (SelectedClassifyRule)
            {
                case FileClassifyRules.不分文件夹:
                    break;

                case FileClassifyRules.按歌手分文件夹:
                    return true;

                case FileClassifyRules.按歌手x专辑分文件夹:
                    break;

                default:
                    break;
            }
            return false;
        }

        private async void MoveFiles()
        {
            FileManager.InitManager();
            await Task.Run(() =>
            {
                var mlist = FileManager.GetMusicInfoInDirectory(SelectedPath);

                View.ViewDispathcer.Invoke(() =>
                {
                    this.MoveRecordCollection.Clear();//清空显示列表
                });
                
                this.DealProgress = 0;//重置进度条
                this.MaxProgress = mlist.Count;//重置进度条最大值

                if (mlist == null || mlist.Count == 0)
                {
                    this.IsNotStartingClassify = true;
                    return;
                }

                this.TidyMusicNames(mlist, this.OldSelectedNameRules);//根据规则，识别歌手名和歌曲名

                MoveFilesInRule(mlist, this.SelectedClassifyRule,this.SelectedNameRules);
                
            });
        }

        private void SelectDir()
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.ShowNewFolderButton = false;
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                SelectedPath = folderBrowserDialog.SelectedPath;
            }
            else
            {
            }
        }

        /// <summary>
        /// 将枚举格式化为字符串
        /// </summary>
        /// <param name="ruleName"></param>
        /// <returns></returns>
        private static string EnumRuleToString(string ruleName)
        {
            var s = ruleName.Split('x');
            if (s.Length >= 2)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append($"{s[0]}{FileNameSeparator.Separator}");
                for (int i = 1; i < s.Length; i++)
                {
                    builder.Append(s[i]);
                }
                return builder.ToString();
            }
            else
            {
                return ruleName;
            }
        }

        #endregion Constructor and private functions

        public void TidyMusicNames(List<MusicFileNameInfo> info, FileNameRules oldNameRules)
        {
            foreach (var item in info)
            {
                item.SetSingerAndSongName(oldNameRules);
            }
        }

        public void MoveFileIntoSingerFolder(MusicFileNameInfo info,FileNameRules renameRule)
        {
            MoveFileIntoSingerFolder(info,renameRule, (src, dst) =>
             {
                 this.View.ViewDispathcer.Invoke(() =>
                 {
                     MoveRecordCollection.Add(new MoveRecordClass()
                     {
                         RecordSrcPath = src,
                         RecordDstPath = dst
                     });
                 });
             });
        }

        public void MoveFileIntoSingerFolder(MusicFileNameInfo info,FileNameRules renameRule,Action<string, string> MovedCallBack)
        {
            string dstDir = Path.Combine(info.FileDirName, info.SingerName);
            string dstFileName = string.Empty;
            switch (renameRule)
            {
                case FileNameRules.歌曲名:
                    dstFileName = $"{info.SongName}{info.Extension}";
                    break;
                case FileNameRules.歌手x歌曲名:
                    dstFileName = $"{info.SingerName}{FileNameSeparator.Separator}{info.SongName}{info.Extension}";
                    break;
                case FileNameRules.歌曲名x歌手:
                    dstFileName = $"{info.SongName}{FileNameSeparator.Separator}{info.SingerName}{info.Extension}";
                    break;
            }            
            string dst = Path.Combine(dstDir, dstFileName);
            FileManager.MoveFileIntoFolder(info.FileFullPath, dst, MovedCallBack,MovedFailCallBack);
        }

        private void MovedFailCallBack(string message)
        {
            this.View.ViewDispathcer.Invoke(() =>
            {
                this.View.SetSnackBarMessage(message);
            });
        }

        public List<string> MoveFilesInRule(List<MusicFileNameInfo> infoList, FileClassifyRules rule,FileNameRules renameRule, params string[] DirPath)
        {
            //TODO : 读取每个文件的信息，根据规则将文件移动到相应文件夹处，注意移动权限

            switch (rule)
            {
                case FileClassifyRules.不分文件夹:
                    break;

                case FileClassifyRules.按歌手分文件夹:
                    {
                        foreach (var item in infoList)
                        {
                            MoveFileIntoSingerFolder(item,renameRule);
                        }
                    }
                    break;

                case FileClassifyRules.按歌手x专辑分文件夹:
                    break;

                default:
                    break;
            }

            return null;
        }

        #region Commands

        public DelegateCommand SelectDirCommand { get; set; }

        public DelegateCommand MoveFilesCommand { get; set; }

        #endregion Commands
    }

    public static class NotifyPropertyChangedExtension
    {
        public static void MutateVerbose<TField>(this INotifyPropertyChanged instance, ref TField field, TField newValue, Action<PropertyChangedEventArgs> raise, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TField>.Default.Equals(field, newValue)) return;
            field = newValue;
            raise?.Invoke(new PropertyChangedEventArgs(propertyName));
        }
    }
}