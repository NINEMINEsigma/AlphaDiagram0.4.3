using System;
using System.Collections.Generic;
using AD.BASE;
using UnityEngine;
using AD.Utility.Internal;
using static AD.Utility.ReflectionExtension;
using System.Linq;

namespace AD.Utility
{
    namespace Internal
    {
        internal class ArithmeticReader
        {
            private class String_View
            {
                public int Start, End;

                public String_View()
                {
                }

                public String_View(int start, int end)
                {
                    Start = start;
                    End = end;
                }

                public string SubString(string source)
                {
                    return source[Start..End];
                }
            }

            public ArithmeticReader(string source, ArithmeticInfo info)
            {
                this.source = source.Trim();
                this.info = info;
            }

            /// <summary>
            /// 原字符串经少量处理后的源字符串
            /// </summary>
            private string source;
            /// <summary>
            /// 源字符串划分出的初层，如果不存在括号化和变量则不生成该List的实例，可以直接赋值info唯一Data表达式的最终结果：一个ArithmeticConstant
            /// </summary>
            private List<String_View> subStrs = new();
            /// <summary>
            /// 闭包传递的源Info实例
            /// </summary>
            private ArithmeticInfo info;
#if UNITY_EDITOR
            public string BreakMessage;
#endif

            public ArithmeticInfo BuildAnalyty(bool isUnSafe)
            {
                try
                {
                    //先分离函数与变量块
                    BuildVariablesAndFunctionsPriority();
                    //再执行显式括号化
                    BuildParenthesesPriority();
                    //然后根据四则运算优先级进行隐式括号化
                    BuildOperatorPriority();
                    //最终解析
                    BuildFinalArgList(isUnSafe);
                }
                catch (ArithmeticStringErrorException ex)
                {
                    if (ex.baseAriInfo == null) throw new ArithmeticStringErrorException(source + " { " + ex.expression + " }", info, this);
                    else throw new ArithmeticStringErrorException(source + " { " + ex.expression + " }", ex.baseAriInfo, ex.baseAriReader);
                }
                catch { throw; }
                return info;
            }

            private void BuildFinalArgList(bool isUnSafe)
            {
#if UNITY_EDITOR
                BreakMessage = "Start BuildFinalArgList";
#endif
            AD.Utility.ArithmeticInfo.ArithmeticData.Symbol currentSymbol = ArithmeticInfo.ArithmeticData.Symbol.Addition;
                List<ArithmeticInfo.ArithmeticData> arithmeticDatas = new();
                foreach (var subStr in subStrs)
                {
                    if (subStr.End - subStr.Start == 1)
                    {
                        char current = source[subStr.Start];
                        if (current == '+' || current == '-' || current == '*' || current == '/')
                        {
                            currentSymbol = (AD.Utility.ArithmeticInfo.ArithmeticData.Symbol)current;
                            continue;
                        }
                    }
                    {
                        string SubCurrentExpression = subStr.SubString(source);
                        if (ArithmeticInfo.Parse(SubCurrentExpression, out ArithmeticInfo current))
                        {
                            var temp = new ArithmeticInfo.ArithmeticData(SubCurrentExpression);
                            temp.ItemSymbol = currentSymbol;
                            temp.Item = current;
                            arithmeticDatas.Add(temp);
                        }
                        else if (isUnSafe)
                        {
                            var temp = new ArithmeticInfo.ArithmeticData(SubCurrentExpression);
                            temp.ItemSymbol = currentSymbol;
                            temp.Item = new ArithmeticConstant(0);
                            arithmeticDatas.Add(temp);
                        }
                        else throw new ArithmeticStringErrorException(SubCurrentExpression);
                    }
                }
                info.ArithmeticDatas = arithmeticDatas;
#if UNITY_EDITOR
                BreakMessage = "End BuildFinalArgList";
#endif
            }

            #region BuildVariablesAndFunctionsPriority

            //引索的初和末都是不能读取的符号位
            private Dictionary<int, int> VariablesAndFunctionsLevelBlock = new();

            private void BuildVariablesAndFunctionsPriority()
            {
#if UNITY_EDITOR
                BreakMessage = "Start BuildVariablesAndFunctionsPriority";
#endif
                int start = 0, level = 0;
                for (int i = 0, e = source.Length; i < e; i++)
                {
                    char current = source[i];
                    //扫描初层，层级为0时展开初层，并将首位引索设为当前位置
                    if (current == '{')
                    {
                        if (level++ == 0) start = i;
                    }
                    //扫描末层，层级降为0时截取初层
                    else if (current == '}')
                    {
                        if (--level == 0) VariablesAndFunctionsLevelBlock[start] = i;
                    }
                }
                if (level != 0) throw new ArithmeticStringErrorException(source);
#if UNITY_EDITOR
                BreakMessage = "End BuildVariablesAndFunctionsPriority";
#endif
            }

            #endregion

            #region BuildParenthesesPriority

            //引索的初和末都是不能读取的符号位
            private Dictionary<int, int> ParenthesesBlock = new();

            private void BuildParenthesesPriority()
            {
#if UNITY_EDITOR
                BreakMessage = "Start BuildParenthesesPriority";
#endif
                int start = 0, level = 0;
                for (int i = 0, e = source.Length; i < e; i++)
                {
                    char current = source[i];
                    //跳过变量与函数的源字符串
                    if (VariablesAndFunctionsLevelBlock.TryGetValue(i, out int newIndex))
                    {
                        i = newIndex + 1;
                        //结尾引索不可能超过源字符串末端，但是+1后可能，所以进行一次判断
                        if (i >= e) break;
                        current = source[i];
                    }
                    if (current == '(')
                    {
                        if (level++ == 0)
                        {
                            //首段初始化初层
                            //if (start == 0 && i != 0)
                            //{
                            //    //前一位必是符号层
                            //    BuildSubParenthesesPriorityBlock(start, i - 1);
                            //}
                            start = i;
                        }
                    }
                    else if (current == ')')
                    {
                        if (--level == 0)
                        {
                            BuildSubParenthesesPriorityBlock(start, i);
                            start = i;
                        }
                    }
                }
                if (level != 0) throw new ArithmeticStringErrorException(source);
                //末段初始化初层
                //if (start + 2 <= source.Length)
                //{
                //    BuildSubParenthesesPriorityBlock(start + 2, source.Length);
                //}
#if UNITY_EDITOR
                BreakMessage = "End BuildParenthesesPriority";
#endif
            }

            private void BuildSubParenthesesPriorityBlock(int start, int end)
            {
#if UNITY_EDITOR
                if (start == end)
                {
                    ADGlobalSystem.ThrowLogicError("Start == End");
                }
#endif
                ParenthesesBlock[start] = end;
            }

            #endregion

            #region BuildOperatorPriority

            private void BuildOperatorPriority()
            {
#if UNITY_EDITOR
                BreakMessage = "Start BuildOperatorPriority";
#endif
                int start = 0;
                bool IsJustMD = true;
                for (int i = 0, e = source.Length; i < e; i++)
                {
                    char current = source[i];
                    //跳过括号化的源字符串
                    if (ParenthesesBlock.TryGetValue(i, out int newIndex1))
                    {
                        i = newIndex1 + 1;
                        //结尾引索不可能超过源字符串末端，但是+1后可能，所以进行一次判断
                        if (i >= e) break;
                        current = source[i];
                    }
                    //跳过变量与函数的源字符串
                    if (VariablesAndFunctionsLevelBlock.TryGetValue(i, out int newIndex0))
                    {
                        i = newIndex0 + 1;
                        //结尾引索不可能超过源字符串末端，但是+1后可能，所以进行一次判断
                        if (i >= e) break;
                        current = source[i];
                    }
                    //处理符号位，并根据优先级合并同级
                    if (current == '+' || current == '-')
                    {
                        IsJustMD = false;
                        BuildOperator(ref start, ref i, e);
                    }
                    else if (current == '*' || current == '/')
                    {
                        BuildOperator(ref start, ref i, e);
                    }
                }
                BuildSubStringExpression(start, source.Length);

#if UNITY_EDITOR
                BreakMessage = "Mid BuildOperatorPriority";
#endif

                if (!IsJustMD)
                {
                    List<String_View> newSubStrs = new();
                    for (int i = 0, e = subStrs.Count; i < e; i++)
                    {
                        string current = subStrs[i].SubString(source);
                        if (current == "+" || current == "-")
                        {
                            if (i != 0) newSubStrs.Add(subStrs[i - 1]);
                            newSubStrs.Add(subStrs[i]);
                        }
                        else if (current == "*" || current == "/")
                        {
                            subStrs[i + 1].Start = subStrs[i - 1].Start;
                        }
                    }
                    newSubStrs.Add(subStrs[^1]);
                    subStrs = newSubStrs;
                }

#if UNITY_EDITOR
                BreakMessage = "End BuildOperatorPriority";
#endif

                void BuildOperator(ref int start, ref int i, int e)
                {
                    BuildSubStringExpression(start, i);
                    start = i;
                    BuildSubStringExpression(start, i + 1);
                    start = i + 1;
                    //最后一位字符不可能是运算符
                    if (i >= e) throw new ArithmeticStringErrorException(source);
                }
            }

            private void BuildSubStringExpression(int start, int end)
            {
                subStrs.Add(new String_View(start, end));
            }

            #endregion
        }
    }

    #region Exception

    [System.Serializable]
    public class ArithmeticException : ADException
    {
        public ArithmeticException() : base("Arithmetic Error") { }
        public ArithmeticException(string message) : base(message) { }
    }

    [System.Serializable]
    public class ArithmeticStringNullOrEmptyException : ADException
    {
        public ArithmeticStringNullOrEmptyException() : base("Exception is null or empty") { }
    }

    [System.Serializable]
    public class ArithmeticStringErrorException : ADException
    {
        public ArithmeticStringErrorException(string expression) : base("Exception is about expression : " + expression) { this.expression = expression; }
        internal ArithmeticStringErrorException(string expression, ArithmeticInfo baseAriInfo, ArithmeticReader baseAriReader) : base("Exception is about expression : " + expression)
        {
            this.expression = expression;
            this.baseAriInfo = baseAriInfo;
            this.baseAriReader = baseAriReader;
        }

        public string expression;
        public ArithmeticInfo baseAriInfo;
        internal ArithmeticReader baseAriReader;
    }

    [System.Serializable]
    public class ArithmeticResultNullException : ADException
    {
        public ArithmeticResultNullException() : base("Result is null") { }
    }

    [System.Serializable]
    public class ArithmeticNANException : ADException
    {
        public ArithmeticNANException() : base("The calculation process occurs NAN") { }
    }

    [System.Serializable]
    public class ArithmeticNotLegitimateException : ADException
    {
        public ArithmeticNotLegitimateException() : base("The calculation process use unlegitimate object") { }
    }

    [System.Serializable]
    public class ArithmeticNotSupportException : ADException
    {
        public ArithmeticNotSupportException() : base("The calculation process not supported") { }
        public ArithmeticNotSupportException(string message) : base("The calculation process not supported " + message) { }
    }

    #endregion

    public static class ArithmeticExtension
    {
        public static ArithmeticInfo Parse(string expression)
        {
            return ArithmeticInfo.Parse(expression, out ArithmeticInfo result, false) ? result : null;
        }

        public static bool TryParse(string expression, out ArithmeticInfo result)
        {
            try
            {
                return ArithmeticInfo.Parse(expression, out result, false);
            }
            catch (Exception ex)
            {
                Debug.LogWarning("ArithmeticExtension.TryParse");
                Debug.LogException(ex);
                return result = ArithmeticInfo.GetUnlegitimateOne();
            }
        }

        public static bool TryParseWithUnSafeMode(string expression, out ArithmeticInfo result)
        {
            try
            {
                return ArithmeticInfo.Parse(expression, out result, true);
            }
            catch
            {
                return result = ArithmeticInfo.GetUnlegitimateOne();
            }
        }
    }

    [Serializable]
    public class ArithmeticInfo
    {
        [Serializable]
        public class ArithmeticData
        {
            [SerializeField] private string m_currentExpression;
            public string CurrentExpression
            {
                get => m_currentExpression; private set => m_currentExpression = value;
            }

            public enum Symbol
            {
                Addition = '+', Subtraction = '-', Multiplication = '*', Division = '/'
            }

            public Symbol ItemSymbol = Symbol.Addition;
            public ArithmeticInfo Item;

            public ArithmeticData(string currentExpression)
            {
                CurrentExpression = currentExpression;
            }
        }

        //大写的I和小写的l无法区分，于是写作Unlegitimate
        public static ArithmeticInfo GetUnlegitimateOne() => new(false);

        protected ArithmeticInfo(bool isLegitimate = true) { this.m_isLegitimate = isLegitimate; }

        public static bool Parse(string expression, out ArithmeticInfo result, bool isUnSafe = false)
        {
            //预处理
            expression = expression.Trim();
            if (expression.Length > 2 && expression[0] == '(' && expression[^1] == ')')
            {
                string temp = expression[1..^1];
                //这种情况下应当是有开头结尾是显式的括号包裹着，此时才更新处理源字符串，否则可能是(x)+(y)类似的情况，不能去掉括号
                if (!temp.Contains('(')) expression = temp;
            }
            //首先尝试简单解析
            if (ArithmeticBoolen.TryParse(expression, out ArithmeticBoolen boolen)) return result = boolen;
            if (ArithmeticFunction.TryParse(expression, out ArithmeticFunction function)) return result = function;
            if (ArithmeticVariable.TryParse(expression, out ArithmeticInfo variable)) return result = variable;
            if (ArithmeticConstant.TryParse(expression, out ArithmeticConstant constant)) return result = constant;
            //非简单语句时进行以下解析
            result = new(true);

            var reader = new ArithmeticReader(expression, result);
            reader.BuildAnalyty(isUnSafe);
            result.isLegitimate = result.m_isLegitimate = true;

            return result;
        }

        public virtual float ReadValue()
        {
            if (!m_isLegitimate || ArithmeticDatas == null || ArithmeticDatas.Count == 0)
            {
                Debug.LogError("You are trying to use an ArithmeticInfo which is illegitimate");
                return 0;
            }
            float result = 0;
            foreach (var singleData in ArithmeticDatas)
            {
                switch (singleData.ItemSymbol)
                {
                    case ArithmeticData.Symbol.Addition:
                        result += singleData.Item.ReadValue();
                        break;
                    case ArithmeticData.Symbol.Subtraction:
                        result -= singleData.Item.ReadValue();
                        break;
                    case ArithmeticData.Symbol.Multiplication:
                        result *= singleData.Item.ReadValue();
                        break;
                    case ArithmeticData.Symbol.Division:
                        result /= singleData.Item.ReadValue();
                        break;
                    default:
                        throw new ArithmeticNotSupportException();
                }
            }
            return result;
        }

        public static implicit operator bool(ArithmeticInfo arithmeticInfo) => arithmeticInfo.isLegitimate && arithmeticInfo.m_isLegitimate;

        /// <summary>
        /// 是否是一个合法的算术表达式
        /// </summary>
        [SerializeField] private bool m_isLegitimate = false;
        /// <summary>
        /// 由子类决定的：是否是一个合法的算术表达式
        /// </summary>
        [SerializeField] protected bool isLegitimate = false;

        public List<ArithmeticData> ArithmeticDatas;

        [SerializeField] protected string m_key;
        [SerializeField] protected float m_value;

        //[SerializeField] 
        private ArithmeticConstant m_ArithmeticConstantValue;
        //[SerializeField] 
        private ArithmeticVariable m_ArithmeticVariableValue;
       // [SerializeField] 
        private ArithmeticFunction m_ArithmeticFunctionValue;
        public ArithmeticConstant ArithmeticConstantValue { get => m_ArithmeticConstantValue; protected set => m_ArithmeticConstantValue = value; }
        public ArithmeticVariable ArithmeticVariableValue { get => m_ArithmeticVariableValue; protected set => m_ArithmeticVariableValue = value; }
        public ArithmeticFunction ArithmeticFunctionValue { get => m_ArithmeticFunctionValue; protected set => m_ArithmeticFunctionValue = value; }

        public override string ToString()
        {
            return this.ReadValue().ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj is ArithmeticInfo other) return other.ReadValue().Equals(this.ReadValue());
            else return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    [Serializable]
    public class ArithmeticConstant : ArithmeticInfo
    {
        public static implicit operator float(ArithmeticConstant arithmeticConstant) => arithmeticConstant.ReadValue();

        public ArithmeticConstant() : this(0) { }
        public ArithmeticConstant(float value) : base(true)
        {
            m_value = value;
            this.isLegitimate = true;
            this.ArithmeticConstantValue = this;
        }

        public override float ReadValue()
        {
            if (!this) throw new ArithmeticNotLegitimateException();
            return m_value;
        }

        public void SetValue(float value)
        {
            m_value = value;
        }

        public static bool TryParse(string expression, out ArithmeticConstant constant)
        {
            constant = null;
            if (float.TryParse(expression, out float resultValue))
            {
                constant = new ArithmeticConstant(resultValue);
                return true;
            }
            return false;
        }
    }

    [Serializable]
    public class ArithmeticVariable : ArithmeticInfo
    {
        [Serializable]
        public class VariableEntry
        {
            public int RefIndex;
            public ArithmeticConstant Value;
        }

        internal static Dictionary<string, VariableEntry> VariableConstantPairs = new();

        public static implicit operator float(ArithmeticVariable arithmeticVariable) => arithmeticVariable.ReadValue();

        public ArithmeticVariable() : base(false) { }
        public ArithmeticVariable(string key)
        {
            m_key = key;
            this.isLegitimate = !string.IsNullOrEmpty(key);
            RefValue();
        }
        public ArithmeticVariable(string key, float value)
        {
            m_key = key;
            this.isLegitimate = !string.IsNullOrEmpty(key);
            m_value = value;
            InitValue();
        }

        ~ArithmeticVariable()
        {
            UpdateValue();
            if (this)
            {
                VariableConstantPairs[m_key].RefIndex--;
                if (VariableConstantPairs[m_key].RefIndex == 0) VariableConstantPairs.Remove(m_key);
            }
        }

        public override float ReadValue()
        {
            UpdateValue();
            if (!this) throw new ArithmeticNotLegitimateException();
            return m_value;
        }

        public void UpdateValue()
        {
            this.isLegitimate = VariableConstantPairs.TryGetValue(this.m_key, out var variableEntry);
            if (this.isLegitimate) this.m_value = variableEntry.Value.ReadValue();
        }

        private void RefValue()
        {
            this.isLegitimate = VariableConstantPairs.TryGetValue(this.m_key, out var variableEntry);
            if (this.isLegitimate)
            {
                this.m_value = variableEntry.Value.ReadValue();
                variableEntry.RefIndex++;
            }
        }

        private void InitValue()
        {
            this.isLegitimate = !VariableConstantPairs.ContainsKey(this.m_key);
            if (this.isLegitimate) VariableConstantPairs.Add(m_key, new VariableEntry() { RefIndex = 1, Value = new ArithmeticConstant(m_value) });
        }

        public static bool TryParse(string expression, out ArithmeticInfo variable)
        {
            expression = expression.Trim();
            if (expression[0] == '{' && expression[^1] == '}')
            {
                expression = expression[1..^1].Trim();
                //如果去除头尾括号后依然存在标识符括号那么不是变量
                if (expression.Contains('{'))
                {
                    variable = null;
                    return false;
                }
                variable = MakeEquals(expression) ?? IfEquals(expression) ?? new ArithmeticVariable(expression);
                return variable;
            }
            variable = null;
            return false;
        }

        private static ArithmeticInfo MakeEquals(string expression)
        {
            ArithmeticVariable variable = null;
            if (!expression.Contains('=')) return null;
            string[] strs = expression.Split('=');
            if (strs.Length == 2)
            {
                string name = strs[0].Trim();
                bool IsCanParseArgs = ArithmeticInfo.Parse(strs[1], out ArithmeticInfo temp);
                if (!IsCanParseArgs)
                {
                    IsCanParseArgs = ArithmeticBoolen.TryParse(strs[1], out ArithmeticBoolen tempb);
                    temp = tempb;
                }
                if (VariableConstantPairs.TryGetValue(name, out var variableEntry))
                {
                    if (IsCanParseArgs) variableEntry.Value.SetValue(temp.ReadValue());
                    else throw new ArithmeticStringErrorException(expression);
                    variable = new ArithmeticVariable(name);
                }
                else
                {
                    if (!IsCanParseArgs) throw new ArithmeticStringErrorException(expression);
                    variable = new ArithmeticVariable(name, temp.ReadValue());
                }
            }
            return variable;
        }

        private static ArithmeticInfo IfEquals(string expression)
        {
            ArithmeticVariable variable = null;
            if (!expression.Contains(':')) return null;
            string[] strs = expression.Split(':');
            if (strs.Length == 2)
            {
                bool IsCanParseArgs = ArithmeticInfo.Parse(strs[1], out ArithmeticInfo temp);
                if (VariableConstantPairs.TryGetValue(strs[0].Trim(), out var variableEntry))
                {
                    if (IsCanParseArgs)
                    {
                        if (variableEntry.Value.ReadValue() == temp.ReadValue()) return new ArithmeticBoolen(true);
                        else return new ArithmeticBoolen(false);
                    }
                    else throw new ArithmeticStringErrorException(expression);
                }
                else
                {
                    throw new ArithmeticStringErrorException(expression);
                }
            }
            return variable;
        }
    }

    [Serializable]
    public class ArithmeticFunction : ArithmeticInfo
    {
        [Serializable]
        public class FunctionEntry
        {
            public FunctionEntry(ADReflectedMethod reflectedMethod, object targetInstance)
            {
                m_reflectedMethod = reflectedMethod;
                TargetInstance = targetInstance;
            }

            public ADReflectedMethod m_reflectedMethod;
            public object TargetInstance { get; private set; }
        }

        internal static Dictionary<string, FunctionEntry> FunctionPairs = new();

        public Func<object[], float> Func { get; private set; }
        public int ArgsTotal { get; private set; }

        public static implicit operator Func<object[], float>(ArithmeticFunction arithmeticFunction) => arithmeticFunction.GetMethod();

        public ArithmeticFunction() : base(false) { }
        public ArithmeticFunction(string name)
        {
            m_key = name;
            this.isLegitimate = !string.IsNullOrEmpty(name);
            this.ArithmeticFunctionValue = this;
            UpdateFunc();
        }

        public override float ReadValue()
        {
            if (this.ArithmeticDatas.Count != ArgsTotal) throw new ArithmeticNotSupportException();
            object[] args = new object[this.ArithmeticDatas.Count];
            for (int i = 0; i < ArithmeticDatas.Count; i++)
            {
                args[i] = (this.ArithmeticDatas[i].ItemSymbol == ArithmeticData.Symbol.Subtraction ? -1 : 1) * this.ArithmeticDatas[i].Item.ReadValue();
            }
            return Func(args);
        }

        public Func<object[], float> GetMethod()
        {
            UpdateFunc();
            return Func;
        }

        public void UpdateFunc()
        {
            try
            {
                this.isLegitimate = FunctionPairs.TryGetValue(m_key, out FunctionEntry entry);
                if (this.isLegitimate)
                {
                    Func = objects => (float)entry.m_reflectedMethod.Invoke(entry.TargetInstance, objects);
                    ArgsTotal = entry.m_reflectedMethod.ArgsTotal;
                }
            }
            catch
            {
                this.isLegitimate = false;
            }
        }

        public static void RegisterFunc(string name, FunctionEntry entry) => FunctionPairs[name] = entry;

        public static bool TryParse(string expression, out ArithmeticFunction function)
        {
            expression = expression.Trim();
            if (expression[0] == '{' && expression[^1] == '}')
            {
                if (expression.Contains('('))
                {
                    if (expression.Contains(')'))
                    {
                        expression = expression[1..^1].Trim();
                        int fi = expression.First(ch => ch == '(');
                        //...() （就在倒数第二的位置，)在倒数第一
                        if (fi == expression.Length - 2)
                        {
                            if (expression[fi + 1] == ')') return function = new ArithmeticFunction(expression.Trim());
                            else throw new ArithmeticStringErrorException(expression);
                        }
                        string[] strs = new string[2];
                        //删除第一个(与最后一个)
                        strs[0] = expression[..fi].Trim();
                        strs[1] = expression[(fi + 1)..].Trim()[..^1];
                        function = new ArithmeticFunction(strs[0]);
                        if (function)
                        {
                            //构造参数
                            function.ArithmeticDatas = BuildArgs(strs[1].Split(',')).ToList();
                            return true;
                        }
                    }
                    throw new ArithmeticStringErrorException(expression);
                }
            }
            function = null;
            return false;
        }

        private static ArithmeticData[] BuildArgs(string[] sourceStrs)
        {
            ArithmeticData[] result = new ArithmeticData[sourceStrs.Length];
            for (int i = 0, e = result.Length; i < e; i++)
            {
                result[i] = new ArithmeticData(sourceStrs[i]);
                if (ArithmeticInfo.Parse(sourceStrs[i], out ArithmeticInfo arithmeticInfo))
                    result[i].Item = arithmeticInfo;
                else throw new ArithmeticStringErrorException(sourceStrs[i]);
            }
            return result;
        }
    }

    [Serializable]
    public class ArithmeticBoolen: ArithmeticConstant
    {
        public ArithmeticBoolen(bool boolen) : base(boolen ? 1 : 0) { }

        public override float ReadValue()
        {
            return base.ReadValue() == 0 ? 0 : 1;
        }

        public virtual bool ReadBoolen()
        {
            return base.ReadValue() != 0;
        }

        public override string ToString()
        {
            return ReadBoolen() ? "true" : "false";
        }

        public static bool TryParse(string expression,out ArithmeticBoolen boolen)
        {
            expression = expression.Trim();
            if(expression.Equals( "true")||expression.Equals("True")||expression.Equals("TRUE"))
            {
                return boolen = new ArithmeticBoolen(true);
            }
            else if (expression.Equals("false") || expression.Equals("False") || expression.Equals("False"))
            {
                return boolen = new ArithmeticBoolen(false);
            }
            boolen = null;
            return false;
        }
    }


}
