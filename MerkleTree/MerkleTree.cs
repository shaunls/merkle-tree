using MerkleTree.Types;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MerkleTree
{
    // TODO:
    // Add ability to create leaves from arbitrary binary data blocks of any length (i.e. log support)

    public class MerkleTree
    {
        private readonly List<Task> m_Tasks;
        private readonly Stopwatch m_StopWatch;
        private readonly SHA256Managed m_Sha256Managed;

        private readonly List<MerkleNode> m_BranchNodes;
        private readonly SortedDictionary<int, MerkleNode> m_LeafNodes;

        private bool m_IsVerboseMode;
        private bool m_IsPerfCounterOn;
        private bool m_IsMultithreaded;

        private MerkleNode m_MerkleRoot;
        private ChunkSize m_ChunkSize = ChunkSize.SizeOf4096;

        public int Levels { get; private set; }

        public byte[] RootHash => m_MerkleRoot?.Hash ?? new byte[0];
        public int ChunkSizeInBytes => (int)m_ChunkSize;
        public int TreeSizeInBytes => m_LeafNodes?.Count + m_BranchNodes?.Count * 32 ?? 0;
        public int LeafCount => m_LeafNodes?.Count ?? 0;
        public int BranchCount => m_BranchNodes?.Count ?? 0;

        public MerkleTree()
        {
            m_BranchNodes = new List<MerkleNode>();
            m_LeafNodes = new SortedDictionary<int, MerkleNode>();

            m_Tasks = new List<Task>();
            m_StopWatch = new Stopwatch();

            m_Sha256Managed = new SHA256Managed();
            m_Sha256Managed.Initialize();

            m_IsMultithreaded = true;
        }

        public MerkleTree SetOptions(bool isMultiThreaded = true, bool isPerfCounterOn = false, bool isVerboseMode = false)
        {
            m_IsMultithreaded = isMultiThreaded;
            m_IsPerfCounterOn = isPerfCounterOn;
            m_IsVerboseMode = isVerboseMode;

            return this;
        }

        public MerkleTree CreateFromFile(Stream bytes, ChunkSize chunkSize)
        {
            m_ChunkSize = chunkSize;

            CreateLeafNodes((int)bytes.Length);

            HashLeafNodes(bytes);

            CreateBranchNodes();

            return this;
        }

        /// <summary>
        /// Create the unhashed Leaf MerkleNodes
        /// </summary>
        private void CreateLeafNodes(int sizeBytes)
        {
            var chunkSize = (int)m_ChunkSize;

            // Data is less than our chunk size
            if (sizeBytes < chunkSize)
            {
                var merkleNode = new MerkleNode(0, sizeBytes);
                m_LeafNodes.Add(0, merkleNode);
            }
            else
            {
                // Iterate through the binary data, split into chunks and double hash
                var nodeCount = Math.Ceiling((double)sizeBytes / chunkSize);

                for (var i = 1; i <= nodeCount; i++)
                {
                    // TODO: Clean up the binary chunking methodology
                    var multiplier = i;

                    var length = chunkSize;
                    if ((multiplier - 1) * chunkSize + chunkSize > sizeBytes)
                    {
                        length = sizeBytes - (multiplier - 1) * chunkSize;
                    }

                    var offset = (i - 1) * chunkSize;
                    var merkleNode = new MerkleNode(offset, length);
                    m_LeafNodes.Add(offset, merkleNode);
                }
            }
        }

        /// <summary>
        /// Hash all of the Leaf nodes
        /// </summary>
        private void HashLeafNodes(Stream bytes)
        {
            if (m_LeafNodes?.Count == 0)
            {
                Console.WriteLine("Error: Failed to hash leaf nodes, node list was empty.");
                return;
            }

            if (m_IsPerfCounterOn)
            {
                m_StopWatch.Start();
            }

            foreach (var leafNode in m_LeafNodes)
            {
                // TODO: Find a non-allocatey way of doing this
                // Get a copy of the data from the Stream
                var nodeBytes = new byte[leafNode.Value.Length];
                bytes.Read(nodeBytes, 0, leafNode.Value.Length);

                if (m_IsMultithreaded)
                {
                    m_Tasks.Add(Task.Run(() =>
                    {
                        HashLeafNode(leafNode.Value, nodeBytes);
                    }));
                }
                else
                {
                    HashLeafNode(leafNode.Value, nodeBytes);
                }
            }

            if (m_IsMultithreaded)
            {
                try
                {
                    Task.WaitAll(m_Tasks.ToArray());
                }
                catch (AggregateException e)
                {
                    Console.WriteLine("Error hashing one or more of the binary chunks:");

                    foreach (var inner in e.InnerExceptions)
                    {
                        Console.WriteLine("{0}: {1}", inner.GetType().Name, inner.Message);
                    }
                }
            }

            if (m_IsPerfCounterOn)
            {
                m_StopWatch.Stop();
                Console.WriteLine($"Hashed {m_LeafNodes.Count} leaves in {m_StopWatch.ElapsedMilliseconds}ms. MultiThreaded Hashing:{m_IsMultithreaded}\n");
                m_StopWatch.Reset();
            }
        }

        private void CreateBranchNodes()
        {
            if (m_LeafNodes.Count == 0)
            {
                Console.WriteLine("Error: no leaves were found!");
                return;
            }

            if (m_IsPerfCounterOn)
            {
                m_StopWatch.Start();
            }

            var childNodes = m_LeafNodes.Values.ToList();

            // Recurse the nodes
            while (true)
            {
                if (childNodes.Count == 1)
                {
                    m_MerkleRoot = m_LeafNodes[0];
                }
                else
                {
                    // Holds a list of all the nodes above the child nodes
                    var parentNodes = new List<MerkleNode>();

                    for (var i = 0; i < childNodes.Count; i += 2)
                    {
                        // Get two current nodes
                        var leftNode = childNodes[i];
                        var rightNode = i + 1 < childNodes.Count ? childNodes[i + 1] : null;

                        // Create the parent of these two children
                        var parent = CreateBranchNode(leftNode, rightNode);

                        // Set the new parent node on the children
                        leftNode.SetParent(parent);
                        rightNode?.SetParent(parent);

                        // Add the two child nodes to the Merkle Tree only if they are not leaves
                        if (!leftNode.IsLeafNode)
                        {
                            m_BranchNodes.Add(leftNode);
                        }

                        if (rightNode != null && !rightNode.IsLeafNode)
                        {
                            m_BranchNodes.Add(rightNode);
                        }

                        parentNodes.Add(parent);

                        if (m_IsVerboseMode)
                        {
                            Console.WriteLine($"Level:{Levels}, Node:{parent.Hash.ToHexString()}");
                        }
                    }

                    Levels++;

                    // Parent nodes become the children, and reiterate
                    childNodes = parentNodes;

                    continue;
                }

                break;
            }

            if (m_IsPerfCounterOn)
            {
                m_StopWatch.Stop();
                Console.WriteLine($"Generated {m_BranchNodes.Count} Merkle Nodes in {m_StopWatch.ElapsedMilliseconds}ms.\n");
                m_StopWatch.Reset();
            }
        }

        /// <summary>
        /// Concatenates the left and right MerkleNode hash bytes into a new MerkleNode
        /// </summary>
        /// <returns>
        /// A branch type MerkleNode 
        /// </returns>
        public MerkleNode CreateBranchNode(MerkleNode leftNode, MerkleNode rightNode)
        {
            // If we have an uneven node count, the parent node simply pushes the leaf hash up
            if (rightNode == null)
            {
                return leftNode;
            }

            var len = leftNode.Hash.Length + rightNode.Hash.Length;
            var bytes = new byte[len];
            Buffer.BlockCopy(leftNode.Hash, 0, bytes, 0, leftNode.Hash.Length);
            Buffer.BlockCopy(rightNode.Hash, 0, bytes, leftNode.Hash.Length, leftNode.Hash.Length);

            var hash = m_Sha256Managed.ComputeHash(bytes);

            return new MerkleNode(leftNode, rightNode, hash);
        }

        /// <summary>
        /// SHA256 Hashes the input bytes and uses the output as input for the second hash
        /// </summary>
        public void HashLeafNode(MerkleNode leafNode, byte[] bytes)
        {
            // Double hash the input data to avoid length-extension attacks
            var hash1 = m_Sha256Managed.ComputeHash(bytes);
            var hash2 = m_Sha256Managed.ComputeHash(hash1);

            leafNode.SetLeafHash(hash2);
        }
    }
}
