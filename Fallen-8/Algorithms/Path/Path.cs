﻿// 
// Path.cs
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
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Fallen8.API.Model;

namespace Fallen8.API.Algorithms.Path
{
    /// <summary>
    /// The Path.
    /// </summary>
    public struct Path : IEnumerable<VertexModel>
    {
        #region Properties

        /// <summary>
        /// The path elements
        /// </summary>
        private List<PathElement> _pathElements;

        /// <summary>
        /// The weight of this path
        /// </summary>
        public double Weight;

        #endregion

        #region constructor

        /// <summary>
        /// Creates a new path
        /// </summary>
        /// <param name="maximumLength">Maximum length.</param>
        public Path(Int32 maximumLength = 6)
        {
            _pathElements = new List<PathElement>(maximumLength);
            Weight = 0;
        }

        /// <summary>
        /// Create a new path
        /// </summary>
        /// <param name="maxDepth">Maximum length</param>
        /// <param name="path">The path where the to copy from</param>
        /// <param name="aPathElement">A new path element</param>
        public Path(Int32 maxDepth, Path path, PathElement aPathElement)
        {
            _pathElements = new List<PathElement>(path._pathElements);
            var pathLength = path.GetLength();
            _pathElements.Add(aPathElement);

            Weight = path.Weight + aPathElement.Weight;
        }

        #endregion

        #region public methods

        /// <summary>
        /// Returns the elements of the path
        /// </summary>
        /// <returns>Path elements.</returns>
        public List<PathElement> GetPathElements()
        {
            return _pathElements;
        }

        /// <summary>
        /// Gets the length of the path
        /// </summary>
        /// <returns>Path length</returns>
        public Int32 GetLength()
        {
            return _pathElements == null ? 0 : _pathElements.Count;
        }

        /// <summary>
        /// Adds a path element
        /// </summary>
        /// <param name="pathElement">PathElement.</param>
        public void AddPathElement(PathElement pathElement)
        {
          	_pathElements.Add(pathElement);
          	Weight += pathElement.Weight;
        }

        /// <summary>
        /// Returns the last vertex of the path.
        /// </summary>
        /// <returns>Vertex.</returns>
        public VertexModel GetLastVertex()
        {
            return _pathElements.Last().GetTargetVertex();
        }

        #endregion

        #region IEnumerable<VertexModel> Members

        public IEnumerator<VertexModel> GetEnumerator()
        {
            if (_pathElements != null)
            {
                for (var i = 0; i < _pathElements.Count; i++)
                {
                    var pathElement = _pathElements[i];
                    if (pathElement.Direction == Direction.IncomingEdge)
                    {
                        yield return pathElement.Edge.SourceVertex;
                    }
                    else
                    {
                        yield return pathElement.Edge.TargetVertex;
                    }
                }
            }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}