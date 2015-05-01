namespace GoDaddy.PublicSuffixData.Internal
{
    internal class DomainSegmentTree
    {
        internal DomainSegmentNodeCollection Children { get; set; }

        public bool Contains(string segment)
        {
            return Children != null && Children.ContainsKey(segment);
        }

        public DomainSegmentTree this[string segment]
        {
            get 
            {
                return Children != null && Children.ContainsKey(segment) ? Children[segment] : null; 
            }
        }

        public DomainSegmentTree Add(string segment)
        {
            var node = new DomainSegmentNode {Segment = segment};
            (Children ?? (Children = new DomainSegmentNodeCollection())).Add(node);
            return node;
        }
    }
}
