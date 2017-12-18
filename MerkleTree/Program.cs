// Copyright (c) 2017 Shaun Sales. All rights reserved.
// Use of this source code is governed by a MIT license that can be found
// in the LICENSE file in the project root or at https://opensource.org/licenses/MIT
using MerkleTree.Types;
using System;
using System.IO;

namespace MerkleTree
{
    /// <remarks>
    /// To build executables:
    /// $ dotnet publish -c Release -r win10-x64
    /// $ dotnet publish -c Release -r ubuntu.16.10-x64
    /// </remarks>
    internal static class Program
    {
        private static void Main(string[] args)
        {
            // Options
            // - Chunk size
            // - Double hash branch nodes
            // - Use pass through hashing for single element nodes
            // - Swap Endianness for hash storage/display

            // Use cases
            // What to use as input (file and split into chunks, group of files etc)
            // What to output (serialized Merkle Tree, Chunked Data)
            // How to validate a chunk of data (input a Merkle Root, and leaves)

            // Create some random binary data
            var kb = (int)Math.Pow(2, 10);
            var mb = (int)Math.Pow(2, 20);

            var stream = GenerateRandomStream(kb);

            var merkleTree = new MerkleTree()
                .SetOptions(isMultiThreaded: false, isPerfCounterOn: true, isVerboseMode: true)
                .CreateFromFile(stream, ChunkSize.SizeOf64);

            // Output Merkle Tree Generation Statistics
            Console.WriteLine($"Completed genereating Merkle Tree for {stream.Length.ToFileSizeString()} of data!");
            Console.WriteLine($"Leaves:{merkleTree.LeafCount}x{merkleTree.ChunkSizeInBytes.ToFileSizeString()}, Nodes:{merkleTree.BranchCount}, Tree Levels:{merkleTree.Levels}, Tree Size:{merkleTree.TreeSizeInBytes.ToFileSizeString()}");
            Console.WriteLine($"Tree To Data Ratio:{stream.Length / merkleTree.TreeSizeInBytes:N2}:1");
            Console.WriteLine($"Merkle Root:{merkleTree.RootHash.ToHexString()}");
            Console.ReadLine();
        }

        private static Stream GenerateRandomStream(int sizeInBytes)
        {
            var random = new Random();
            var bytes = new byte[sizeInBytes];

            random.NextBytes(bytes);

            Console.WriteLine($"Generated {bytes.Length.ToFileSizeString()} bytes of random data.\n");

            return new MemoryStream(bytes);
        }
    }
}
