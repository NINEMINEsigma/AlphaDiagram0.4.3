using System;

namespace AD.Utility
{
    public abstract class AlgorithmBase
    {
        /// <summary>
        /// 参数类型列表，若为null，则为未知
        /// </summary>
        public abstract Type[] ArgsTypes { get; }
        /// <summary>
        /// 返回值，若为null，则未知
        /// </summary>
        public abstract Type ReturnType { get; }

        public abstract object Invoke(params object[] args);

        /// <summary>
        /// 如果希望算法类被获取时以值类型独立，则实现该函数且不返回null
        /// </summary>
        /// <returns></returns>
        public virtual AlgorithmBase Clone()
        {
            return null;
        }
    }

    /// <summary>
    /// void
    /// </summary>
    public class Void_t
    {
        
    }

    public class AlgorithmDelegate<_Delegate> : AlgorithmBase
    {
        private readonly Type[] _ArgsTypes;
        public override Type[] ArgsTypes => _ArgsTypes;

        private readonly Type _ReturnType;
        public override Type ReturnType => _ReturnType;

        public _Delegate Delegate;

        public AlgorithmDelegate(_Delegate @delegate)
        {
            Delegate = @delegate;
            /*
            if(Delegate.GetType().IsSubclassOf(typeof(Delegate)))
            {
                _ArgsTypes = null;
                _ReturnType = null;
            }
            */
        }
        public AlgorithmDelegate(_Delegate @delegate,Type[] argsT, Type retT)
        {
            Delegate = @delegate;
            _ArgsTypes = argsT;
            _ReturnType = retT;
            /*
            if(Delegate.GetType().IsSubclassOf(typeof(Delegate)))
            {
                _ArgsTypes = null;
                _ReturnType = null;
            }
            */
        }

        public override object Invoke(params object[] args)
        {
            return Delegate.RunMethodByName("Invoke", ReflectionExtension.PublicFlags, args);
        }
    }
}
