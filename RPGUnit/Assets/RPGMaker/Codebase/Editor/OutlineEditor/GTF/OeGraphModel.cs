using RPGMaker.Codebase.Editor.OutlineEditor.Model;
using System;
using UnityEditor.GraphToolsFoundation.Overdrive;
using UnityEditor.GraphToolsFoundation.Overdrive.BasicModel;

namespace RPGMaker.Codebase.Editor.OutlineEditor.GTF
{
    public class OeGraphModel : GraphModel
    {
        protected override Type GetEdgeType(IPortModel toPort, IPortModel fromPort) {
            return typeof(OutlineEdgeModel);
        }
    }
}