using MerkleTree.Types;
using System.IO;
using System.Text;
using Xunit;

namespace MerkleTree.Tests
{
    public class MerkleTreeTests
    {
        [Theory]
        [InlineData("0")]
        public void TestRootHashWithSimpleInput(string value)
        {
            var data = new MemoryStream(Encoding.ASCII.GetBytes(value));
            var merkleTree = new MerkleTree()
                .SetOptions(isMultiThreaded: false, isPerfCounterOn: false, isVerboseMode: false)
                .CreateFromFile(data, ChunkSize.SizeOf16);


            switch (value)
            {
                case "0":
                    var result = merkleTree.RootHash.ToHexString();
                    var expected = "123456";
                    Assert.True(result == expected, $"Hash for case '{data.ToArray().ToHexString()}' expected '{expected}' but got '{result}'.");
                    break;
            }
        }
    }
}
