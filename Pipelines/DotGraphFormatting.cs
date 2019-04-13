﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Pipelines
{
    public static class DotGraphFormatting
    {
        static void AppendFormat(string name, string format, StringBuilder result)
        {
            result.AppendLine($@"{name} [{format}]");
        }

        static readonly Dictionary<Type, Action<ILabeledNode, HashSet<NodeMetadata>, StringBuilder>> PipeAppendersByType =
            new Dictionary<Type, Action<ILabeledNode, HashSet<NodeMetadata>, StringBuilder>>
            {
                { typeof(CollectorPipe<>), AppendCollectorPipeFormatting },
                { typeof(FunctionPipe<,>), AppendFunctionPipeFormatting },
                { typeof(InputPipe<>), AppendInputPipeFormatting },
            };

        public static StringBuilder AppendFormatting(ILabeledNode node, HashSet<NodeMetadata> metadata)
        {
            Action<ILabeledNode, HashSet<NodeMetadata>, StringBuilder> processNode = (node_, metadata_, result_) => 
                PipeAppendersByType[node_.GetType().GetGenericTypeDefinition()](node_, metadata_, result_);
            return DotGraph.ProcessTree(node, new StringBuilder(), processNode, delegate { }, metadata);
        }

        private static void AppendInputPipeFormatting(ILabeledNode node, HashSet<NodeMetadata> metadata, StringBuilder result)
        {
            AppendFormat(DotGraph.CheckNameUnique(node, metadata).Name, @"color=green", result);
        }

        private static void AppendFunctionPipeFormatting(ILabeledNode node, HashSet<NodeMetadata> metadata, StringBuilder result)
        {
            ILabeledNode output = ((IFunctionPipe)node).Output;
            var nodeMetadata = DotGraph.CheckNameUnique(output, metadata);
            NodeMetadata functionNodeMetadata = DotGraph.CheckNameUnique(node, metadata);

            string label = nodeMetadata.count == 0 ? "" : $"label={DotGraph.Quoted(nodeMetadata.Node.Name)}, ";
            AppendFormat(nodeMetadata.Name, $@"{label}color=""#9fbff4""", result);

            string functionLabel = functionNodeMetadata.count == 0 ? "" : $"label={DotGraph.Quoted(functionNodeMetadata.Node.Name)}, ";
            AppendFormat(functionNodeMetadata.Name, $@"{functionLabel}shape=invhouse", result);
        }

        private static void AppendCollectorPipeFormatting(ILabeledNode node, HashSet<NodeMetadata> metadata, StringBuilder result)
        {
            var nodeMetadata = DotGraph.CheckNameUnique(node, metadata);
            string label = nodeMetadata.count == 0 ? "" : "label=Collector, ";
            AppendFormat(nodeMetadata.Name, $@"{label}color = ""#c361f4""", result);
        }
    }
}