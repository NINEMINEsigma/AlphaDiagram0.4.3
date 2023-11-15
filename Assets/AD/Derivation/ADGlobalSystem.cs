using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using AD.BASE;
using AD.UI;
using AD.Utility;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem.Controls;

namespace AD
{
    public class RegisterInfo
    {
        public RegisterInfo(List<ButtonControl> buttons, UnityAction action, PressType type)
        {
            this.buttons = buttons;
            this._action = action;
            this.type = type;
        }
        ~RegisterInfo()
        {
            this.UnRegister();
        }

        public void UnRegister()
        {
            ADGlobalSystem.RemoveListener(buttons, action, type);
        }

        public void TryRegister()
        {
            ADGlobalSystem.AddListener(buttons, action, type);
        }

        private bool _state = true;
        public bool state
        {
            get
            {
                return _state;
            }
            set
            {
                if (_state != value)
                {
                    if (value) TryRegister();
                    else UnRegister();
                }
                _state = value;
            }
        }

        private List<ButtonControl> buttons = new List<ButtonControl>();
        public UnityAction _action;
        public UnityAction action
        {
            get => _action;
            set
            {
                if (state) TryRegister();
                Debug.LogWarning("you try to reset this mul-button's action,make sure you want to do this");
                _action = value;
            }
        }
        public PressType type { get; protected set; }
    }

    public interface IMulHitControl
    {
        bool WasPressedThisFrame();
    }

    public class MulHitSameControl : ButtonControl, IMulHitControl
    {
        public bool WasPressedThisFrame()
        {
            if (TargetButton.wasPressedThisFrame)
            {
                CurrentHitCount++;
                if (CurrentHitCount == TargetHitCount)
                {
                    CurrentTime = 0;
                    CurrentHitCount = 0;
                    return true;
                }
            }
            return false;
        }

        public MulHitSameControl(int targetHitCounter, ButtonControl targetButton)
        {
            TargetHitCount = targetHitCounter;
            TargetButton = targetButton;
        }

        private float CurrentTime = 0;
        private float CurrentHitCount = 0;
        public int TargetHitCount = 0;
        public ButtonControl TargetButton = null;

        public void Update()
        {
            CurrentTime += Time.deltaTime;
            if (CurrentTime > 0.5f)
            {
                CurrentHitCount = 0;
                CurrentTime = 0;
            }
        }

        public override string ToString()
        {
            return TargetButton.ToString() + "(MulHit" + TargetHitCount.ToString() + ")";
        }
    }

    public class MulHitSomeControl : ButtonControl, IMulHitControl
    {
        public bool WasPressedThisFrame()
        {
            bool isHit = false;
            foreach (var button in TargetButtons)
            {
                if (button.wasReleasedThisFrame)
                {
                    return false;
                }
                else if (button.wasPressedThisFrame) isHit = true;
            }
            return isHit;
        }

        public MulHitSomeControl(List<ButtonControl> targetButtons)
        {
            TargetButtons = targetButtons;
        }

        public List<ButtonControl> TargetButtons = null;

        public override string ToString()
        {
            string str = "";
            foreach (var button in TargetButtons)
            {
                str += button.shortDisplayName;
            }
            return TargetButtons.ToString() + "(MulHit " + str + ")";
        }
    }

    public enum PressType
    {
        JustPressed,
        ThisFramePressed,
        ThisFrameReleased
    }

    [ExecuteAlways]
    public class ADGlobalSystem : SceneBaseController
    {
        #region Attribute

        private static bool AppQuitting = false;
        public static ADGlobalSystem _m_instance = null;
        public static ADGlobalSystem instance
        {
            get
            {
                if (AppQuitting) return _m_instance;
                if (_m_instance == null)
                {
                    var cat = GameObject.FindObjectsOfType(typeof(ADGlobalSystem));
                    if (cat.Length > 0) _m_instance = cat[0] as ADGlobalSystem;
                }
                if (_m_instance == null)
                {
                    try
                    {
                        _m_instance = GameObject.Instantiate(Resources.Load<GameObject>("GlobalSystem")).GetComponent<ADGlobalSystem>();
                    }
                    catch
                    {
                        _m_instance = new GameObject().AddComponent<ADGlobalSystem>();
                        _m_instance.name = "GlobalSystem";
                    }
                }
                return _m_instance;
            }
        }
    
        [Space(20),Header("GlobalSystem")]
        public bool IsNeedExcepion = true;
        public uint MaxRecordItemCount = 10000;
        public static bool IsKeepException => instance.IsNeedExcepion;

        public AD.UI.Button _Button;
        public AD.UI.Dropdown _DropDown;
        public AD.UI.InputField _InputField;
        public AD.UI.RawImage _RawImage;
        public AD.UI.Slider _Slider;
        public AD.UI.Text _Text;
        public AD.UI.Toggle _Toggle;

        public AD.UI.ModernUIButton _ModernButton;
        public AD.UI.ModernUIDropdown _ModernUIDropdown;
        public AD.UI.ModernUIFillBar _ModernUIFillBar;
        public AD.UI.ModernUIInputField _ModernUIInputField;
        public AD.UI.ModernUISwitch _ModernUISwitch;

        public ViewController _Image;
        public ColorManager _ColorManager;
        public AudioSourceController _AudioSource;
        public CustomWindowElement _CustomWindowElement;

        public static T GenerateElement<T>()where T:ADUI
        {
            try
            {
                return instance.GetFieldByName<T>("_" + typeof(T).Name);
            }
            catch
            {
                return null;
            }
        }

#if UNITY_EDITOR
        [MenuItem("GameObject/AD/GlobalSystem", false, 10)]
        public static void ADD(MenuCommand menuCommand)
        {
            if (instance != null)
            {
                Selection.activeObject = instance.gameObject;
                return;
            }
            try
            {
                ADGlobalSystem obj = GameObject.Instantiate(Resources.Load<GameObject>("GlobalSystem")).GetComponent<ADGlobalSystem>();
                Selection.activeObject = obj.gameObject;
            }
            catch
            {
                AD.ADGlobalSystem obj = new GameObject("GlobalSystem").AddComponent<AD.ADGlobalSystem>();
                _m_instance = obj;
                GameObjectUtility.SetParentAndAlign(obj.gameObject, menuCommand.context as GameObject);
                Undo.RegisterCreatedObjectUndo(obj.gameObject, "Create " + obj.name);
                Selection.activeObject = obj.gameObject;
            }
        }
#endif

        #endregion

        #region InputSystem

        public Dictionary<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>> multipleInputController
        { get; private set; } = new();

        public List<MulHitSameControl> mulHitControls { get; private set; } = new();

        private static void ReleaseThisFrameUpdate(KeyValuePair<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>> key)
        {
            try
            {
                if (key.Key.All((P) => (!P.GetType().Equals(typeof(MulHitSameControl)) && P.wasReleasedThisFrame)))
                {
                    key.Value.TryGetValue(PressType.ThisFrameReleased, out var events);
                    events?.Invoke();
                }
            }
            catch (System.Exception ex)
            {
                AddError("ReleaseThisFrameUpdate(key) key=" + key.ToString() + "\nException:" + ex.Message + "\nStackTrace:" + ex.StackTrace);
            }
        }
        private static void PressThisFrameUpdate(KeyValuePair<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>> key)
        {
            try
            {
                if (key.Key.All((P) => P is IMulHitControl IMul ? IMul.WasPressedThisFrame() : P.wasPressedThisFrame))
                {
                    key.Value.TryGetValue(PressType.ThisFramePressed, out var events);
                    events?.Invoke();
                }
            }
            catch (System.Exception ex)
            {
                AddError("PressThisFrameUpdate(key) key=" + key.ToString() + "\nException:" + ex.Message + "\nStackTrace:" + ex.StackTrace);
            }
        }
        private static void PressButtonUpdate(KeyValuePair<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>> key)
        {
            try
            {
                if (key.Key.All((P) => (!P.GetType().Equals(typeof(MulHitSameControl)) && P.isPressed)))
                {
                    key.Value.TryGetValue(PressType.JustPressed, out var events);
                    events?.Invoke();
                }
            }
            catch (System.Exception ex)
            {
                AddError("PressButtonUpdate(key) key=" + key.ToString() + "\nException:" + ex.Message + "\nStackTrace:" + ex.StackTrace);
            }
        }

        public static RegisterInfo AddListener(ButtonControl key, UnityEngine.Events.UnityAction action, PressType type = PressType.JustPressed)
        {
            KeyValuePair<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>> pair
                = instance.multipleInputController.FirstOrDefault((P) => { return P.Key[0].Equals(key) && P.Key.Count == 1; });
            if (pair.Equals(default(KeyValuePair<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>>)))
            {
                List<ButtonControl> currentList = new List<ButtonControl> { key };
                ADOrderlyEvent currentEv = new();
                currentEv.AddListener(action);

                instance.multipleInputController.Add(currentList, new Dictionary<PressType, ADOrderlyEvent> { { type, currentEv } });

                AddMessage(key.ToString() + "-based event was established");

                instance._IsOnValidate = true;
                return new RegisterInfo(currentList, action, type);
            }
            else
            {
                if (!pair.Value.ContainsKey(type)) pair.Value.Add(type, new());
                pair.Value[type].AddListener(action);
                instance._IsOnValidate = true;
                return new RegisterInfo(pair.Key, action, type);
            }
        }
        public static RegisterInfo AddListener(List<ButtonControl> keys, UnityEngine.Events.UnityAction action, PressType type = PressType.JustPressed)
        {
            KeyValuePair<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>> pair
                = instance.multipleInputController.FirstOrDefault((P) => { return P.Key.Intersect(keys).ToList().Count == keys.Count; });

            if (pair.Equals(default(KeyValuePair<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>>)))
            {
                ADOrderlyEvent currentEv = new();
                currentEv.AddListener(action);
                instance._IsOnValidate = true;

                if (keys.FindAll((P) => P == keys[0]).Count == keys.Count)
                {
                    List<ButtonControl> ckeys = new List<ButtonControl> { new MulHitSameControl(keys.Count, keys[0]) };
                    instance.mulHitControls.Add(ckeys[0] as MulHitSameControl);

                    instance.multipleInputController.Add(ckeys, new Dictionary<PressType, ADOrderlyEvent> { { type, currentEv } });

                    AddMessage(new MulHitSameControl(keys.Count, keys[0]).ToString() + "-based event was established");

                    return new RegisterInfo(ckeys, action, type);
                }
                else
                {
                    instance.multipleInputController.Add(keys, new Dictionary<PressType, ADOrderlyEvent> { { type, currentEv } });

                    AddMessage(keys.ToString() + "-based event was established");
                    return new RegisterInfo(keys, action, type);
                }
            }
            else
            {
                if (!pair.Value.ContainsKey(type)) pair.Value.Add(type, new());
                pair.Value[type].AddListener(action);
                instance._IsOnValidate = true;
                return new RegisterInfo(pair.Key, action, type);
            }
        }
        public static void RemoveListener(ButtonControl key, UnityEngine.Events.UnityAction action, PressType type = PressType.JustPressed)
        {
            KeyValuePair<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>> pair
                = instance.multipleInputController.FirstOrDefault((P) => { return P.Key[0].Equals(key) && P.Key.Count == 1; });
            if (!pair.Equals(default(KeyValuePair<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>>)) && pair.Value.ContainsKey(type))
            {
                pair.Value[type].RemoveListener(action);
            }
            instance._IsOnValidate = true;
        }
        public static void RemoveListener(List<ButtonControl> keys, UnityEngine.Events.UnityAction action, PressType type = PressType.JustPressed)
        {
            KeyValuePair<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>> pair
                = instance.multipleInputController.FirstOrDefault((P) => { return P.Key.Intersect(keys).ToList().Count == keys.Count; });
            if (!pair.Equals(default(KeyValuePair<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>>)) && pair.Value.ContainsKey(type))
            {
                pair.Value[type].RemoveListener(action);
            }
            if (keys.FindAll((P) => P == keys[0]).Count == keys.Count)
            {
                var temp = instance.mulHitControls.Find((P) => P.TargetButton == keys[0]);
                RemoveListener(temp, action, type);
                instance.mulHitControls.Remove(temp);
            }
            instance._IsOnValidate = true;
        }
        public static void RemoveAllListeners(ButtonControl key, PressType type = PressType.JustPressed)
        {
            KeyValuePair<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>> pair
                = instance.multipleInputController.FirstOrDefault((P) => { return P.Key[0].Equals(key) && P.Key.Count == 1; });
            if (!pair.Equals(default(KeyValuePair<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>>)) && pair.Value.ContainsKey(type))
            {
                pair.Value[type].RemoveAllListeners();
            }
            instance._IsOnValidate = true;
        }
        public static void RemoveAllListeners(List<ButtonControl> keys, PressType type = PressType.JustPressed)
        {
            KeyValuePair<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>> pair
                = instance.multipleInputController.FirstOrDefault((P) => { return P.Key.Intersect(keys).ToList().Count == keys.Count; });
            if (!pair.Equals(default(KeyValuePair<List<ButtonControl>, Dictionary<PressType, ADOrderlyEvent>>)) && pair.Value.ContainsKey(type))
            {
                pair.Value[type].RemoveAllListeners();
            }
            if (keys.FindAll((P) => P == keys[0]).Count == keys.Count)
            {
                var temp = instance.mulHitControls.Find((P) => P.TargetButton == keys[0]);
                RemoveAllListeners(temp, type);
                instance.mulHitControls.Remove(temp);
            }
            instance._IsOnValidate = true;
        }
        public static void RemoveAllButtonListeners()
        {
            instance.multipleInputController = new();
            instance.mulHitControls = new();
        }

        /// <summary>
        /// 设置快捷组合键的MulButton所对应的事件组唯一且无法清除，唯一方法是使用RegisterInfo操作
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public static RegisterInfo AddShortcutKeyCombinations(List<ButtonControl> keys, UnityEngine.Events.UnityAction action)
        {
            List<ButtonControl> buttons = new List<ButtonControl> { new MulHitSomeControl(keys) };
            ADOrderlyEvent currentEv = new();
            Dictionary<PressType, ADOrderlyEvent> currentDic = new()
            {
                [PressType.ThisFramePressed] = currentEv
            };
            currentEv.AddListener(action);
            instance.multipleInputController[buttons] = currentDic;
            return new RegisterInfo(buttons, action, PressType.ThisFramePressed);
        }

        #endregion

        #region MonoFunction 

        protected override void Awake()
        {
            if (_m_instance != null && _m_instance != this) DestroyImmediate(_m_instance.gameObject);
            _m_instance = this;
            base.Awake();
        }

        private void LateUpdate()
        {
            Instance_Update();
            KeyControl_Update();
            Record_Update();
            Time_Update();

            void Instance_Update()
            {
                if (_m_instance != null && _m_instance != this) DestroyImmediate(_m_instance.gameObject);
                if (_m_instance == null) _m_instance = this;
            }

            void KeyControl_Update()
            {
                foreach (var key in mulHitControls) key.Update();
                foreach (var key in multipleInputController)
                {
                    PressButtonUpdate(key);
                    PressThisFrameUpdate(key);
                    ReleaseThisFrameUpdate(key);
                }
            }

            void Record_Update()
            {
                if (record.Count > MaxRecordItemCount) SaveRecord();
            }

            void Time_Update()
            {
                TimeExtension.Update();
            }
        }

        [HideInInspector] public bool _IsOnValidate = false;

        private void OnValidate()
        {
            _IsOnValidate = true;
        }

        public void OnApplicationQuit()
        {
            AppQuitting = true;
            SaveRecord();
        }

        #endregion

        #region UtilityFunction

        public static void Output<T>(string filePath, T obj)
        {
            if (obj == null)
            {
                AddMessage("Failed Output " + filePath);
                return;
            }
            FileC.CreateDirectroryOfFile(filePath);
            if (typeof(T).Equals(typeof(string)))
            {
                File.WriteAllText(filePath, obj as string, Encoding.UTF8);
            }
            else if (obj.GetType().IsPrimitive)
            {
                File.WriteAllText(filePath, obj.ToString(), Encoding.UTF8);
            }
#if EASY3
            else if (obj.GetType().GetAttribute<EaseSave3Attribute>() != null)
            {
                ES3.Save(filePath.Split('.')[^1], obj, filePath);
            }
#endif // EASY3
            else if (obj.GetType().GetAttribute<SerializableAttribute>() != null)
            {
                File.WriteAllText(filePath, JsonConvert.SerializeObject(obj), Encoding.UTF8);
            }
            else
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    BinaryFormatter formatter = new BinaryFormatter();
                    formatter.Serialize(ms, obj);
                    byte[] bytes = ms.GetBuffer();
                    File.Create(filePath);
                    File.WriteAllBytes(filePath, bytes);
                }
            }
            AddMessage("Output " + filePath);
        }

        public static bool Input(string filePath, out string str)
        {
            if (FileC.GetDirectroryOfFile(filePath) == null)
            {
                str = "";
                return false;
            }
            else
            {
                try
                {
                    str = File.ReadAllText(filePath, Encoding.UTF8);
                    return true;
                }
                catch
                {
                    str = "";
                    return false;
                }
            }
        }

        public static bool Input<T>(string filePath, out object obj)
        {
            if (FileC.GetDirectroryOfFile(filePath) == null)
            {
                obj = default(T);
                return false;
            }
            else if (typeof(T).IsPrimitive)
            {
                try
                {
                    obj = typeof(T).GetMethod("Parse").Invoke(File.ReadAllText(filePath, Encoding.UTF8), null);
                    return true;
                }
                catch
                {
                    obj = default(T);
                    return false;
                }
            }
#if EASY3
            else if (typeof(T).GetAttribute<EaseSave3Attribute>() != null)
            {
                try
                {
                    obj = ES3.Load(Path.GetFileNameWithoutExtension(filePath), filePath);
                    if (obj != null) return true;
                    else return false;
                }
                catch
                {
                    obj = default(T);
                    return false;
                }
            }
#endif // ESAY3
            else if (typeof(T).GetAttribute<SerializableAttribute>() != null)
            {
                try
                {
                    obj = JsonConvert.DeserializeObject<T>(File.ReadAllText(filePath, Encoding.UTF8));
                    if (obj != null) return true;
                    else return false;
                }
                catch
                {
                    obj = default(T);
                    return false;
                }
            }
            else
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        obj = formatter.Deserialize(ms);
                    }
                    if (obj != null) return true;
                    else return false;
                }
                catch
                {
                    obj = default(T);
                    return false;
                }
            }
        }

        public static bool Deserialize<T>(string source, out object obj)
        { 
            if (typeof(T).IsPrimitive)
            {
                try
                {
                    obj = typeof(T).GetMethod("Parse").Invoke(source, null);
                    return true;
                }
                catch
                {
                    obj = default(T);
                    return false;
                }
            }
            else if (typeof(T).GetAttribute<SerializableAttribute>() != null)
            {
                try
                {
                    obj = JsonConvert.DeserializeObject<T>(source);
                    if (obj != null) return true;
                    else return false;
                }
                catch
                {
                    obj = default(T);
                    return false;
                }
            }
            else
            {
                try
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        BinaryFormatter formatter = new BinaryFormatter();
                        obj = formatter.Deserialize(ms);
                    }
                    if (obj != null) return true;
                    else return false;
                }
                catch
                {
                    obj = default(T);
                    return false;
                }
            }
        }

        public static bool Serialize<T>(T obj, out string str)
        {
#if UNITY_EDITOR
            if (typeof(T).GetAttribute<SerializableAttribute>() == null)
            {
                Debug.LogWarning("this type is not use SerializableAttribute but you now is try to serialize it");
            }
#endif
            try
            {
                str = JsonConvert.SerializeObject(obj, Formatting.Indented);
                return true;
            }
            catch
            {
                str = "error";
                return false;
            }
        }

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

        #endregion

        #region UtilityRecord

        public string RecordPath = "null";

        public List<UtilityPackage> record = new List<UtilityPackage>();

        public static void AddMessage(string message, string state = "")
        {
            if (instance.record.Count > 0 && instance.record[^1].message == message && instance.record[^1].state == state)
            {
                instance.record[^1].times++;
            }
            UtilityPackage cMessage = new UtilityPackage(message, state);
            instance.record.Add(cMessage);
            if (state != "") Debug.Log(cMessage.ObtainResult());
        }

        public static void AddWarning(string message, string state = "Warning")
        {
            if (instance.record.Count > 0 && instance.record[^1].message == message && instance.record[^1].state == state)
            {
                instance.record[^1].times++;
            }
            UtilityPackage cMessage = new UtilityPackage(message, state);
            instance.record.Add(cMessage);
            if (state == "Warning") Debug.LogWarning(cMessage.ObtainResult());
        }

        public static void AddError(string message, string state = "Error")
        {
            if (instance.record.Count > 0 && instance.record[^1].message == message && instance.record[^1].state == state)
            {
                instance.record[^1].times++;
            }
            UtilityPackage cMessage = new UtilityPackage(message, state);
            instance.record.Add(cMessage);
            if (state == "Error") Debug.LogError(cMessage.ObtainResult());
        }

        public static void ThrowLogicError(string message, string state = "LogicError")
        {
            if (instance.record.Count > 0 && instance.record[^1].message == message && instance.record[^1].state == state)
            {
                instance.record[^1].times++;
            }
            UtilityPackage cMessage = new UtilityPackage(message, state);
            instance.record.Add(cMessage);
            throw new ADException("[Problems that should not occur]" + message);
        }

        public string ObtainResultAndClean()
        {
            string result = "<Result>" + DateTime.Now.ToString() + "\n";
            foreach (var item in record) result += item.ObtainResult();
            record = new List<UtilityPackage>();
            Debug.Log("Record is clean");
            return result;
        }

        public void SaveRecord()
        {
            if (record.Count > 0)
            {
                string allMessage = ObtainResultAndClean();

                Output((RecordPath == "null") ? (Path.Combine(
                        Application.persistentDataPath,
                        "ADGlobalSystemLog",
                        DateTime.Now.Hour.ToString() + "H" +
                        DateTime.Now.Minute.ToString() + "M" +
                        DateTime.Now.Second.ToString() + "S" + ".AD.log")) : (RecordPath), allMessage);
            }
        }

        public static T Error<T>(string message, Exception ex, T result) where T : class, new()
        {
            if (IsKeepException) throw new ADException(message, ex);
            AddError(message + "\nError: " + ex.Message + "\nStackTrace: " + ex.StackTrace);
            return result;
        }
        public static T Error<T>(string message, Exception ex = null) where T : class, new()
        {
            if (IsKeepException) throw new ADException(message, ex);
            AddError(message + "\nError: " + ex.Message + "\nStackTrace: " + ex.StackTrace);
            return default(T);
        }
        public static T Error<T>(string message) where T : class, new()
        {
            if (IsKeepException) throw new ADException(message);
            AddError(message);
            return default(T);
        }

        public static bool Error(string message, bool result = false, Exception ex = null)
        {
            if (IsKeepException) throw new ADException(message, ex);
            AddError(message);
            return result;
        }

        public static void TrackError(string message, System.Exception ex)
        {
            //utility.AddError("\nMessage: " + message + "\nError: " + ex.Message + "\nStackTrace: " + ex.StackTrace);
            Error<object>("\nMessage: " + message + "\nError: " + ex.Message + "\nStackTrace: " + ex.StackTrace, ex);
        }

        public static T FinalCheck<T>(T result, string message = "you obtain a null object")
        {
            if (result == null) AddError(message);
            return result;
        }

        public static T FinalCheckWithThrow<T>(T result, string message = "you obtain a null object")
        {
            if (result == null) throw new ADException(message);
            return result;
        }

        public static void FunctionalRecord<T>(T func)
        {
            AddMessage(func.ToString());
        }


        #endregion
    }

    public static class MethodBaseExtension
    {
        public static MethodBase TrackError(this MethodBase method, System.Exception ex)
        {
            var att = method.GetAttribute<ADSafeAttribute>();
            if (att == null)
            {
                ADGlobalSystem.AddWarning(method.Name + " not has an Attribute(User)");
                ADGlobalSystem.AddError(method.Name + "\n" + ex.Message);
                return method;
            }
            ADGlobalSystem.TrackError((att.message == "") ? method.Name : att.message, ex);
            return method;
        }

        public static object SafeTrackError(this MethodBase method, System.Exception ex)
        {
            var att = method.GetAttribute<ADSafeAttribute>();
            ADGlobalSystem.TrackError((att.message == "") ? method.Name : att.message, ex);
            return System.Activator.CreateInstance(att.type);
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ADSafeAttribute : Attribute
    {
        public ADSafeAttribute() { }
        public ADSafeAttribute(string message) { this.message = message; }
        public ADSafeAttribute(System.Type type) { this.type = type; }
        public ADSafeAttribute(string message, System.Type type) { this.message = message; this.type = type; }

        public string message = "";
        public System.Type type = null;
    }

#if EASY3
    [AttributeUsage(AttributeTargets.Class)]
    public class EaseSave3Attribute : Attribute
    {
    }
#endif // EASY3

    [Serializable]
    public class UtilityPackage
    {
        public string currentTime;
        public string message;
        public string state;
        public int times;

        public UtilityPackage(string message, string state = "")
        {
            currentTime = DateTime.Now.ToString();
            this.message = message;
            this.state = state;
            times = 1;
        }

        public string ObtainResult()
        {
            return ((times == 1) ? "" : ("#" + times.ToString() + "=>")) + "[ " + currentTime + "  " + state + " ] " + message + "\n";
        }
    }

}
