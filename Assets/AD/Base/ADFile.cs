using System;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Unity.VisualScripting;
using AD.Utility;

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

        private bool isDelete = false;

        public void Delete()
        {
            FileStream?.Close();
            FileC.DeleteFile(FilePath);
            FileStream = null;
            IsError = false;
            IsEmpty = true;
            isDelete = true;
        }

        /// <summary>
        /// Just Use This ADFile Delete , You Can Use This Function To Create
        /// </summary>
        /// <param name="isRefresh"></param>
        /// <param name="isKeepFileControl"></param>
        /// <returns></returns>
        public bool Create(bool isRefresh = true, bool isKeepFileControl = true)
        {
            if (isDelete)
            {
                try
                {
                    if (File.Exists(FilePath))
                    {
                        Timestamp = File.GetLastWriteTime(FilePath).ToUniversalTime();
                    }
                    else File.Create(FilePath);
                    InitFileStream(isRefresh, isKeepFileControl);
                }
                catch (Exception ex)
                {
                    SetErrorStatus(ex);
                    return false;
                }
                return true;
            }
            return false;
        }

        ~ADFile()
        {
            FileStream?.Close();
        }

        public ADFile()
        {
            SetErrorStatus(new ADException("Empty"));
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
                else if (!isTryCreate)
                {
                    SetErrorStatus(new ADException("File Cannt Found"));
                    return;
                }
                else FileC.CreateFile(filePath);
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
                else if (!isTryCreate)
                {
                    SetErrorStatus(new ADException("File Cannt Found"));
                    return;
                }
                else FileC.CreateFile(filePath);
                IsSync = isSync;
                InitFileStream(isRefresh, stream);
            }
            catch (Exception ex)
            {
                SetErrorStatus(ex);
            }
        }

        public ADFile(bool isCanOverwrite, string filePath, bool isRefresh, bool isSync, bool isKeepFileControl)
        {
            try
            {
                FilePath = filePath;
                if (File.Exists(filePath))
                {
                    if (isCanOverwrite)
                    {
                        SetErrorStatus(new ADException("File Is Exists"));
                        return;
                    }
                }
                else FileC.CreateFile(filePath);
                Timestamp = File.GetLastWriteTime(filePath).ToUniversalTime();
                IsSync = isSync;
                InitFileStream(isRefresh, isKeepFileControl);
            }
            catch (Exception ex)
            {
                SetErrorStatus(ex);
            }
        }

        public ADFile(bool isCanOverwrite, string filePath, bool isRefresh, bool isSync, Stream stream)
        {
            try
            {
                FilePath = filePath;
                if (File.Exists(filePath))
                {
                    if (isCanOverwrite)
                    {
                        SetErrorStatus(new ADException("File Is Exists"));
                        return;
                    }
                }
                else FileC.CreateFile(filePath);
                Timestamp = File.GetLastWriteTime(filePath).ToUniversalTime();
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

        /// <summary>
        /// Text Mode
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isRefresh"></param>
        /// <param name="encoding"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Deserialize<T>(bool isRefresh, System.Text.Encoding encoding, out object obj)
        {
            try
            {
                string source = GetString(isRefresh, encoding);
                if (typeof(T).IsPrimitive)
                {
                    obj = typeof(T).GetMethod("Parse").Invoke(source, null);
                    return true;
                }
                else if (typeof(T).GetAttribute<SerializableAttribute>() != null)
                {
                    obj = JsonConvert.DeserializeObject<T>(source);
                    if (obj != null) return true;
                    else return false;
                }
            }
            catch (Exception ex)
            {
                SetErrorStatus(ex);
#if UNITY_EDITOR
                Debug.LogError("ADFile.Deserialize<T>(bool,Encoding) : T is " + typeof(T).FullName + " , is failed on " + FilePath);
                Debug.LogException(ex);
#endif
            }
            obj = default(T);
            return false;
        }

        /// <summary>
        /// Binary Stream Mode(Load From Immediate Current File)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="isText"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool Deserialize<T>(out object obj)
        {
            try
            {
                if (FileStream != null)
                {
                    obj = new BinaryFormatter().Deserialize(FileStream);
                }
                else
                {
                    using FileStream fs = new(FilePath, FileMode.Open);
                    obj = new BinaryFormatter().Deserialize(fs);
                }
                return obj != null;
            }
            catch (Exception ex)
            {
                SetErrorStatus(ex);
#if UNITY_EDITOR
                Debug.LogError("ADFile.Deserialize<T>() : T is " + typeof(T).FullName + " , is failed on " + FilePath);
                Debug.LogException(ex);
#endif
                obj = default(T);
                return false;
            }
        }

        public bool Serialize<T>(T obj, System.Text.Encoding encoding, bool isAllowSerializeAsBinary = true)
        {
            try
            {
                if (typeof(T).GetAttribute<SerializableAttribute>() == null)
                {
                    Debug.LogWarning("this type is not use SerializableAttribute but you now is try to serialize it");
                    if (!isAllowSerializeAsBinary) throw new ADException("Not Support");
                    using MemoryStream ms = new();
                    new BinaryFormatter().Serialize(ms, obj);
                    byte[] bytes = ms.GetBuffer();
                    File.WriteAllBytes(FilePath, bytes);
                    return true;
                }
                else
                {
                    if (FileStream == null)
                    {
                        File.WriteAllText(FilePath, JsonConvert.SerializeObject(obj, Formatting.Indented), encoding);
                        UpdateFileData();
                    }
                    else
                    {
                        this.FileData = encoding.GetBytes(JsonConvert.SerializeObject(obj, Formatting.Indented));
                        FileStream.Write(this.FileData, 0, this.FileData.Length);
                    }
                    return true;
                }
            }
            catch (Exception ex)
            {
                SetErrorStatus(ex);
#if UNITY_EDITOR
                Debug.LogError("ADFile.Deserialize<T>(bool,Encoding) : T is " + typeof(T).FullName + " , is failed on " + FilePath);
                Debug.LogException(ex);
#endif
                return false;
            }
        }

        public bool Serialize<T>(T obj)
        {
            try
            {
                using MemoryStream ms = new();
                new BinaryFormatter().Serialize(ms, obj);
                this.FileData = ms.GetBuffer();
                if (FileStream == null) File.WriteAllBytes(FilePath, FileData);
                else FileStream.Write(FileData, 0, FileData.Length);
                return true;
            }
            catch (Exception ex)
            {
                SetErrorStatus(ex);
#if UNITY_EDITOR
                Debug.LogError("ADFile.Deserialize<T>(bool,Encoding) : T is " + typeof(T).FullName + " , is failed on " + FilePath);
                Debug.LogException(ex);
#endif
                return false;
            }
        }

    }
}

