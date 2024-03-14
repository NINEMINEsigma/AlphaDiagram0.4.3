using System;
using System.IO;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections.Generic;

namespace AD.BASE
{
    [Serializable]
    public sealed class ADFile : IDisposable
    {
        public static implicit operator bool(ADFile file) => file.ErrorException == null;

        public string FilePath { get; private set; } = "";
        public DateTime Timestamp { get; private set; } = DateTime.UtcNow;
        public bool IsError { get; private set; } = false;
        public bool IsEmpty { get; private set; } = false;
        public Exception ErrorException { get; private set; } = null;
        public bool IsSync = false;
        public bool IsKeepFileControl { get; private set; } = false;

        private Stream FileStream;
        public byte[] FileData { get; private set; } = null;

        private bool isDelete = false;

        public void Delete()
        {
            this.Dispose();
            FileC.DeleteFile(FilePath);
            ErrorException = null;
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
            Dispose();
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
            Debug.LogException(ex);
        }

        private bool DebugMyself()
        {
            if (this.IsEmpty || this.ErrorException != null)
            {
                Debug.LogError("This File Was Drop in Error");
                Debug.LogException(ErrorException);
                return true;
            }
            return false;
        }

        public void UpdateFileData()
        {
            if (DebugMyself()) return;
            if (this.IsKeepFileControl)
            {
                UpdateFileData(FileStream);
            }
            else
            {
                using (var nFileStream = new FileStream(FilePath, FileMode.Open, FileAccess.ReadWrite))
                {
                    UpdateFileData(nFileStream);
                }
            }
        }

        public void UpdateFileData(Stream stream)
        {
            if (DebugMyself()) return;
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
            if (DebugMyself()) return null;
            return AssetBundle.LoadFromMemory(FileData);
        }

        public T LoadObject<T>(bool isRefresh, Func<string, T> loader, System.Text.Encoding encoding)
        {
            if (DebugMyself()) return default;
            if (isRefresh) UpdateFileData();
            string str = encoding.GetString(FileData);
            return loader(str);
        }

        public object LoadObject<T>(bool isRefresh, Func<string, object> loader, System.Text.Encoding encoding)
        {
            if (DebugMyself()) return null;
            if (isRefresh) UpdateFileData();
            string str = encoding.GetString(FileData);
            return loader(str);
        }

        public T LoadObject<T>(bool isRefresh, Func<string, T> loader)
        {
            if (DebugMyself()) return default;
            if (isRefresh) UpdateFileData();
            string str = System.Text.Encoding.Default.GetString(FileData);
            return loader(str);
        }

        public object LoadObject<T>(bool isRefresh, Func<string, object> loader)
        {
            if (DebugMyself()) return null;
            if (isRefresh) UpdateFileData();
            string str = System.Text.Encoding.Default.GetString(FileData);
            return loader(str);
        }

        public string GetString(bool isRefresh, System.Text.Encoding encoding)
        {
            if (DebugMyself()) return null;
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
            if (DebugMyself())
            {
                obj = ErrorException;
                return false;
            }
            string source = "";
            try
            {
                source = GetString(isRefresh, encoding);
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
                Debug.LogError("ADFile.Deserialize<T>(bool,Encoding) : T is " + typeof(T).FullName + " , is failed on " + FilePath + "\nsource : " + source);
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
            if (DebugMyself())
            {
                obj = ErrorException;
                return false;
            }
            try
            {
                if (IsKeepFileControl)
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
            if (DebugMyself())
            {
                return false;
            }
            try
            {
                if (typeof(T).GetAttribute<SerializableAttribute>() == null)
                {
                    Debug.LogWarning("this type is not use SerializableAttribute but you now is try to serialize it");
                    if (!isAllowSerializeAsBinary) throw new ADException("Not Support");
                    using MemoryStream ms = new();
                    new BinaryFormatter().Serialize(ms, obj);
                    FileData = ms.GetBuffer();
                    SaveFileData();
                    return true;
                }
                else
                {
                    if (IsKeepFileControl)
                    {
                        this.FileData = encoding.GetBytes(JsonConvert.SerializeObject(obj, Formatting.Indented));
                        FileStream.Write(this.FileData, 0, this.FileData.Length);
                    }
                    else
                    {
                        File.WriteAllText(FilePath, JsonConvert.SerializeObject(obj, Formatting.Indented), encoding);
                        UpdateFileData();
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
            if (DebugMyself())
            {
                return false;
            }
            try
            {
                using MemoryStream ms = new();
                new BinaryFormatter().Serialize(ms, obj);
                this.FileData = ms.GetBuffer();
                FileData = ms.GetBuffer();
                SaveFileData();
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

        public void Close()
        {
            if (IsKeepFileControl)
            {
                FileStream?.Close();
                FileStream?.Dispose();
                FileStream = null;
                IsKeepFileControl = false;
                IsError = false;
                IsEmpty = true;
            }
        }

        public void Keep()
        {
            if (!IsKeepFileControl)
            {
                Close();
                InitFileStream(false, true);
            }
        }

        public void Dispose()
        {
            this.Close();
            this.FileData = null;
        }

        public void Append(byte[] appendition)
        {
            byte[] newData = new byte[appendition.Length + FileData.Length];
            Array.Copy(FileData, 0, newData, 0, FileData.Length);
            Array.Copy(appendition, 0, newData, FileData.Length, appendition.Length);
            FileData = newData;
        }

        public void ReplaceAllData(byte[] data)
        {
            FileData = data;
        }

        public static byte[] ToBytes(object obj)
        {
            using MemoryStream ms = new();
            new BinaryFormatter().Serialize(ms, obj);
            return ms.GetBuffer();
        }

        public static object FromBytes(byte[] bytes)
        {
            using MemoryStream ms = new();
            ms.Write(bytes, 0, bytes.Length);
            ms.Position = 0;
            return new BinaryFormatter().Deserialize(ms);
        }

        public bool SaveFileData()
        {
            if (DebugMyself())
            {
                return false;
            }
            try
            {
                if (IsKeepFileControl)
                {
                    FileStream.Write(FileData, 0, FileData.Length);
                }
                else
                {
                    File.WriteAllBytes(FilePath, FileData);
                }
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
            return true;
        }
    }

    [Serializable]
    public class OfflineFile
    {
        public List<byte[]> MainMapDatas = new();
        public Dictionary<string, byte[]> SourceAssetsDatas = new();
        public Dictionary<string, string> PathRelayers = new();

        public void Add(ICanMakeOffline target)
        {
            MainMapDatas.Add(ADFile.ToBytes(target));
            foreach (var path in target.GetFilePaths())
            {
                if (!SourceAssetsDatas.ContainsKey(path))
                {
                    try
                    {
                        SourceAssetsDatas.Add(path, File.ReadAllBytes(path));
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }
            }
        }

        public void Add(object target)
        {
            if (target is ICanMakeOffline mO) Add(mO);
            else
            {
                MainMapDatas.Add(ADFile.ToBytes(target));
            }
        }

        public void Build(string path)
        {
            ADFile file = new(path, true, false, false, true);
            file.ReplaceAllData(ADFile.ToBytes(this));
            file.Dispose();
        }

        public static OfflineFile BuildFrom(string path)
        {
            return ADFile.FromBytes(File.ReadAllBytes(path)) as OfflineFile;
        }

        public void ReleaseFile(string directory)
        {
            foreach (var asset in SourceAssetsDatas)
            {
                string fileName = Path.GetFileName(asset.Key);
                ADFile file = new(Path.Combine(directory, fileName), true, false, false, true);
                file.ReplaceAllData(SourceAssetsDatas[asset.Key]);
                file.SaveFileData();
                file.Dispose();
                PathRelayers.Add(asset.Key, file.FilePath);
            }
        }

        /// <summary>
        /// Used After <see cref="ReleaseFile(string)"/>
        /// </summary>
        /// <param name="origin"></param>
        /// <returns></returns>
        public string GetNewPath(string origin)
        {
            return PathRelayers.TryGetValue(origin, out var path) ? path : null;
        }
    }

    public interface ICanMakeOffline
    {
        public string[] GetFilePaths();

        public void ReplacePath(Dictionary<string, string> sourceAssetsDatas);
    }
}

