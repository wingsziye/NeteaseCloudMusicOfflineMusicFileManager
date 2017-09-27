using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace 文件目录整合
{
    public class FileManager
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr _lopen(string lpPathName, int iReadWrite);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(IntPtr hObject);

        public const int OF_READWRITE = 2;
        public const int OF_SHARE_DENY_NONE = 0x40;
        public static readonly IntPtr HFILE_ERROR = new IntPtr(-1);

        public FileManager()
        {
        }

        public static void InitManager()
        {
            
        }

        public enum FileMoveRecord
        {
            Record,
            Undo
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="src">源文件完整路径，包含文件后缀</param>
        /// <param name="dst">目标完整路径，包含文件名和后缀</param>
        /// <param name="undoRecord"></param>
        public static void MoveFileIntoFolder(string src, string dst, Action<string, string> MovedSuccessCallBack, Action<string> MovedFailCallBack, FileMoveRecord undoRecord = FileMoveRecord.Record)
        {
            if (File.Exists(src))
            {
                if (FileManager.CheckFileIsNotUsing(src))
                {
                    var dstDir = Path.GetDirectoryName(dst);
                    if (!Directory.Exists(dstDir))
                    {
                        Directory.CreateDirectory(dstDir);
#if DEBUG
                        Console.WriteLine($"创建文件夹：{dstDir}");
#endif
                    }
                    try
                    {
                        //File.Move(src, dst);
                        File.Copy(src, dst);//先用copy试一下，免得不小心删错了
                        MovedSuccessCallBack(src, dst);
                        try
                        {
                            switch (undoRecord)
                            {
                                case FileMoveRecord.Record:
                                    break;

                                case FileMoveRecord.Undo:
                                    break;
                            }
                        }
                        catch (Exception)
                        {
                            throw;
                        }
                    }
                    catch (Exception ex)
                    {
                        MovedFailCallBack($"文件移动失败,异常信息:{ex.Message}\n移动路径{ src} -> { dst}");
#if DEBUG
                        //Console.WriteLine($"文件移动异常,{ex.Message}\n移动路径{ src} -> { dst}");
                        //throw;
#endif
                    }
                }
                else
                {
                    MovedFailCallBack($"文件正在被占用{src}");
                    Console.WriteLine($"文件正在被占用{src}");
                }
            }
#if DEBUG
            else
            {
                MovedFailCallBack($"源路径不存在,{src}");
            }
#endif
        }

        #region StaticFunctions

        /// <summary>
        /// 检查文件是否没被占用
        /// </summary>
        /// <param name="path"></param>
        /// <returns>true:没被占用.false:正被占用</returns>
        public static bool CheckFileIsNotUsing(string path)
        {
            IntPtr vHandle = _lopen(path, OF_READWRITE | OF_SHARE_DENY_NONE);
            if (vHandle == FileManager.HFILE_ERROR)
            {
                return false;
            }
            CloseHandle(vHandle);
            return true;
        }

        public static List<string> GetChildDirectorys(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                return GetChildDirectorys(new DirectoryInfo(dirPath));
            }
            else
            {
                return null;
            }
        }

        public static List<string> GetChildDirectorys(DirectoryInfo info)
        {
            List<string> nameList = new List<string>();

            var diries = info.GetDirectories("*", SearchOption.TopDirectoryOnly);

            foreach (var item in diries)
            {
                nameList.Add(item.FullName);
            }

            return nameList;
        }

        public static List<string> GetFileListInDirectory(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                return GetFileListInDirectory(new DirectoryInfo(dirPath));
            }
            else
            {
                return null;
            }
        }

        public static List<string> GetFileListInDirectory(DirectoryInfo info)
        {
            List<string> nameList = new List<string>();
            var fileNames = from fileName
                            in Directory.EnumerateFiles(info.FullName, $"*{FileNameSeparator.Separator}*.*", SearchOption.TopDirectoryOnly)
                            select fileName;
            //DirectoryInfo dir = new DirectoryInfo(dirPath);
            //var files = dir.GetFiles($"*{FileNameSeparator.Separator}*.*" , SearchOption.TopDirectoryOnly);

            foreach (var name in fileNames)
            {
                nameList.Add(name);
            }
            return nameList;
        }

        public static List<MusicFileNameInfo> GetMusicInfoInDirectory(string dirPath)
        {
            if (Directory.Exists(dirPath))
            {
                return GetMusicInfoInDirectory(new DirectoryInfo(dirPath));
            }
            else
            {
                return null;
            }
        }

        private static readonly string searchPattern =
            $"*{FileNameSeparator.Separator}*.mp3|*{FileNameSeparator.Separator}*.aac|*{FileNameSeparator.Separator}*.flac|*{FileNameSeparator.Separator}*.ape";

        public static List<MusicFileNameInfo> GetMusicInfoInDirectory(DirectoryInfo info)
        {
            List<MusicFileNameInfo> infoList = new List<MusicFileNameInfo>();

            var fileList = GetFileListInDirectory(info);

            var musicPaths = from path in fileList
                             where (path.ToLower().EndsWith(".mp3")
                                 || path.ToLower().EndsWith(".aac")
                                 || path.ToLower().EndsWith(".flac")
                                 || path.ToLower().EndsWith(".ape"))
                             select path;

            foreach (var path in musicPaths)
            {
                var m = new MusicFileNameInfo();
                m.FileDirName = Path.GetDirectoryName(path);
                m.FileFullPath = path;
                m.FileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
                m.FileName = Path.GetFileName(path);
                m.Extension = Path.GetExtension(path);
                infoList.Add(m);
            }
            return infoList;
        }

        #endregion StaticFunctions
    }
}