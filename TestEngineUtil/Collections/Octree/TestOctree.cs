using Engine8.EngineUtil.Collections.Octree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace TestEngineUtil.Collections.Octree
{

    /// <summary>
    /// Unit tests for the Octree class.
    /// </summary>
    public class TestOctree
    {

        [Fact]
        public void TestCreateEmptyOctree()
        {
            /* Create empty octree. */
            var octree = new Octree<float>(Octree<float>.DefaultOrigin);

            /* Assert that the resulting octree is empty. */
            Assert.Equal(0, octree.Count);
        }

    }

}
