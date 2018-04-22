using System;
using System.Collections.Generic;
using System.Text;

namespace jemml.Data.Transform.Transform.Wavelet
{
    public class WaveletPacket
    {
        public int DecompositionLevel { get; private set; }
        public int SubspaceIndex { get; private set; }
        public double[] Coefficients { get; private set; }
        public string Identifier { get; private set; }
        public WaveletPacket Parent { get; private set; }
        public WaveletPacket Sibling { get; private set; }
        public WaveletPacket LeftChild { get; private set; }
        public WaveletPacket RightChild { get; private set; }

        public WaveletPacket(double[] coefficients, string identifier, WaveletPacket parent = null, int subspaceIndex = 0)
        {
            if (parent != null)
            {
                this.DecompositionLevel = parent.DecompositionLevel + 1;
            }
            else
            {
                this.DecompositionLevel = 0;
            }

            this.SubspaceIndex = subspaceIndex;
            this.Parent = parent;
            this.Sibling = null;
            this.LeftChild = null;
            this.RightChild = null;
            this.Coefficients = coefficients;
            this.Identifier = identifier;
        }

        public void SetChildren(WaveletPacket leftChild, WaveletPacket rightChild)
        {
            this.LeftChild = leftChild;
            this.RightChild = rightChild;
            this.LeftChild.Sibling = rightChild;
            this.RightChild.Sibling = leftChild;
        }

        public WaveletPacket FindSubspace(WaveletSubspace subspace)
        {
            // if this isn't the top of the tree move to the top of the tree
            WaveletPacket top = this;
            while (top.Parent != null)
            {
                top = top.Parent;
            }

            if (subspace.DecompositionLevel == top.DecompositionLevel)
            {
                return top;
            }
            // do a recursive tree search for the desired subspace and return it
            return SearchSubspace(top, subspace);
        }

        private WaveletPacket SearchSubspace(WaveletPacket top, WaveletSubspace subspace)
        {
            if (subspace.DecompositionLevel == top.LeftChild.DecompositionLevel)
            {
                if (subspace.SubspaceIndex == top.LeftChild.SubspaceIndex)
                {
                    return top.LeftChild;
                }
                return top.RightChild;
            }
            else
            {
                int levelDiff = (subspace.DecompositionLevel - top.LeftChild.DecompositionLevel);
                int maxLeftIndex = top.LeftChild.SubspaceIndex * (levelDiff * 2) + ((int)Math.Pow(2, levelDiff) - 1);

                if (subspace.SubspaceIndex <= maxLeftIndex)
                {
                    return SearchSubspace(top.LeftChild, subspace);
                }
                return SearchSubspace(top.RightChild, subspace);
            }
        }
    }
}
