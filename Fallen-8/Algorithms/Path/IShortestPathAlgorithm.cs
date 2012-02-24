﻿// 
// IShortestPathAlgorithm.cs
//  
// Author:
//       Henning Rauch <Henning@RauchEntwicklung.biz>
// 
// Copyright (c) 2012 Henning Rauch
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using Fallen8.API.Plugin;

namespace Fallen8.API.Algorithms.Path
{
    /// <summary>
    /// The interface for path algorithms
    /// </summary>
    public interface IShortestPathAlgorithm : IFallen8Plugin
    {
        /// <summary>
        /// Calculates shortest paths
        /// </summary>
        /// <param name="sourceVertexId">The source vertex identifier.</param>
        /// <param name="destinationVertexId">The destination vertex identifier.</param>
        /// <param name="maxDepth">The maximum depth.</param>
        /// <param name="maxPathWeight">The maximum path weight.</param>
        /// <param name="maxResults">The maximum number of results.</param>
        /// <param name="edgePropertyFilter">Edge property filter delegate.</param>
        /// <param name="edgeFilter">Edge filter delegate.</param>
        /// <param name="adjacentVertexFilter">Vertex filter delegate.</param>
        /// <param name="edgePriority">Edge priority order delegate.</param>
        /// <param name="edgeCost">The edge cost delegate.</param>
        /// <param name="vertexCost">The vertex cost delegate.</param>
        /// <returns>Paths</returns>
        IEnumerable<Path> Calculate(
            Int32 sourceVertexId, 
            Int32 destinationVertexId,
            UInt16 maxDepth = 1,
            Double maxPathWeight = Double.MaxValue,
            UInt16 maxResults = 1,
            PathDelegates.EdgePropertyFilter edgePropertyFilter = null,
            PathDelegates.EdgeFilter edgeFilter = null,
            PathDelegates.AdjacentVertexFilter adjacentVertexFilter = null,
            PathDelegates.EdgePriority edgePriority = null,
            PathDelegates.EdgeCost edgeCost = null,
            PathDelegates.VertexCost vertexCost = null);
    }
}
