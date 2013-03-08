using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordGridGame
{
    class TrieNode
    {
        public char letter;
        public List<TrieNode> nodes;
        public bool isWord = false;
        public TrieNode()
        {
            nodes = new List<TrieNode>();
        }
        public bool Contains(char c)
        {
            foreach(TrieNode node in nodes) {
                if (node.letter == c)
                {
                    return true;
                }
            }
            return false;
        }
        public TrieNode GetChild(char c)
        {
            foreach (TrieNode node in nodes)
            {
                if (node.letter == c)
                {
                    return node;
                }
            }
            return null;
        }
    }
}
