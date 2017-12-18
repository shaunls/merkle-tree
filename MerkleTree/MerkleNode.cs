// Copyright (c) 2017 Shaun Sales. All rights reserved.
// Use of this source code is governed by a MIT license that can be found
// in the LICENSE file in the project root or at https://opensource.org/licenses/MIT

namespace MerkleTree
{
    // TODO:
    // Create an interface for IMerkleNode
    // Create class for MerkleBranchNode
    // Create class for MerkleLeafNode

    public class MerkleNode
    {
        public int Offset { get; }
        public int Length { get; }
        public byte[] Hash { get; private set; }

        public MerkleNode LeftNode { get; }
        public MerkleNode RightNode { get; }

        public MerkleNode Parent { get; private set; }

        public bool IsLeafNode => Offset > -1;

        public MerkleNode(MerkleNode leftNode, MerkleNode rightNode, byte[] hash)
        {
            LeftNode = leftNode;
            RightNode = rightNode;
            Hash = hash;
            
            // This is not a leaf node, don't link to a binary chunk
            Offset = -1;
        }

        public MerkleNode(int offset, int length)
        {
            Offset = offset;
            Length = length;
        }

        public void SetLeafHash(byte[] hash)
        {
            Hash = hash;
        }

        public void SetParent(MerkleNode parentNode)
        {
            Parent = parentNode;
        }
    }
}
