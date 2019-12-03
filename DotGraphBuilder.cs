using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentApi.Graph
{
	
	public static class DotGraphBuilder
	{
        private static GraphProperties graphProperties;

		public static GraphProperties DirectedGraph(string graphName)
		{
            graphProperties = new GraphProperties(graphName, true);
            return graphProperties;
		}

        public static GraphProperties NondirectedGraph(string graphName)
        {
            graphProperties = new GraphProperties(graphName, false);
            return graphProperties;
        }
    }

    public class GraphProperties
    {
        private IGraphPart selectedEntity;

        private string graphName;

        private bool isDirected;

        private Dictionary<IGraphPart,Properties> propeties = new Dictionary<IGraphPart, Properties>();

        public GraphProperties(string graphName, bool isDirected)
        {
            this.graphName = graphName;
            this.isDirected = isDirected;
        }

        public GraphProperties AddNode(string nodeName)
        {
            selectedEntity = new NodePart(nodeName);
            propeties.Add(selectedEntity, null);
            return this;
        } 

        public GraphProperties AddEdge(string startNode, string endNode)
        {
            selectedEntity = new EdgePart(Tuple.Create(startNode, endNode));
            propeties.Add(selectedEntity, null);
            return this;
        }

        public GraphProperties With(Action<Properties> attributeConfigurator)
        {
            if (selectedEntity == null)
                throw new InvalidOperationException("With() invoked before AddNode() or AddEdge()");
            var properties = new Properties(selectedEntity.GetType());
            attributeConfigurator(properties);
            propeties[selectedEntity] = properties;
            return this;
        }

        public string Build()
        {
            var graph = new Graph(graphName, isDirected, true);
            foreach (var nodeProperties in propeties)
            {
                AddPart(nodeProperties.Key,nodeProperties.Value);
            }
            void AddPart<T>(T part,Properties properties)
            {
                if (part is NodePart node)
                {
                    graph.AddNode(node.Name);
                    var item = graph.Nodes.Last();
                    if (properties != null)
                        foreach (var attr in properties.Attributes)
                        {
                            item.Attributes.Add(attr.Key, attr.Value);
                        }
                }

                if (part is EdgePart edge)
                {
                    graph.AddEdge(edge.EndNodes.Item1, edge.EndNodes.Item2);
                    var item = graph.Edges.Last();
                    if (properties != null)
                        foreach (var attr in properties.Attributes)
                        {
                            item.Attributes.Add(attr.Key, attr.Value);
                        }
                }
            }
            return graph.ToDotFormat();
        }
    }

    public interface IGraphPart
    {
    }

    public class NodePart : IGraphPart
    {
        public string Name { get; }

        public NodePart(string name) => Name = name;
    }

    public class EdgePart : IGraphPart
    {
        public Tuple<string,string> EndNodes { get; }

        public EdgePart(Tuple<string, string> endNodes) => EndNodes = endNodes;
    }

    public class Properties
    {
        private readonly Type partType;

        public Dictionary<string, string> Attributes { get; } = new Dictionary<string, string>();
        
        public Properties(Type type)
        {
            partType = type;
        }
        
        public Properties Color(string color)
        {
            Attributes.Add("color", color);
            return this;
        }

        public Properties FontSize(int size)
        {
            Attributes.Add("fontsize", size.ToString());
            return this;
        }

        public Properties Label(string label)
        {
            Attributes.Add("label", label);
            return this;
        }

        public Properties Weight(int weight)
        {
            ThrowsIfIncorrectType(typeof(EdgePart),
                                  $"The {partType.Name} have no Weigth attribute");
            Attributes.Add("weight", weight.ToString());
            return this;
        }

        public Properties Shape(NodeShape shape)
        {
            ThrowsIfIncorrectType(typeof(NodePart),
                                  $"The {partType.Name} have no Shape attribute");
            Attributes.Add("shape", shape.ToString().ToLower());
            return this;
        }

        private void ThrowsIfIncorrectType(Type type,string errorMessage)
        {
            if (partType != type)
                throw new InvalidOperationException(errorMessage);
        }
    }

    public enum NodeShape
    {
        Box,
        Ellipse
    }

}