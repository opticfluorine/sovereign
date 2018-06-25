using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Engine8.EngineCore.Entities
{

    /// <summary>
    /// Top-level manager of the entity infrastructure.
    /// </summary>
    /// 
    /// The methods exposed by this class are thread-safe.
    public class EntityManager
    {

        /// <summary>
        /// First block reserved for use by automatic assignment.
        /// </summary>
        public const uint FirstReservedBlock = 0;

        /// <summary>
        /// First block not reserved for use by automatic assignment.
        /// </summary>
        public const uint FirstUnreservedBlock = 16777216;

        /// <summary>
        /// Next block identifier.
        /// </summary>
        private int nextBlock = (int)FirstReservedBlock;

        /// <summary>
        /// Gets a new EntityAssigner.
        /// </summary>
        /// 
        /// This method is thread-safe.
        /// 
        /// <returns>New EntityAssigner over the next available block.</returns>
        public EntityAssigner GetNewAssigner()
        {
            var block = Interlocked.Increment(ref nextBlock);
            return GetNewAssigner((uint) block);
        }

        /// <summary>
        /// Gets a new EntityAssigner for the given block.
        /// </summary>
        /// 
        /// No checking of whether the block is already in use is performed. If
        /// the block is already in use, this may lead to entity ID collisions.
        /// 
        /// This method is thread-safe.
        /// 
        /// <param name="block">Block identifier.</param>
        /// <returns>New EntityAssigner over the given block.</returns>
        public EntityAssigner GetNewAssigner(uint block)
        {
            return new EntityAssigner(block);
        }

    }

}
