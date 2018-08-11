using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Engine8.EngineCore.Components.Indexers;

namespace Engine8.EngineCore.Systems.Movement.Components.Indexers
{

    /// <summary>
    /// Indexes the PositionComponentCollection by the entity position.
    /// </summary>
    public class PositionComponentIndexer : BasePositionComponentIndexer
    {

        /// <summary>
        /// Creates the PositionComponentCollection position indexer.
        /// </summary>
        /// <param name="componentCollection">PositionComponentCollection.</param>
        public PositionComponentIndexer(PositionComponentCollection componentCollection)
            : base(componentCollection, componentCollection)
        {

        }

    }

}
