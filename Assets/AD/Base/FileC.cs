using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace AD.BASE
{
    public static class FileC
    {
        #region a

        //获取成这个文件的文件路径（不包括本身）
        public static DirectoryInfo GetDirectroryOfFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ADException("FileC.TryCreateDirectroryOfFile arg : filePath is null or empty");
            var dir_name = Path.GetDirectoryName(filePath);
            if (Directory.Exists(dir_name))
            {
                return Directory.GetParent(dir_name);
            }
            return null;
        }

        //生成这个文件的文件路径（不包括本身）
        public static bool TryCreateDirectroryOfFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ADException("FileC.TryCreateDirectroryOfFile arg : filePath is null or empty");
            var dir_name = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir_name))
            {
                Directory.CreateDirectory(dir_name);
                return false;
            }
            else return true;
        }

        public static void ReCreateDirectroryOfFile(string filePath, bool recursive = true)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ADException("FileC.TryCreateDirectroryOfFile arg : filePath is null or empty");
            var dir_name = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir_name))
            {
                Directory.CreateDirectory(dir_name);
            }
            else
            {
                Directory.Delete(dir_name, recursive);
                Directory.CreateDirectory(dir_name);
            }
        }

        //生成这个文件的文件路径（不包含本身）
        public static DirectoryInfo CreateDirectroryOfFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ADException("FileC.TryCreateDirectroryOfFile arg : filePath is null or empty");
            var dir_name = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(dir_name))
            {
                return Directory.CreateDirectory(dir_name);
            }
            else
            {
                return Directory.GetParent(dir_name);
            }
        }

        //移动整个路径
        public static void MoveFolder(string sourcePath, string destPath)
        {
            if (Directory.Exists(sourcePath))
            {
                if (!Directory.Exists(destPath))
                {
                    //目标目录不存在则创建 
                    try
                    {
                        Directory.CreateDirectory(destPath);
                    }
                    catch (Exception ex)
                    {
                        //throw new Exception(" public static void MoveFolder(string sourcePath, string destPath),Target Directory fail to create" + ex.Message);
                        Debug.LogWarning("public static void MoveFolder(string sourcePath, string destPath),Target Directory fail to create" + ex.Message);
                        return;
                    }
                }
                //获得源文件下所有文件 
                List<string> files = new(Directory.GetFiles(sourcePath));
                files.ForEach(c =>
                {
                    string destFile = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                    //覆盖模式 
                    if (File.Exists(destFile))
                    {
                        File.Delete(destFile);
                    }
                    File.Move(c, destFile);
                });
                //获得源文件下所有目录文件 
                List<string> folders = new List<string>(Directory.GetDirectories(sourcePath));

                folders.ForEach(c =>
                {
                    string destDir = Path.Combine(new string[] { destPath, Path.GetFileName(c) });
                    //Directory.Move必须要在同一个根目录下移动才有效，不能在不同卷中移动。 
                    //Directory.Move(c, destDir); 

                    //采用递归的方法实现 
                    MoveFolder(c, destDir);
                });
            }
            else
            {
                //throw new Exception(" public static void MoveFolder(string sourcePath, string destPath),sourcePath cannt find");
                Debug.Log("public static void MoveFolder(string sourcePath, string destPath),sourcePath cannt find");
            }
        }

        //拷贝整个路径
        public static void CopyFilefolder(string sourceFilePath, string targetFilePath)
        {
            //获取源文件夹中的所有非目录文件
            string[] files = Directory.GetFiles(sourceFilePath);
            string fileName;
            string destFile;
            //如果目标文件夹不存在，则新建目标文件夹
            if (!Directory.Exists(targetFilePath))
            {
                Directory.CreateDirectory(targetFilePath);
            }
            //将获取到的文件一个一个拷贝到目标文件夹中 
            foreach (string s in files)
            {
                fileName = Path.GetFileName(s);
                destFile = Path.Combine(targetFilePath, fileName);
                File.Copy(s, destFile, true);
            }
            //上面一段在MSDN上可以看到源码 

            //获取并存储源文件夹中的文件夹名
            string[] filefolders = Directory.GetFiles(sourceFilePath);
            //创建Directoryinfo实例 
            DirectoryInfo dirinfo = new DirectoryInfo(sourceFilePath);
            //获取得源文件夹下的所有子文件夹名
            DirectoryInfo[] subFileFolder = dirinfo.GetDirectories();
            for (int j = 0; j < subFileFolder.Length; j++)
            {
                //获取所有子文件夹名 
                string subSourcePath = sourceFilePath + "\\" + subFileFolder[j].ToString();
                string subTargetPath = targetFilePath + "\\" + subFileFolder[j].ToString();
                //把得到的子文件夹当成新的源文件夹，递归调用CopyFilefolder
                CopyFilefolder(subSourcePath, subTargetPath);
            }
        }

        //重命名文件
        public static void FileRename(string sourceFile, string newNameWithFullPath)
        {
            CopyFile(sourceFile, newNameWithFullPath);
            DeleteFile(sourceFile);
        }

        /*public static /*ExecutionResult void FileRename(string sourceFile, string destinationPath, string destinationFileName)
        {
            //ExecutionResult result;
            FileInfo tempFileInfo;
            FileInfo tempBakFileInfo;
            DirectoryInfo tempDirectoryInfo;

            //result = new ExecutionResult();
            tempFileInfo = new FileInfo(sourceFile);
            tempDirectoryInfo = new DirectoryInfo(destinationPath);
            tempBakFileInfo = new FileInfo(destinationPath + "\\" + destinationFileName);
            try
            {
                if (!tempDirectoryInfo.Exists)
                    tempDirectoryInfo.Create();
                if (tempBakFileInfo.Exists)
                    tempBakFileInfo.Delete();
                //move file to bak
                tempFileInfo.MoveTo(destinationPath + "\\" + destinationFileName);

            //    result.Status = true;
            //    result.Message = "Rename file OK";
            //    result.Anything = "OK";
            }
            catch (Exception ex)
            {
                //    result.Status = false;
                //    result.Anything = "Mail";
                //   result.Message = ex.Message;
                //    if (mesLog.IsErrorEnabled)
                //    {
                //        mesLog.Error(MethodBase.GetCurrentMethod().Name, "Rename file error. Msg :" + ex.Message);
                //        mesLog.Error(ex.StackTrace);
                //    }
                Debug.LogWarning(MethodBase.GetCurrentMethod().Name + "Rename file error. Msg :" + ex.Message);
            }

           // return result;
        }*/

        private static Dictionary<string, List<FileInfo>> Files = new();

        public static void ClearAllFiles()
        {
            Files.Clear();
        }

        public static bool TryGetFiles(string group, out List<FileInfo> fileInfos)
        {
            return Files.TryGetValue(group, out fileInfos);
        }

        public static List<FileInfo> GetFiles(string group)
        {
            Files.TryGetValue(group, out var files);
            return files;
        }

        public static void LoadFiles(string group, string dictionary, Predicate<string> _Right)
        {
            if (Files.ContainsKey(group))
                Files[group] = Files[group].Union(FindAll(dictionary, _Right)).ToList();
            else
            {
                var result = FindAll(dictionary, _Right);
                if (result != null)
                    Files[group] = result;
            }
        }

        /// <summary>
        /// 获取该文件夹下顶部文件夹子项中使用该扩展名的文件
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="extension"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static List<FileInfo> FindAll(string dictionary, string extension, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            DirectoryInfo direction = new(dictionary);
            if (extension[0] == '.') extension = "*" + extension[1..];
            else if (extension[0] != '*') extension = "*" + extension;
            FileInfo[] files = direction.GetFiles(extension, searchOption);
            List<FileInfo> result = new();
            foreach (var it in files) result.Add(it);
            return result.Count != 0 ? result : null;
        }

        /// <summary>
        /// 获取该文件夹下顶部文件夹子项中匹配的部分（默认）
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="_Right"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static List<FileInfo> FindAll(string dictionary, Predicate<string> _Right, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            DirectoryInfo direction = new(dictionary);
            FileInfo[] files = direction.GetFiles("*", searchOption);
            List<FileInfo> result = new();
            foreach (var it in files)
                if (_Right(it.Name)) result.Add(it);
            return result.Count != 0 ? result : null;
        }

        /// <summary>
        /// 获取该文件夹下全部子项（默认）
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="searchOption"></param>
        /// <returns></returns>
        public static List<FileInfo> FindAll(string dictionary, SearchOption searchOption = SearchOption.AllDirectories)
        {
            DirectoryInfo direction = new(dictionary);
            FileInfo[] files = direction.GetFiles("*", searchOption);
            List<FileInfo> result = new();
            foreach (var it in files) result.Add(it);
            return result.Count != 0 ? result : null;
        }

        public static FileInfo First(DirectoryInfo direction, string name)
        {
            return First(direction, T => Path.GetFileNameWithoutExtension(T) == name);
        }

        public static FileInfo First(string dictionary, string name)
        {
            return First(dictionary, T => Path.GetFileNameWithoutExtension(T) == name);
        }

        public static FileInfo First(string dictionary, Predicate<string> _Right)
        {
            DirectoryInfo direction = new(dictionary);
            FileInfo[] files = direction.GetFiles("*");
            foreach (var it in files)
                if (_Right(it.Name)) return it;
            return null;
        }

        public static FileInfo First(DirectoryInfo direction, Predicate<string> _Right)
        {
            FileInfo[] files = direction.GetFiles("*");
            foreach (var it in files)
                if (_Right(it.Name)) return it;
            return null;
        }

        public static AssetBundle LoadAssetBundle(string path)
        {
            return AssetBundle.LoadFromFile(path);
        }

        public static AssetBundle LoadAssetBundle(string path, params string[] targetsName)
        {
            AssetBundle asset = AssetBundle.LoadFromFile(path);
            foreach (var item in targetsName)
            {
                asset.LoadAsset(item);
            }
            return asset;
        }

        /// <summary>
        /// 分段，断点下载文件
        /// </summary>
        /// <param name="loadPath">下载地址</param>
        /// <param name="savePath">保存路径</param>
        /// <returns></returns>
        public static IEnumerator BreakpointResume(MonoBehaviour sendObject, string loadPath, string savePath, double loadedBytes, UnityAction<float, string> callback)
        {
            UnityWebRequest headRequest = UnityWebRequest.Head(loadPath);
            yield return headRequest.SendWebRequest();

            if (!string.IsNullOrEmpty(headRequest.error))
            {
                callback(-1, headRequest.error + ":cannt found the file");
                yield break;
            }
            headRequest.Dispose();
            using UnityWebRequest Request = UnityWebRequest.Get(loadPath);
            //append设置为true文件写入方式为接续写入，不覆盖原文件。
            Request.downloadHandler = new DownloadHandlerFile(savePath, true);
            FileInfo file = new FileInfo(savePath);

            //请求网络数据从第fileLength到最后的字节；
            Request.SetRequestHeader("Range", "bytes=" + file.Length + "-");

            if (Request.downloadProgress<1)
            {
                Request.SendWebRequest();
                while (!Request.isDone)
                {
                    callback(Request.downloadProgress * 100, "%");
                    //超过一定的字节关闭现在的协程，开启新的协程，将资源分段下载
                    if (Request.downloadedBytes >= loadedBytes)
                    {
                        sendObject.StopCoroutine(nameof(BreakpointResume));

                        //如果 UnityWebRequest 在进行中，就停止。
                        Request.Abort();
                        if (!string.IsNullOrEmpty(headRequest.error))
                        {
                            callback(0, headRequest.error + ":failed");
                            yield break;
                        }
                        yield return sendObject.StartCoroutine(BreakpointResume(sendObject, loadPath, savePath, loadedBytes, callback));
                    }
                    yield return null;
                }
            }
            if (string.IsNullOrEmpty(Request.error)) callback(1, "succeed");
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public class OpenFileName
        {
            public int structSize = 0;
            public IntPtr dlgOwner = IntPtr.Zero;
            public IntPtr instance = IntPtr.Zero;
            public string filter = null;
            public string customFilter = null;
            public int maxCustFilter = 0;
            public int filterIndex = 0;
            public string file = null;
            public int maxFile = 0;
            public string fileTitle = null;
            public int maxFileTitle = 0;
            public string initialDir = null;
            public string title = null;
            public int flags = 0;
            public short fileOffset = 0;
            public short fileExtension = 0;
            public string defExt = null;
            public IntPtr custData = IntPtr.Zero;
            public IntPtr hook = IntPtr.Zero;
            public string templateName = null;
            public IntPtr reservedPtr = IntPtr.Zero;
            public int reservedInt = 0;
            public int flagsEx = 0;
        }

        public class LocalDialog
        {
            [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
            public static extern bool GetOpenFileName([In, Out] OpenFileName ofn);
            public static bool GetOFN([In, Out] OpenFileName ofn)
            {
                return GetOpenFileName(ofn);
            }

            [DllImport("Comdlg32.dll", SetLastError = true, ThrowOnUnmappableChar = true, CharSet = CharSet.Auto)]
            public static extern bool GetSaveFileName([In, Out] OpenFileName ofn);
            public static bool GetSFN([In, Out] OpenFileName ofn)
            {
                return GetSaveFileName(ofn);
            }
        }

        public static OpenFileName SelectFileOnSystem(string labelName, string subLabelName, params string[] fileArgs)
        {
            OpenFileName targetFile = new OpenFileName();
            targetFile.structSize = Marshal.SizeOf(targetFile);
            targetFile.filter = labelName + "(*" + subLabelName + ")\0";
            for (int i = 0; i < fileArgs.Length - 1; i++)
            {
                targetFile.filter += "*." + fileArgs[i] + ";";
            }
            if (fileArgs.Length > 0) targetFile.filter += "*." + fileArgs[^1] + ";\0";
            targetFile.file = new string(new char[256]);
            targetFile.maxFile = targetFile.file.Length;
            targetFile.fileTitle = new string(new char[64]);
            targetFile.maxFileTitle = targetFile.fileTitle.Length;
            targetFile.initialDir = Application.streamingAssetsPath.Replace('/', '\\');//默认路径
            targetFile.title = "Select A Song";
            targetFile.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000008;
            return targetFile;
        }

        public static OpenFileName SelectFileOnSystem(UnityAction<string> action, string labelName, string subLabelName, params string[] fileArgs)
        {
            OpenFileName targetFile = SelectFileOnSystem(labelName, subLabelName, fileArgs);
            if (LocalDialog.GetOpenFileName(targetFile) && targetFile.file != "")
            {
                action(targetFile.file);
            }
            return targetFile;
        }

        public static bool IsAbsolute(this string path)
        {
            if (path.Length > 0 && (path[0] == '/' || path[0] == '\\'))
                return true;
            if (path.Length > 1 && path[1] == ':')
                return true;
            return false;
        }

        public static Stream CreateFileStream(string FilePath, bool isWriteStream, int bufferSize = 1024, bool IsTurnIntoGZipStream = false)
        {
            Stream stream = null;
            // Check that the path is in a valid format. This will throw an exception if not.
            new FileInfo(FilePath);

            try
            {
                // There's no point in creating an empty MemoryStream if we're only reading from it.
                if (!isWriteStream)
                    return null;
                stream = new MemoryStream(bufferSize);
                return CreateStream(stream, isWriteStream, IsTurnIntoGZipStream);
            }
            catch (Exception ex)
            {
                stream?.Dispose();
                throw ex;
            }
        }

        public static Stream CreatePlayerPrefsStream(string FilePath, bool isWriteStream, bool isAppend, int bufferSize = 1024, bool IsTurnIntoGZipStream = false)
        {
            Stream stream = null;

            // Check that the path is in a valid format. This will throw an exception if not.
            new FileInfo(FilePath);

            try
            {
                if (isWriteStream)
                    stream = new ADPlayerPrefsStream(FilePath, bufferSize, isAppend);
                else
                {
                    if (!PlayerPrefs.HasKey(FilePath))
                        return null;
                    stream = new ADPlayerPrefsStream(FilePath);
                }
                return CreateStream(stream, isWriteStream, IsTurnIntoGZipStream);
            }
            catch (Exception ex)
            {
                stream?.Dispose();
                throw ex;
            }
        }

        public static Stream CreateResourcesStream(string FilePath, bool isWriteStream)
        {
            Stream stream = null;

            // Check that the path is in a valid format. This will throw an exception if not.
            new FileInfo(FilePath);

            try
            {
                if (!isWriteStream)
                {
                    var resourcesStream = new ADResourcesStream(FilePath);
                    if (resourcesStream.Exists)
                        stream = resourcesStream;
                    else
                    {
                        resourcesStream.Dispose();
                        return null;
                    }
                }
                else if (UnityEngine.Application.isEditor)
                    throw new System.NotSupportedException("Cannot write directly to Resources folder." +
                        " Try writing to a directory outside of Resources, and then manually move the file there.");
                else
                    throw new System.NotSupportedException("Cannot write to Resources folder at runtime." +
                        " Use a different save location at runtime instead.");
                return CreateStream(stream, isWriteStream, false);
            }
            catch (System.Exception e)
            {
                if (stream != null)
                    stream.Dispose();
                throw e;
            }
        }

        public static Stream CreateStream(Stream stream, bool isWriteStream, bool IsTurnIntoGZipStream)
        {
            try
            {
                if (IsTurnIntoGZipStream && stream.GetType() != typeof(GZipStream))
                {
                    stream = isWriteStream ? new GZipStream(stream, CompressionMode.Compress) : new GZipStream(stream, CompressionMode.Decompress);
                }

                return stream;
            }
            catch (System.Exception e)
            {
                stream?.Dispose();
                if (e.GetType() == typeof(System.Security.Cryptography.CryptographicException))
                    throw new System.Security.Cryptography.CryptographicException("Could not decrypt file." +
                        " Please ensure that you are using the same password used to encrypt the file.");
                else throw e;
            }
        }

        public static void CopyTo(Stream source, Stream destination)
        {
#if UNITY_2019_1_OR_NEWER
            source.CopyTo(destination);
#else
            byte[] buffer = new byte[2048];
            int bytesRead;
            while ((bytesRead = source.Read(buffer, 0, buffer.Length)) > 0)
                destination.Write(buffer, 0, bytesRead);
#endif
        }

        #endregion

        #region S

        public static string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        public static void DeleteFile(string filePath)
        {
            if (FileExists(filePath))
                File.Delete(filePath);
        }

        public static void CreateFile(string filePath)
        {
            File.Create(filePath).Close();
        }

        public static void CreateFile(string filePath, out FileStream fileStream)
        {
            fileStream = File.Create(filePath);
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

        #endregion
    }
}
