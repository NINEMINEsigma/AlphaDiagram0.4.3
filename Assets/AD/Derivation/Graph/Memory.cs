using Unity.Collections;

namespace AD.Graph.Memory
{
    public interface I_void_Tidy_void
    {
        void _Tidy();
    }
    public interface I_void_Tidy_deallocate_void
    {
        void _Tidy_deallocate();
    }

    public class _Tidy_guard<_Ty> where _Ty : I_void_Tidy_void
    {
        public virtual _Ty _Target { get; set; }
        ~_Tidy_guard()
        {
            _Target?._Tidy();
        }
    }

    public class _Tidy_deallocate_guard<_Ty> where _Ty : I_void_Tidy_deallocate_void
    {
        public virtual _Ty _Target { get; set; }
        ~_Tidy_deallocate_guard()
        {
            _Target?._Tidy_deallocate();
        }
    }
}
