using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sovereign.ClientCore.Rendering.Graph
{

    /// <summary>
    /// Interface for creating and updating rendering graphs.
    /// </summary>
    public interface IRenderingGraphUpdater
    {

        /// <summary>
        /// Creates or updates the rendering graph.
        /// </summary>
        /// <param name="currentGraph">Current graph, or null if the graph must be created.</param>
        /// <returns>Created or updated rendering graph.</returns>
        RenderingGraph CreateOrUpdateGraph(RenderingGraph currentGraph);

    }

}
