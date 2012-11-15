﻿using System;
using System.IO.IsolatedStorage;
using System.Text;

namespace ResourceLibrary
{
    public class ISOHelper
    {
        public static bool FileExists(string Filename)
        {
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                lock (iso)
                {
                    if(iso.FileExists(Filename))
                        return true;
                    else
                        return false;
                }
            }
        }

        public static bool DirectoryExists(string DirectoryName)
        {
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                lock (iso)
                {
                    if (iso.DirectoryExists(DirectoryName))
                        return true;
                    else
                        return false;
                }
            }
        }

        public static void MoveFileOverwrite(string FilenameSrc,string FilenameDst)
        {
            
            // create  a directory for the file in destination
            string[] filename_split = FilenameDst.Split('/');
            
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                lock (iso)
                {
                    if (! iso.DirectoryExists(filename_split[0]))
                        iso.CreateDirectory(filename_split[0]);

                    if (iso.FileExists(FilenameDst))
                        iso.DeleteFile(FilenameDst);
                    if (iso.FileExists(FilenameSrc))
                        iso.MoveFile(FilenameSrc, FilenameDst);
                }
            }
        }

        public static bool DeleteDirectory(String DirectoryName)
        {
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                lock (iso)
                {
                    if (!iso.DirectoryExists(DirectoryName))
                        return true;
                    var files = iso.GetFileNames(DirectoryName + "\\*");
                    foreach (string file in files)
                    {
                        try
                        {
                            iso.DeleteFile(DirectoryName + "\\" + file);
                        }
                        catch (Exception e)
                        {
                            ErrorLogging.Log("ISOHelper", e.Message, "ISOHelperError", "file :" + DirectoryName + "\\" + file);
                        }
                    }

                    files = iso.GetFileNames(DirectoryName + "\\*");
                    if (files.Length < 1)
                    {
                        iso.DeleteDirectory(DirectoryName);
                        return true;
                    }

                    return false;
                }
            }
        }

        public static bool DeleteFile(String FileName)
        {
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                lock (iso)
                {
                    if (!iso.FileExists(FileName))
                    {
                        return false;
                    }
                    else
                    {
                        iso.DeleteFile(FileName);
                        return true;
                    }
                }
            }
        }

        public static bool WriteToFile(string Filename, string Data)
        {
            try
            {
                using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    lock (iso)
                    {
                        string[] filename = Filename.Split('/');
                        if (!iso.DirectoryExists(filename[0]))
                            iso.CreateDirectory(filename[0]);
                        IsolatedStorageFileStream isf = new IsolatedStorageFileStream(Filename, System.IO.FileMode.Create, iso);
                        byte[] data = Encoding.UTF8.GetBytes(Data);
                        isf.Write(data, 0, data.Length);
                        isf.Close();
                    }
                }
                return true;
            }
            catch (Exception e)
            {
                ErrorLogging.Log("ISOHelper", e.Message, "ISOHelperError", Filename);
                return false;
            }
        }

        public static string ReadFromFile(string Filename)
        {
            string string_data = string.Empty;
            try
            {
                using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    lock (iso)
                    {
                        if (!FileExists(Filename))
                            return string.Empty;
                        IsolatedStorageFileStream isf = new IsolatedStorageFileStream(Filename, System.IO.FileMode.Open, iso);
                        byte[] data = new byte[(int)isf.Length];
                        isf.Read(data, 0, (int)isf.Length);
                        isf.Close();
                        string_data = Encoding.UTF8.GetString(data, 0, data.Length);
                    }
                }
            }
            catch (Exception e)
            {
                ErrorLogging.Log("ISOHelper", e.Message, "ISOHelperError", Filename);
            }
            return string_data;
        }

        public static void SaveDataUsage(long bytesReceived)
        {
            long totaldata = 0;
            long session_data = 0;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("total_data", out totaldata);
            totaldata += bytesReceived / 1024 / 1024;
            IsolatedStorageSettings.ApplicationSettings["total_data"] = totaldata;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("session_data", out session_data);
            session_data += bytesReceived / 1024 / 1024;
            IsolatedStorageSettings.ApplicationSettings["session_data"] = session_data;
            IsolatedStorageSettings.ApplicationSettings.Save();
        }
    }
}
