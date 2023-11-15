using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

namespace AD.BASE
{
    public static class FileC
    { 
        //获取成这个文件的文件路径（不包括本身）
        public static DirectoryInfo GetDirectroryOfFile(string filePath)
        {
            Debug.Log($"CreateDirectrory {filePath}[folder_path],");
            if (!string.IsNullOrEmpty(filePath))
            {
                var dir_name = Path.GetDirectoryName(filePath);
                if (Directory.Exists(dir_name))
                {
                    Debug.Log($"Exists {dir_name}[dir_name],");
                    return Directory.GetParent(dir_name);
                }
            }
            return null;
        }

        //生成这个文件的文件路径（不包括本身）
        public static void TryCreateDirectroryOfFile(string filePath)
        {
            Debug.Log($"CreateDirectrory {filePath}[folder_path],");
            if (!string.IsNullOrEmpty(filePath))
            {
                var dir_name = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir_name))
                {
                    Debug.Log($"No Exists {dir_name}[dir_name],");
                    Directory.CreateDirectory(dir_name);
                }
                else
                {
                    Debug.Log($"Exists {dir_name}[dir_name],");
                }
            }
        }

        //生成这个文件的文件路径（不包含本身）
        public static DirectoryInfo CreateDirectroryOfFile(string filePath)
        {
            Debug.Log($"CreateDirectrory {filePath}[folder_path],");
            if (!string.IsNullOrEmpty(filePath))
            {
                var dir_name = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(dir_name))
                {
                    Debug.Log($"No Exists {dir_name}[dir_name],");
                    return Directory.CreateDirectory(dir_name);
                }
                else
                {
                    Debug.Log($"Exists {dir_name}[dir_name],");
                    return Directory.GetParent(dir_name);
                }
            }
            return null;
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

        public static void CopyFile(string sourceFile, string targetFilePath)
        {
            File.Copy(sourceFile, targetFilePath, true);
        }
        public static void DeleteFile(string sourceFile)
        {
            File.Delete(sourceFile);
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

        public static List<FileInfo> FindAll(string dictionary,string extension)
        {
            return FindAll(dictionary, T => Path.GetExtension(T) == extension);
        }

        public static List<FileInfo> FindAll(string dictionary, Predicate<string> _Right)
        {
            DirectoryInfo direction = new(dictionary);
            FileInfo[] files = direction.GetFiles("*");
            List<FileInfo> result = new();
            foreach (var it in files)
                if (_Right(it.Name)) result.Add(it);
            return result.Count != 0 ? result : null;
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
         
        public static AssetBundle LoadAssetBundle(string path)
        { 
            return AssetBundle.LoadFromFile(path); ;
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


        public static bool BreakpointResume(this ICanBreakpointResume self, string loadPath, string savePath, double loadedBytes, UnityAction<string> callback)
        {
            if (self.As<MonoBehaviour>(out var result))
            {
                result.StartCoroutine(BreakpointResume(result, loadPath, savePath, loadedBytes, callback));
                return true;
            }
            else return false;
        }

        /// <summary>
        /// 分段，断点下载文件
        /// </summary>
        /// <param name="loadPath">下载地址</param>
        /// <param name="savePath">保存路径</param>
        /// <returns></returns>
        public static IEnumerator BreakpointResume(MonoBehaviour sendObject, string loadPath, string savePath, double loadedBytes, UnityAction<string> callback)
        {
            //UnityWebRequest 经配置可传输 HTTP HEAD 请求的 UnityWebRequest。
            UnityWebRequest headRequest = UnityWebRequest.Head(loadPath);
            //开始与远程服务器通信。
            yield return headRequest.SendWebRequest();

            if (!string.IsNullOrEmpty(headRequest.error))
            {
                callback(headRequest.error + ":cannt found the file");
                yield break;
            }
            //获取文件总大小
            ulong totalLength = ulong.Parse(headRequest.GetResponseHeader("Content-Length"));
            Debug.Log("获取大小" + totalLength);
            headRequest.Dispose();
            UnityWebRequest Request = UnityWebRequest.Get(loadPath);
            //append设置为true文件写入方式为接续写入，不覆盖原文件。
            Request.downloadHandler = new DownloadHandlerFile(savePath, true);
            //创建文件
            FileInfo file = new FileInfo(savePath);
            //当前下载的文件长度
            ulong fileLength = (ulong)file.Length;

            //请求网络数据从第fileLength到最后的字节；
            Request.SetRequestHeader("Range", "bytes=" + fileLength + "-");

            if (!string.IsNullOrEmpty(headRequest.error))
            {
                callback(headRequest.error + ":failed");
                yield break;
            }
            if (fileLength < totalLength)
            {
                Request.SendWebRequest();
                while (!Request.isDone)
                {
                    double progress = (Request.downloadedBytes + fileLength) / (double)totalLength;
                    callback((progress * 100 + 0.01f).ToString("f2") + "%");
                    // Debug.Log("下载量" + Request.downloadedBytes);
                    //超过一定的字节关闭现在的协程，开启新的协程，将资源分段下载
                    if (Request.downloadedBytes >= loadedBytes)
                    {
                        sendObject.StopCoroutine("BreakpointResume");

                        //如果 UnityWebRequest 在进行中，就停止。
                        Request.Abort();
                        if (!string.IsNullOrEmpty(headRequest.error))
                        {
                            callback(headRequest.error + ":failed");
                            yield break;
                        }
                        yield return sendObject.StartCoroutine(BreakpointResume(sendObject, loadPath, savePath, loadedBytes, callback));
                    }
                    yield return null;
                }
            }
            if (string.IsNullOrEmpty(Request.error))
            {
                Debug.Log("下载成功" + savePath);
                callback("succeed");
            }
            //表示不再使用此 UnityWebRequest，并且应清理它使用的所有资源。
            Request.Dispose();
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
            OpenFileName targetFile = SelectFileOnSystem(labelName,subLabelName,fileArgs);
            if (LocalDialog.GetOpenFileName(targetFile) && targetFile.file != "")
            {
                action(targetFile.file);
            }
            return targetFile;
        }
    }

    public interface ICanBreakpointResume
    {

    }
}
