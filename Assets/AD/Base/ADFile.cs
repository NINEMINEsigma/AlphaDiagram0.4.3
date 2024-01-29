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
        public bool IsEmpty { get; private set; } = false;
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
            if (this.IsKeepFileControl = isKeepFileControl)
            {
                FileStream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite);
            }
            if (isRefresh) UpdateFileData();
        }

        private void InitFileStream(bool isRefresh, Stream stream)
        {
            if (isRefresh) UpdateFileData(stream);
        }

        private void SetErrorStatus(Exception ex)
        {
            this.IsError = true;
            this.IsEmpty = true;
            this.ErrorException = ex;
            Timestamp = DateTime.UtcNow;
            IsSync = false;
        }

        public void UpdateFileData()
        {
            if (this.IsKeepFileControl)
            {
                UpdateFileData(FileStream);
            }
            else
            {
                using var nFileStream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite);
                {
                    UpdateFileData(nFileStream);
                }
            }
        }

        public void UpdateFileData(Stream stream)
        {
            FileData = new byte[stream.Length];
            byte[] buffer = new byte[256];
            int len, i = 0;
            while ((len = stream.Read(buffer, 0, buffer.Length)) != 0)
            {
                for (int j = 0; j < len; j++)
                {
                    FileData[i++] = buffer[j];
                }
            }
        }

        public AssetBundle LoadAssetBundle()
        {
            return AssetBundle.LoadFromMemory(FileData);
        }

        public T LoadObject<T>(bool isRefresh, Func<string, T> loader, System.Text.Encoding encoding)
        {
            if (isRefresh) UpdateFileData();
            string str = encoding.GetString(FileData);
            return loader(str);
        }

        public object LoadObject<T>(bool isRefresh, Func<string, object> loader, System.Text.Encoding encoding)
        {
            if (isRefresh) UpdateFileData();
            string str = encoding.GetString(FileData);
            return loader(str);
        }

        public T LoadObject<T>(bool isRefresh, Func<string, T> loader)
        {
            if (isRefresh) UpdateFileData();
            string str = System.Text.Encoding.Default.GetString(FileData);
            return loader(str);
        }

        public object LoadObject<T>(bool isRefresh, Func<string, object> loader)
        {
            if (isRefresh) UpdateFileData();
            string str = System.Text.Encoding.Default.GetString(FileData);
            return loader(str);
        }

        public string GetString(bool isRefresh, System.Text.Encoding encoding)
        {
            if (isRefresh) UpdateFileData();
            return encoding.GetString(FileData);
        }

    }
}

