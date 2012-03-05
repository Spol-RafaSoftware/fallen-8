// 
//  Fallen8RESTService.cs
//  
//  Author:
//       Henning Rauch <Henning@RauchEntwicklung.biz>
//  
//  Copyright (c) 2012 Henning Rauch
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, version 3 of the License.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Linq;
using System.Collections.Generic;
using Fallen8.API.Model;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Fallen8.API.Plugin;
using Fallen8.API.Index;
using Fallen8.API.Algorithms.Path;
using Fallen8.API.Helper;
using System.IO;
using System.ServiceModel.Web;
using Fallen8.API.Service.REST.Ressource;
using System.Text;

namespace Fallen8.API.Service.REST
{
    /// <summary>
    /// Fallen-8 REST service.
    /// </summary>
    public sealed class Fallen8RESTService : IFallen8RESTService, IDisposable
    {
        #region Data

        /// <summary>
        ///   The internal Fallen-8 instance
        /// </summary>
        private readonly Fallen8 _fallen8;
        
        /// <summary>
        /// The ressources.
        /// </summary>
        private readonly Dictionary<String, MemoryStream> _ressources;

        /// <summary>
        /// The html befor the code injection
        /// </summary>
        private readonly String _frontEndPre;

        /// <summary>
        /// The html after the code injection
        /// </summary>
        private readonly String _frontEndPost;
        
        #endregion
        
        #region Constructor
        
        /// <summary>
        /// Initializes a new instance of the Fallen8RESTService class.
        /// </summary>
        /// <param name='fallen8'>
        /// Fallen-8.
        /// </param>
        public Fallen8RESTService(Fallen8 fallen8)
        {
            _fallen8 = fallen8;
            _ressources = FindRessources();
            _frontEndPre = Frontend.Pre;
            _frontEndPost = Frontend.Post;
        }
 
        #endregion
        
        #region IDisposable Members

        public void Dispose()
        {
            //do nothing atm
        }

        #endregion

        #region IFallen8RESTService implementation
        
        public int AddVertex (VertexSpecification definition)
        {
            #region initial checks

            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }

            #endregion
            
            return _fallen8.CreateVertex(definition.CreationDate, GenerateProperties(definition.Properties)).Id;
        }

        public int AddEdge (EdgeSpecification definition)
        {
            #region initial checks

            if (definition == null)
            {
                throw new ArgumentNullException("definition");
            }
        
            #endregion

            return _fallen8.CreateEdge(definition.SourceVertex, definition.EdgePropertyId, definition.TargetVertex, definition.CreationDate, GenerateProperties(definition.Properties)).Id;
        }

        public Fallen8RESTProperties GetAllVertexProperties (string vertexIdentifier)
        {
            return GetGraphElementProperties (vertexIdentifier);
        }

        public Fallen8RESTProperties GetAllEdgeProperties (string edgeIdentifier)
        {
            return GetGraphElementProperties (edgeIdentifier);
        }

        public List<ushort> GetAllAvailableOutEdgesOnVertex (string vertexIdentifier)
        {
            VertexModel vertex;
            return _fallen8.TryGetVertex(out vertex, Convert.ToInt32(vertexIdentifier)) ? vertex.GetOutgoingEdgeIds() : null;
        }

        public List<ushort> GetAllAvailableIncEdgesOnVertex (string vertexIdentifier)
        {
            VertexModel vertex;
            return _fallen8.TryGetVertex(out vertex, Convert.ToInt32(vertexIdentifier)) ? vertex.GetIncomingEdgeIds() : null;
        }

        public List<int> GetOutgoingEdges (string vertexIdentifier, string edgePropertyIdentifier)
        {
            VertexModel vertex;
            if (_fallen8.TryGetVertex(out vertex, Convert.ToInt32(vertexIdentifier))) 
            {
                ReadOnlyCollection<EdgeModel> edges;
                if (vertex.TryGetOutEdge(out edges, Convert.ToInt32(edgePropertyIdentifier))) 
                {
                    return edges.Select(_ => _.Id).ToList();
                }
            }
            return null;
        }

        public List<int> GetIncomingEdges (string vertexIdentifier, string edgePropertyIdentifier)
        {
            VertexModel vertex;
            if (_fallen8.TryGetVertex(out vertex, Convert.ToInt32(vertexIdentifier))) 
            {
                ReadOnlyCollection<EdgeModel> edges;
                if (vertex.TryGetInEdges(out edges, Convert.ToInt32(edgePropertyIdentifier))) 
                {
                    return edges.Select(_ => _.Id).ToList();
                }
            }
            return null;
        }

        public void Trim ()
        {
            _fallen8.Trim();
        }

        public Fallen8Status Status ()
        {
            var currentProcess = Process.GetCurrentProcess();
            var totalBytesOfMemoryUsed = currentProcess.WorkingSet64;
            
            var freeMem = new PerformanceCounter("Memory", "Available Bytes");
            var freeBytesOfMemory = Convert.ToInt64(freeMem.NextValue());
            
            var vertexCount = _fallen8.GetVertices().Count;
            var edgeCount = _fallen8.GetEdges().Count;
            
            IEnumerable<String> availableIndices;
            Fallen8PluginFactory.TryGetAvailablePlugins<IIndex>(out availableIndices);
            
            IEnumerable<String> availablePathAlgos;
            Fallen8PluginFactory.TryGetAvailablePlugins<IShortestPathAlgorithm>(out availablePathAlgos);
            
            IEnumerable<String> availableServices;
            Fallen8PluginFactory.TryGetAvailablePlugins<IFallen8Service>(out availableServices);
            
            return new Fallen8Status
            {
                AvailableIndexPlugins = new List<String>(availableIndices),
                AvailablePathPlugins = new List<String>(availablePathAlgos),
                AvailableServicePlugins = new List<String>(availableServices),
                EdgeCount = edgeCount,
                VertexCount = vertexCount,
                UsedMemory = totalBytesOfMemoryUsed,
                FreeMemory = freeBytesOfMemory
            };
        }
        
        public Stream GetFrontend()
        {
            if (WebOperationContext.Current != null)
            {
                var baseUri = WebOperationContext.Current.IncomingRequest.UriTemplateMatch.BaseUri;

                WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";

                var sb = new StringBuilder();

                sb.Append(_frontEndPre);
                sb.Append(Environment.NewLine);
                sb.AppendLine("var baseUri = \"" + baseUri.ToString() + "\";" + Environment.NewLine);
                sb.Append(_frontEndPost);

                return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
            }

            return new MemoryStream(Encoding.UTF8.GetBytes("Sorry, no frontend available."));
        }
        
        public Stream GetFrontendRessources(String ressourceName)
        {
            MemoryStream ressourceStream;
            if (_ressources.TryGetValue(ressourceName, out ressourceStream)) 
            {
                var result = new MemoryStream();
                var buffer = new byte[32768];
                int read;
                while ((read = ressourceStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    result.Write(buffer, 0, read);
                }
                ressourceStream.Position = 0;
                result.Position = 0;

                if (WebOperationContext.Current != null)
                {
                    var extension = ressourceName.Split('.').Last();

                    switch (extension)
                    {
                        case "html":
                        case "htm":
                            WebOperationContext.Current.OutgoingResponse.ContentType = "text/html";
                            break;
                        case "css":
                            WebOperationContext.Current.OutgoingResponse.ContentType = "text/css";
                            break;
                        case "gif":
                            WebOperationContext.Current.OutgoingResponse.ContentType = "image/gif";
                            break;
                        case "ico":
                            WebOperationContext.Current.OutgoingResponse.ContentType = "image/ico";
                            break;
                        case "swf":
                            WebOperationContext.Current.OutgoingResponse.ContentType = "application/x-shockwave-flash";
                            break;
                        case "js":
                            WebOperationContext.Current.OutgoingResponse.ContentType = "text/javascript";
                            break;
                        default:
                            throw new ApplicationException(String.Format("File type {0} not supported", extension));
                    }
                }

                return result;
            }
            
            return null;
        }
        
        #endregion
        
        #region private helper

        /// <summary>
        /// Find all ressources
        /// </summary>
        /// <returns>Ressources</returns>
        private static Dictionary<string, MemoryStream> FindRessources()
        {
            var ressourceDirectory = Environment.CurrentDirectory + System.IO.Path.DirectorySeparatorChar + "Service" +
                                     System.IO.Path.DirectorySeparatorChar + "REST" +
                                     System.IO.Path.DirectorySeparatorChar + "Ressource" +
                                     System.IO.Path.DirectorySeparatorChar;

            return Directory.EnumerateFiles(ressourceDirectory)
                .ToDictionary(
                    key => key.Split(System.IO.Path.DirectorySeparatorChar).Last(),
                    CreateMemoryStreamFromFile);
        }

        /// <summary>
        /// Creates a memory stream from a file
        /// </summary>
        /// <param name="value">The path of the file</param>
        /// <returns>MemoryStream</returns>
        private static MemoryStream CreateMemoryStreamFromFile(string value)
        {
            MemoryStream result;

            using (var file = File.OpenRead(value))
            {
                var reader = new BinaryReader(file);
                result = new MemoryStream(reader.ReadBytes((Int32)file.Length));
            }

            return result;
        }

        /// <summary>
        /// Generates the properties.
        /// </summary>
        /// <returns>
        /// The properties.
        /// </returns>
        /// <param name='propertySpecification'>
        /// Property specification.
        /// </param>
        private static PropertyContainer[] GenerateProperties (Dictionary<UInt16, PropertySpecification> propertySpecification)
        {
            PropertyContainer[] properties = null;
            
            if (propertySpecification != null)
            {
                var propCounter = 0;
                properties = new PropertyContainer[propertySpecification.Count];
                
                foreach (var aPropertyDefinition in propertySpecification)
                {
                    properties[propCounter] = new PropertyContainer 
                    { 
                        PropertyId = aPropertyDefinition.Key, 
                        Value = Convert.ChangeType(aPropertyDefinition.Value.Property, Type.GetType(aPropertyDefinition.Value.TypeName, true, true)) 
                    };
                    propCounter++;
                }
            }
        
            return properties;
        }
        
        /// <summary>
        /// Gets the graph element properties.
        /// </summary>
        /// <returns>
        /// The graph element properties.
        /// </returns>
        /// <param name='vertexIdentifier'>
        /// Vertex identifier.
        /// </param>
        private Fallen8RESTProperties GetGraphElementProperties (string vertexIdentifier)
        {
            AGraphElement vertex;
            if (_fallen8.TryGetGraphElement(out vertex, Convert.ToInt32(vertexIdentifier))) 
            {
                return new Fallen8RESTProperties 
                {
                    Id = vertex.Id,
                    CreationDate = Constants.GetDateTimeFromUnixTimeStamp(vertex.CreationDate),
                    ModificationDate = Constants.GetDateTimeFromUnixTimeStamp(vertex.CreationDate + vertex.ModificationDate),
                    Properties = vertex.GetAllProperties().ToDictionary(key => key.PropertyId, value => value.Value.ToString())
                };
            }
            
            return null;
        }
        
        #endregion
    }
}
