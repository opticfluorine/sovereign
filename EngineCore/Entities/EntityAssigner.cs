using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine8.EngineCore.Entities
{

    /// <summary>
    /// Responsible for assigning entity IDs within a block.
    /// </summary>
    ///
    /// This class is not thread-safe. Consider using one or more unique
    /// assigners for each ISystem.
    public class EntityAssigner
    {

        /// <summary>
        /// Entity block assigned to this assigner.
        /// </summary>
        public uint Block { get; private set; }
        
        /// <summary>
        /// Block ID shifted to the upper dword
        /// </summary>
        private readonly ulong shiftBlock;

        /// <summary>
        /// Counter for the local segment of the id.
        /// </summary>
        private ulong localIdCounter = 0;

        /// <summary>
        /// Creates an assigner for the given block.
        /// </summary>
        /// <param name="block">Block.</param>
        public EntityAssigner(uint block)
        {
            Block = block;
            shiftBlock = block << 32;
        }

        /// <summary>
        /// Gets the next ID from this assigner.
        /// </summary>
        /// <returns>Next ID.</returns>
        public ulong GetNextId()
        {
            return shiftBlock | localIdCounter++;
        }

    }

}
