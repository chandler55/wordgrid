using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WordGridGame
{
    class Tries
    {
        public TrieNode root = new TrieNode();
        public Tries()
        {
            
        }
        public TrieNode Insert(string s)
        {
            char[] charArray = s.ToCharArray();
            TrieNode node = root;
            foreach (char c in charArray)
            {
                node = Insert(c, node);
            }
            node.isWord = true;
            return root;
        }
        private TrieNode Insert(char c, TrieNode node)
        {
            if (node.Contains(c)) return node.GetChild(c);
            else
            {
                TrieNode t = new TrieNode();
                t.letter = c;
                node.nodes.Add(t);
                return t;
            }
        }
        public bool Contains(string s)
        {
            char[] charArray = s.ToCharArray();
            TrieNode node = root;
            bool contains = true;
            foreach (char c in charArray)
            {
                node = Contains(c, node);
                if (node == null)
                {
                    contains = false;
                    break;
                }
            }
            if ((node == null) || (!node.isWord))
                contains = false;
            return contains;
        }
        private TrieNode Contains(char c, TrieNode node)
        {
            if (node.Contains(c))
            {
                return node.GetChild(c);
            }
            else
            {
                return null;
            }
        }
    }
}
