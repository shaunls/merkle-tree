using MerkleTree.Types;
using System;
using Xunit;

namespace MerkleTree.Tests
{
    public class TypeTests
    {
        [Fact]
        public void ValidateChunkSizeType()
        {
            var names = Enum.GetNames(typeof(ChunkSize));
            var values = Enum.GetValues(typeof(ChunkSize));

            for(var i=0; i < names.Length; i++)
            {
                var name = names[i];
                var value = (int)values.GetValue(i);

                var result = name.Contains(value.ToString());

                Assert.True(result, $"{name} is not equal to {value}");
            }
        }
    }
}
