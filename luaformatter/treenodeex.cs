using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace luaformatter
{
    public class TreeNodeEx : System.Windows.Forms.TreeNode, IDictionaryEnumerator
    {
        private DictionaryEntry nodeEntry;
        private IEnumerator enumerator;

        public TreeNodeEx()
        {
            enumerator = base.Nodes.GetEnumerator();
        }

        public string NodeKey
        {
            get
            {
                return nodeEntry.Key.ToString();
            }

            set
            {
                nodeEntry.Key = value;
            }
        }

        public object NodeValue
        {
            get
            {
                return nodeEntry.Value;
            }

            set
            {
                nodeEntry.Value = value;
            }
        }

        public DictionaryEntry Entry
        {
            get
            {
                return nodeEntry;
            }
        }

        public bool MoveNext()
        {
            bool Success;
            Success = enumerator.MoveNext();
            return Success;
        }

        public object Current
        {
            get
            {
                return enumerator.Current;
            }
        }

        public object Key
        {
            get
            {
                return nodeEntry.Key;
            }
        }

        public object Value
        {
            get
            {
                return nodeEntry.Value;
            }
        }

        public void Reset()
        {
            enumerator.Reset();
        }
    }
}
