using System;
using System.Collections;
using System.Collections.Generic;
using AD.BASE;
using AD.Graph.Node;

namespace AD.Graph.Iterator
{
    [Serializable]
    public class OutOfIteratorRangeException : ADException
    {
        public OutOfIteratorRangeException() : base("Out of range") { }
    }

    [Serializable]
    public class ErrorIteratorOperationException : Exception
    {
        public ErrorIteratorOperationException() : base("Iterator operation is work error") { }
    }

    public class Iterator<_T_Node> : IEnumerator, IEnumerator<_T_Node> where _T_Node : _Node
    {
        private readonly _T_Node root;
        private _T_Node currentNode;

        public Iterator() { currentNode = null; }
        public Iterator(_T_Node node) { root = currentNode = node; }

        public _T_Node CurrentNode { get => currentNode; set => currentNode = value; }

        public _T_Node Current => CurrentNode;

        object System.Collections.IEnumerator.Current => CurrentNode;

        public virtual void Dispose()
        {
        }

        public _T_Node GetCurrent() => CurrentNode;

        public virtual bool MoveNext()
        {
            var temp = (_T_Node)currentNode[1];
            if (temp == null) return false;
            currentNode = temp;
            return true;
        }

        public virtual void Reset()
        {
            currentNode = root;
        }
    }
}
