using System.Collections;
using System.Collections.Generic;

namespace GoDaddy.PublicSuffixData.Internal
{
    internal class DomainSegmentNodeCollection : IEnumerable<DomainSegmentNode>
    {
        private readonly IDictionary<string, DomainSegmentNode> _children = new Dictionary<string, DomainSegmentNode>();

        public DomainSegmentNodeCollection()
        {
        }

        public DomainSegmentNodeCollection(IEnumerable<DomainSegmentNode> nodes)
        {
            foreach (var node in nodes)
            {
                Add(node);
            }
        }

        internal void Add(DomainSegmentNode node)
        {
            _children[node.Segment] = node;
        }

        public IEnumerator<DomainSegmentNode> GetEnumerator()
        {
            return _children.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool ContainsKey(string segment)
        {
            return _children.ContainsKey(segment);
        }

        public DomainSegmentTree this[string segment]
        {
            get { return _children[segment]; }
        }
    }
}
