using System.Collections.Generic;
using HtmlAgilityPack;

namespace MVCForum.Utilities
{
    public class NodePositions
    {
        public NodePositions(HtmlDocument doc)
        {
            AddNode(doc.DocumentNode);
            Nodes.Sort(new NodePositionComparer());
        }

        private void AddNode(HtmlNode node)
        {
            Nodes.Add(node);
            foreach (HtmlNode child in node.ChildNodes)
            {
                AddNode(child);
            }
        }

        private class NodePositionComparer : IComparer<HtmlNode>
        {
            public int Compare(HtmlNode x, HtmlNode y)
            {
                return x.StreamPosition.CompareTo(y.StreamPosition);
            }
        }

        public List<HtmlNode> Nodes = new List<HtmlNode>();
    }
}
