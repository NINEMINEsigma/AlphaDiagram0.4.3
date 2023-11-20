using System;
using System.IO;
using UnityEngine;

namespace AD.BASE
{
    [Serializable]
    public sealed class ADFile
    {
        public string FilePath { get; private set; } = "";
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
        public bool IsError { get; private set; } = false;
        public bool IsEmpty { get; private set; } = true;
        public Exception ErrorException { get; private set; } = null;
        public bool IsSync = false;
        public bool IsKeepFileControl { get; private set; } = false;

        public Stream FileStream { get; private set; } = null;
        public byte[] FileData { get; private set; } = null;

        ~ADFile()
        {
            FileStream?.Close();
        }

        public ADFile()
        {

        }

        public ADFile(string filePath, bool isTryCreate, bool isRefresh, bool isSync, bool isKeepFileControl)
        {
            try
            {
                FilePath = filePath;
                if (File.Exists(filePath))
                {
                    Timestamp = File.GetLastWriteTime(filePath).ToUniversalTime();
                }
                else if (!isTryCreate) SetErrorStatus(new ADException("File Cannt Found"));
                else File.Create(filePath);
                IsSync = isSync;
                InitFileStream(isRefresh, isKeepFileControl);
            }
            catch (Exception ex)
            {
                SetErrorStatus(ex);
            }
        }

        public ADFile(string filePath, bool isTryCreate, bool isRefresh, bool isSync, Stream stream)
        {
            try
            {
                FilePath = filePath;
                if (File.Exists(filePath))
                {
                    Timestamp = File.GetLastWriteTime(filePath).ToUniversalTime();
                }
                else if (!isTryCreate) SetErrorStatus(new ADException("File Cannt Found"));
                else File.Create(filePath);
                IsSync = isSync;
                InitFileStream(isRefresh, stream);
            }
            catch (Exception ex)
            {
                SetErrorStatus(ex);
            }
        }

        private void InitFileStream(bool isRefresh, bool isKeepFileControl)
        {
            if ((IsKeepFileControl = isKeepFileControl))
            {
                FileStream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite);
                FileData = new byte[FileStream.Length];
                byte[] buffer = new byte[256];
                int len, i = 0;
                if (isRefresh)
                    while ((len = FileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        for (int j = 0; j < len; j++)
                        {
                            FileData[i++] = buffer[j];
                        }
                    }
            }
            else
            {
                using var nFileStream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite);
                FileData = new byte[nFileStream.Length];
                byte[] buffer = new byte[256];
                int len, i = 0;
                if (isRefresh)
                    while ((len = nFileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        for (int j = 0; j < len; j++)
                        {
                            FileData[i++] = buffer[j];
                        }
                    }
            }
        }

        private void InitFileStream(bool isRefresh,Stream stream)
        {
            FileData = new byte[stream.Length];
            byte[] buffer = new byte[256];
            int len, i = 0;
            if (isRefresh)
                while ((len = stream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    for (int j = 0; j < len; j++)
                    {
                        FileData[i++] = buffer[j];
                    }
                }
        }

        private void SetErrorStatus(Exception ex)
        {
            this.IsError = true;
            this.IsEmpty = true;
            this.ErrorException = ex;
            Timestamp = DateTime.UtcNow;
            IsSync = false;
        }

        public static string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        public static void DeleteFile(string filePath)
        {
            if (FileExists(filePath))
                File.Delete(filePath);
        }

        public static bool FileExists(string filePath) { return File.Exists(filePath); }
        public static void MoveFile(string sourcePath, string destPath) { File.Move(sourcePath, destPath); }
        public static void CopyFile(string sourcePath, string destPath) { File.Copy(sourcePath, destPath); }

        public static void MoveDirectory(string sourcePath, string destPath) { Directory.Move(sourcePath, destPath); }
        public static void CreateDirectory(string directoryPath) { Directory.CreateDirectory(directoryPath); }
        public static bool DirectoryExists(string directoryPath) { return Directory.Exists(directoryPath); }

        /*
		 * 	Given a path, it returns the directory that path points to.
		 * 	eg. "C:/myFolder/thisFolder/myFile.txt" will return "C:/myFolder/thisFolder".
		 */
        public static string GetDirectoryPath(string path, char seperator = '/')
        {
            //return Path.GetDirectoryName(path);
            // Path.GetDirectoryName turns forward slashes to backslashes in some cases on Windows, which is why
            // Substring is used instead.
            char slashChar = UsesForwardSlash(path) ? '/' : '\\';

            int slash = path.LastIndexOf(slashChar);
            // Ignore trailing slash if necessary.
            if (slash == (path.Length - 1))
                slash = path.Substring(0, slash).LastIndexOf(slashChar);
            if (slash == -1)
                Debug.LogError("Path provided is not a directory path as it contains no slashes.");
            return path.Substring(0, slash);
        }

        public static bool UsesForwardSlash(string path)
        {
            if (path.Contains("/"))
                return true;
            return false;
        }

        // Takes a directory path and a file or directory name and combines them into a single path.
        public static string CombinePathAndFilename(string directoryPath, string fileOrDirectoryName)
        {
            if (directoryPath[directoryPath.Length - 1] != '/' && directoryPath[directoryPath.Length - 1] != '\\')
                directoryPath += '/';
            return directoryPath + fileOrDirectoryName;
        }

        public static string[] GetDirectories(string path, bool getFullPaths = true)
        {
            var paths = Directory.GetDirectories(path);
            for (int i = 0; i < paths.Length; i++)
            {
                if (!getFullPaths)
                    paths[i] = Path.GetFileName(paths[i]);
                // GetDirectories sometimes returns backslashes, so we need to convert them to
                // forward slashes.
                paths[i].Replace("\\", "/");
            }
            return paths;
        }

        public static void DeleteDirectory(string directoryPath)
        {
            if (DirectoryExists(directoryPath))
                Directory.Delete(directoryPath, true);
        }

        public static string[] GetFiles(string path, bool getFullPaths = true)
        {
            var paths = Directory.GetFiles(path);
            if (!getFullPaths)
            {
                for (int i = 0; i < paths.Length; i++)
                    paths[i] = Path.GetFileName(paths[i]);
            }
            return paths;
        }

        public static byte[] ReadAllBytes(string path)
        {
            return File.ReadAllBytes(path);
        }

        public static void WriteAllBytes(string path, byte[] bytes)
        {
            File.WriteAllBytes(path, bytes);
        }
    }
}

