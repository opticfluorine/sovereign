/*
 * Sovereign Engine
 * Copyright (c) 2018 opticfluorine
 *
 * Permission is hereby granted, free of charge, to any person obtaining a 
 * copy of this software and associated documentation files (the "Software"), 
 * to deal in the Software without restriction, including without limitation 
 * the rights to use, copy, modify, merge, publish, distribute, sublicense, 
 * and/or sell copies of the Software, and to permit persons to whom the 
 * Software is furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER 
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING 
 * FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
 * DEALINGS IN THE SOFTWARE.
 */

using Sovereign.ClientCore.Rendering.Graph.Actions;
using System.Collections.Generic;

namespace Sovereign.ClientCore.Rendering.Graph
{

    /// <summary>
    /// Describes the rendering options to be performed while generating the next frame.
    /// </summary>
    /// 
    /// The rendering graph is a tree that is traversed in depth-first order. The children
    /// of each node, if any, are visited in order of priority. Rendering actions local to
    /// the node are executed after all children have been traversed. The order in which
    /// local rendering actions are performed is undefined.
    public sealed class RenderingGraph
    {

        /// <summary>
        /// Unique priority of this node. Lower priority nodes are visited earlier.
        /// </summary>
        public int Priority { get; private set; }

        /// <summary>
        /// Child nodes in priority order.
        /// </summary>
        public ISet<RenderingGraph> ChildNodes { get; private set; }
            = new SortedSet<RenderingGraph>(
                Comparer<RenderingGraph>.Create(
                    (a, b) => Comparer<int>.Default.Compare(a.Priority, b.Priority)));

        /// <summary>
        /// Rendering actions local to this node.
        /// </summary>
        public ISet<IRenderingAction> RenderingActions { get; private set; }
            = new HashSet<IRenderingAction>();

        public RenderingGraph(int priority)
        {
            Priority = priority;
        }

        /// <summary>
        /// Adds a child node to the current node.
        /// Does nothing if the child node was already added.
        /// </summary>
        /// <param name="childNode">Child node to add.</param>
        public void AddChildNode(RenderingGraph childNode)
        {
            ChildNodes.Add(childNode);
        }

        /// <summary>
        /// Removes a child node from the current node.
        /// Does nothing if the child node is not present.
        /// </summary>
        /// <param name="childNode">Child node to remove.</param>
        public void RemoveChildNode(RenderingGraph childNode)
        {
            ChildNodes.Remove(childNode);
        }

        /// <summary>
        /// Adds a rendering action to the current node.
        /// Does nothing if the rendering action was already added.
        /// </summary>
        /// <param name="renderingAction">Rendering action to add.</param>
        public void AddRenderingAction(IRenderingAction renderingAction)
        {
            RenderingActions.Add(renderingAction);
        }

        /// <summary>
        /// Removes a rendering action from the current node.
        /// Does nothing if the rendering action was already removed.
        /// </summary>
        /// <param name="renderingAction">Rendering action to remove.</param>
        public void RemoveRenderingAction(IRenderingAction renderingAction)
        {
            RenderingActions.Remove(renderingAction);
        }

        public override int GetHashCode() => Priority;

    }

}
