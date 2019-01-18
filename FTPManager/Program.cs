using FluentFTP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FTPManager
{
    class Program
    {
        static void Main(string[] args)
        {
            DeleteRepetiviveMusic();
            Console.WriteLine("all over if no thread");
            Console.Read();
            Console.Read();
        }

        static void DeleteRepetiviveMusic()
        {
            FtpClient client = new FtpClient("192.168.20.33", 2121, "mixadmin", "adminadmin");
            client.Connect();
            var serverList = GetFtpServerFileList(client);
            var deleteList = new List<MusicInfo>();
            var listcount = serverList.Count;
            var logFs = new FileStream(@"ftpLog.txt",FileMode.Append);
            var logWriter = new StreamWriter(logFs);
            try
            {
                for (int i = 0; i < listcount; i++)
                {
                    var info = serverList[i];
                    var unDeleteInfo = info;
                    for (int j = listcount - 1; j >= 0; j--)
                    {
                        var jinfo = serverList[j];
                        var jinfoUpname = jinfo.FileName.ToUpper().Trim();
                        var infoUpname = info.FileName.ToUpper().Trim();
                        if (jinfoUpname.Contains(infoUpname) || infoUpname.Contains(jinfoUpname))
                        {
                            if (jinfo == unDeleteInfo || jinfo.Extension == "flac" ||
                                jinfo.FileSize >= unDeleteInfo.FileSize)
                            {
                                unDeleteInfo = jinfo;
                                continue;
                            }

                            if (MoveToList(jinfo, serverList, deleteList))
                            {
                                listcount--;
                            }
                            else
                            {
                            }
                        }
                    }
                }
                using (var ts = File.CreateText(@"hasDeleteList.txt"))
                {
                    ts.WriteLine($"count={deleteList.Count}");
                    var tlist = new List<Task>();
                    foreach (var dfino in deleteList)
                    {
                        var w = $"{dfino.FullName} {dfino.FileSize}";
                        Console.WriteLine(w);
                        ts.WriteLine(w);
                        //var t = 
                            client.DeleteFile(@"/netease/cloudmusic/Music/" + dfino.FullName);
                        //tlist.Add(t);
                    }
                    Task.WaitAll(tlist.ToArray());
                }
            }
            catch (Exception e)
            {
                logWriter.WriteLine(e.StackTrace);
                logWriter.Close();
                Console.WriteLine(e);
            }
            
        }

        static bool MoveToList<T>(T msg, List<T> src, List<T> dst)
        {
            if (src.Contains(msg))
            {
                src.Remove(msg);
                dst.Add(msg);
                return true;
            }
            return false;
        }


        static List<MusicInfo> GetFtpServerFileList(FtpClient client)
        {
            if (client==null)
            {
                client = new FtpClient("192.168.20.33", 2121, "mixadmin", "adminadmin");
            }

            // if you don't specify login credentials, we use the "anonymous" user account
            //client.Credentials = new NetworkCredential("david", "pass123");

            // begin connecting to the server
            client.Encoding = Encoding.UTF8;
            if (!client.IsConnected&&!client.IsDisposed)
            {
                client.Connect();
            }

            List<MusicInfo> nameList = new List<MusicInfo>();
            // get a list of files and directories in the "/htdocs" folder
            foreach (FtpListItem item in client.GetListing(@"/netease/cloudmusic/Music/"))
            {
                nameList.Add(new MusicInfo(item.Name, item.Size));
            }
            return nameList;
        }

        static void DeleteAndUpdateFile()
        {
            // create an FTP client
            FtpClient client = new FtpClient("192.168.22.101", 2121, "mixadmin", "adminadmin");

            // if you don't specify login credentials, we use the "anonymous" user account
            //client.Credentials = new NetworkCredential("david", "pass123");

            // begin connecting to the server
            client.Encoding = Encoding.UTF8;
            client.Connect();

            List<string> nameList = new List<string>();
            // get a list of files and directories in the "/htdocs" folder
            foreach (FtpListItem item in client.GetListing(@"/netease/cloudmusic/Music/"))
            {
                nameList.Add(item.Name);
            }

            string cpyDirPath = @"U:\TempFiles\CopyMsc";


            var cpyFileNames = from fileName
                in Directory.EnumerateFiles(cpyDirPath, "*", SearchOption.TopDirectoryOnly)
                select fileName;

            List<string> deleteList = new List<string>();
            foreach (var item in cpyFileNames)
            {
                var cpyname = Path.GetFileName(item);
                if (nameList.Contains(cpyname))
                {
                    Console.WriteLine(item);
                    deleteList.Add(item);
                }
            }
            Console.WriteLine($"共需删除{deleteList.Count}个文件");
            for (int i = 0; i < deleteList.Count; i++)
            {
                try
                {
                    File.Delete(deleteList[i]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{deleteList[i]}删除出错");
                }
            }

            Console.WriteLine();
            Console.WriteLine("全部删除完毕");


            #region 上传文件

            List<string> uploadList = new List<string>();
            foreach (var item in cpyFileNames)
            {
                uploadList.Add(item);
            }
            Console.WriteLine($"共需copy{uploadList.Count}个文件");
            int ucount = 0;
            foreach (var item in uploadList)
            {
                string dst = "/netease/cloudmusic/Music/" + Path.GetFileName(item);
                // upload a file and retry 3 times before giving up
                try
                {
                    client.RetryAttempts = 3;
                    Console.WriteLine("正在上传第" + ucount + "个");
                    if (client.UploadFile(item, dst, FtpExists.Overwrite, false, FtpVerify.Retry))
                    {
                        ucount++;
                    }
                    else
                    {
                        Console.WriteLine("第{0}个上传失败,{1}", ucount, item);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }

            }


            #endregion
        }

        class MusicInfo
        {
            private long fileSize;
            private string fullName;

            public string FileName
            {
                get { return Path.GetFileNameWithoutExtension(fullName); }
            }

            public string Extension
            {
                get { return Path.GetExtension(fullName);}
            }

            public string FullName
            {
                get { return fullName; }
                set { fullName = value; }
            }

            public long FileSize
            {
                get { return fileSize; }
                set { fileSize = value; }
            }

            public MusicInfo() { }

            public MusicInfo(string fullName,long size)
            {
                this.FullName = fullName;
                this.FileSize = size;
            }
        }
    }
}
