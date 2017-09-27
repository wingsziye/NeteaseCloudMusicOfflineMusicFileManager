using System;

namespace 文件目录整合
{
    public class FileNameSeparator
    {
        public static string Separator = " - ";
    }

    public enum FileNameRules
    {
        歌曲名,
        歌手x歌曲名,
        歌曲名x歌手
    }

    public enum FileClassifyRules
    {
        不分文件夹,
        按歌手分文件夹,
        按歌手x专辑分文件夹
    }

    public class MusicFileNameInfo
    {
        private string fileDirName;
        private string fileFullPath;
        private string fileNameWithoutExtension;
        private string fileName;
        private string singerName;
        private string songName;
        private string extension;

        public string FileDirName
        {
            get => fileDirName;
            set => fileDirName = value;
        }

        public string FileFullPath
        {
            get => fileFullPath;
            set => fileFullPath = value;
        }

        public string FileNameWithoutExtension
        {
            get => fileNameWithoutExtension;
            set
            {
                fileNameWithoutExtension = value;
            }
        }

        public string SingerName
        {
            get => singerName;
            private set
            {
                singerName = value;
            }
        }

        public string SongName
        {
            get => songName;
            private set
            {
                songName = value;
            }
        }

        public string FileName { get => fileName; set => fileName = value; }
        public string Extension { get => extension; set => extension = value; }

        //public void SetSingerAndSongName(string singerName,string songName)
        //{
        //    SetSSAction((a,b)=>{ a = singerName;b = songName; });//string为值传递，修改无变化
        //}

        //public void SetSSAction(Action<string,string> action)
        //{
        //    action( SingerName, SongName);//string为值传递，修改无变化
        //}

        public void SetSingerAndSongName(FileNameRules rule)
        {
            switch (rule)
            {
                case FileNameRules.歌曲名:
                    this.singerName = string.Empty;
                    this.SongName = this.FileNameWithoutExtension;
                    break;
                case FileNameRules.歌手x歌曲名:
                    {
                        var sp = this.FileNameWithoutExtension.Split(new string[] { FileNameSeparator.Separator }, StringSplitOptions.None);
                        this.SingerName = sp[0];
                        this.SongName = sp[1];
                    }                   
                    break;
                case FileNameRules.歌曲名x歌手:
                    {
                        var sp = this.FileNameWithoutExtension.Split(new string[] { FileNameSeparator.Separator }, StringSplitOptions.None);
                        this.SingerName = sp[1];
                        this.SongName = sp[0];
                    }
                    break;
            }

            SetSSAction((info) => { info.SingerName = singerName; info.SongName = songName; });
        }

        public void SetSSAction(Action<MusicFileNameInfo> action)
        {
            action(this);
        }
    }
}