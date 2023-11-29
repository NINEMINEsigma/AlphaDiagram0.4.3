using System;
using System.Collections.Generic;
using AD.BASE;
using AD.Utility;

namespace AD.Graph.Node
{
    [Serializable]
    public class OutOfNodeDefinedRangeException : ADException
    {
        public OutOfNodeDefinedRangeException() : base("Out of range") { }
    }

    [Serializable]
    public class ErrorNodeTypeException : Exception
    {
        public ErrorNodeTypeException() : base("Node Type that cannot convert to target node type") { }
    }

    internal interface INodeTemplateTag { }
    internal interface INodeKey1 : INodeTemplateTag { }
    internal interface INodeKey2 : INodeTemplateTag { }
    internal interface INodeKey3 : INodeTemplateTag { }
    internal interface INodeKey4 : INodeTemplateTag { }
    internal interface INodeKeyMore : INodeTemplateTag { }

    public abstract class _Node : INodeTemplateTag
    {
        public abstract _Node this[int index] { get; set; }
        public abstract bool TryGet(int index, out _Node node);
        public abstract List<_Node> TryGet();
    }
    public class _Node<_Ptr> : _Node, INodeKey1 where _Ptr : _Node<_Ptr>
    {
        private _Ptr first = null;

        public override _Node this[int index]
        {
            get => (index == 0) ? first : throw new OutOfNodeDefinedRangeException();
            set
            {
                if (index == 0)
                {
                    if (value.As<_Ptr>(out var that)) first = that;
                    else throw new ErrorNodeTypeException();
                }
                else throw new OutOfNodeDefinedRangeException();
            }
        }
        public override bool TryGet(int index, out _Node node)
        {
            if (index == 0)
            {
                node = first;
                return true;
            }
            else
            {
                node = null;
                return false;
            }
        }
        public override List<_Node> TryGet()
        {
            return new() { first };
        }

        public _Ptr First { get => first; set => first = value; }
        public _Ptr Next { get => first; set => first = value; }
    }
    public class _Node<_Left, _Right> : _Node, INodeKey2
        where _Left : _Node<_Left, _Right>
        where _Right : _Node<_Left, _Right>
    {
        private _Left first = null;
        private _Right second = null;

        public override _Node this[int index]
        {
            get
            {
                if (index == 0) return first;
                else if (index == 1) return second;
                else throw new OutOfNodeDefinedRangeException();
            }
            set
            {
                if (index == 0)
                {
                    if (value.As<_Left>(out var that)) first = that;
                    else throw new ErrorNodeTypeException();
                }
                else if (index == 1)
                {
                    if (value.As<_Right>(out var that)) second = that;
                    else throw new ErrorNodeTypeException();
                }
                else throw new OutOfNodeDefinedRangeException();
            }
        }
        public override bool TryGet(int index, out _Node node)
        {
            if (index == 0)
            {
                node = first;
                return true;
            }
            else if (index == 1)
            {
                node = second;
                return true;
            }
            else
            {
                node = null;
                return false;
            }
        }
        public override List<_Node> TryGet()
        {
            return new() { first, second };
        }

        #region First
        public _Left First { get => first; set => first = value; }
        public _Left Backward { get => first; set => first = value; }
        public _Left Parent { get => first; set => first = value; }
        public _Left Left { get => first; set => first = value; }
        #endregion
        #region Second
        public _Right Second { get => second; set => second = value; }
        public _Right Forward { get => second; set => second = value; }
        public _Right Child { get => second; set => second = value; }
        public _Right Right { get => second; set => second = value; }
        #endregion
    }
    public class _Node<_Left, _Right, _Top, _Buttom> : _Node, INodeKey4
        where _Left : _Node<_Left, _Right, _Top, _Buttom>
        where _Right : _Node<_Left, _Right, _Top, _Buttom>
        where _Top : _Node<_Left, _Right, _Top, _Buttom>
        where _Buttom : _Node<_Left, _Right, _Top, _Buttom>
    {
        private _Left first = null;
        private _Top second = null;
        private _Right third = null;
        private _Buttom fourth = null;

        public override _Node this[int index]
        {
            get
            {
                if (index == 0) return first;
                else if (index == 1) return second;
                else if (index == 2) return third;
                else if (index == 3) return fourth;
                else throw new OutOfNodeDefinedRangeException();
            }
            set
            {
                if (index == 0)
                {
                    if (value.As<_Left>(out var that)) first = that;
                    else throw new ErrorNodeTypeException();
                }
                else if (index == 1)
                {
                    if (value.As<_Top>(out var that)) second = that;
                    else throw new ErrorNodeTypeException();
                }
                else if (index == 2)
                {
                    if (value.As<_Right>(out var that)) third = that;
                    else throw new ErrorNodeTypeException();
                }
                else if (index == 3)
                {
                    if (value.As<_Buttom>(out var that)) fourth = that;
                    else throw new ErrorNodeTypeException();
                }
                else throw new OutOfNodeDefinedRangeException();
            }
        }
        public override bool TryGet(int index, out _Node node)
        {
            if (index == 0)
            {
                node = first;
                return true;
            }
            else if (index == 1)
            {
                node = second;
                return true;
            }
            else if (index == 2)
            {
                node = third;
                return true;
            }
            else if (index == 3)
            {
                node = fourth;
                return true;
            }
            else
            {
                node = null;
                return false;
            }
        }
        public override List<_Node> TryGet()
        {
            return new() { first, second, third, fourth };
        }

        #region First
        public _Left First { get => first; set => first = value; }
        public _Left Forward { get => first; set => first = value; }
        public _Left Parent { get => first; set => first = value; }
        public _Left Leaf { get => first; set => first = value; }
        public _Left Leaf0 { get => first; set => first = value; }
        public _Left Left { get => first; set => first = value; }
        #endregion
        #region Second
        public _Top Second { get => second; set => second = value; }
        public _Top Leaf1 { get => second; set => second = value; }
        public _Top Top { get => second; set => second = value; }
        #endregion
        #region Third
        public _Right Third { get => third; set => third = value; }
        public _Right Backward { get => third; set => third = value; }
        public _Right Child { get => third; set => third = value; }
        public _Right Leaf2 { get => third; set => third = value; }
        public _Right Right { get => third; set => third = value; }
        #endregion
        #region Second
        public _Buttom Fourth { get => fourth; set => fourth = value; }
        public _Buttom Leaf3 { get => fourth; set => fourth = value; }
        public _Buttom Buttom { get => fourth; set => fourth = value; }
        #endregion
    }

    public class _DictionaryNode_Key<_Ptr> : _Node, INodeKeyMore where _Ptr : _DictionaryNode_Key<_Ptr>
    {
        private Dictionary<int, _Ptr> data = new();

        public override _Node this[int index]
        {
            get
            {
                if (data.ContainsKey(index)) return data[index];
                else throw new OutOfNodeDefinedRangeException();
            }
            set
            {
                if (value == null) data.Remove(index);
                else if (value.As<_Ptr>(out var ptr)) data[index] = ptr;
            }
        }
        public override bool TryGet(int index, out _Node node)
        {
            bool cat = data.TryGetValue(index, out var _node);
            node = _node;
            return cat;
        }
        public override List<_Node> TryGet()
        {
            return data.GetSubListAboutValue<_Node, int, _Ptr>();
        }
    }
    public class _ListNode_Key<_Ptr> : _Node, INodeKeyMore where _Ptr : _ListNode_Key<_Ptr>
    {
        private List<_Ptr> data = new();

        public override _Node this[int index]
        {
            get
            {
                if (data.Count > index) return data[index];
                else throw new OutOfNodeDefinedRangeException();
            }
            set
            {
                if (value == null)
                {
                    if (data.Count > index) data.RemoveAt(index);
                    else throw new OutOfNodeDefinedRangeException();
                }
                else if (value.As<_Ptr>(out var ptr))
                {
                    if (data.Count > index) data[index] = ptr;
                    else throw new OutOfNodeDefinedRangeException();
                }
            }
        }
        public override bool TryGet(int index, out _Node node)
        {
            if (index < data.Count)
            {
                node = data[index];
                return true;
            }
            else
            {
                node = null;
                return false;
            }
        }
        public override List<_Node> TryGet()
        {
            return data.GetSubList<_Ptr, _Node>();
        }
    }

    public class _BidirectionalNode_Key<_Ptr> : _Node<_Ptr, _Ptr> where _Ptr : _Node<_Ptr, _Ptr>
    {

    }
    public class _QuadNode_Key<_Ptr> : _Node<_Ptr, _Ptr, _Ptr, _Ptr> where _Ptr : _Node<_Ptr, _Ptr, _Ptr, _Ptr>
    {

    }
}
