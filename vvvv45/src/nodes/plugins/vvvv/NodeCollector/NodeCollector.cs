#region usings
using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;

using VVVV.Core;
using VVVV.Core.Logging;
using VVVV.Hosting.Factories;
#endregion usings

namespace VVVV.Nodes
{
	#region PluginInfo
	[PluginInfo(Name = "NodeList", Category = "VVVV", 
	            Help = "Collects nodes in Search Paths and returns the list of all known nodes. For now Search Paths are expected to have nodes sorted in subdirectories by type (effects, plugins, modules...)", 
	            Tags = "", AutoEvaluate = true)]
	#endregion PluginInfo
	public class NodeCollector : IPluginEvaluate
	{
		#region fields & pins
		[Input("Search Paths", StringType = StringType.Directory, DefaultString = ".")]
		protected IDiffSpread<string> FSearchPaths;		

		//[Input("Unsorted Directories", StringType = StringType.Directory)]
		//protected ISpread<string> FUnsortedDirs;
		
		[Output("Nodes")]
		protected ISpread<string> FNodes;

        [Output("Collected Paths")]
        protected ISpread<string> FCollectedPaths;

        [Import]
		protected NodeCollection NodeCollection;
		
		[Import]
		protected ILogger Flogger;
		
		[Import]
		protected INodeInfoFactory FNodesInfoFactory;
		
		List<string> FLastPaths = new List<string>();
		List<bool> FWasRepository = new List<bool>();
		#endregion fields & pins
			
		//called when data for any output pin is requested
		public void Evaluate(int SpreadMax)
		{
			if (!FSearchPaths.IsChanged) return;
			
			foreach (var p in FLastPaths)
				NodeCollection.RemoveCombined(p);				

			FLastPaths.Clear();
			
			foreach (var p in FSearchPaths)
			{
				NodeCollection.AddCombined(p, true);
				FLastPaths.Add(p);
			}

			NodeCollection.Collect();
						
			var nodeInfos =
				from nodeInfo in FNodesInfoFactory.NodeInfos
				where !nodeInfo.Ignore
				select nodeInfo.Systemname;
			
			FNodes.AssignFrom(nodeInfos);
            FCollectedPaths.AssignFrom(NodeCollection.Paths.Select(sp => sp.Dir));
		}
	}

    #region PluginInfo
    [PluginInfo(Name = "NodePaths", Category = "VVVV",
                Help = "Returns a list of all collected search paths.",
                Tags = "")]
    #endregion PluginInfo
    public class NodePathsNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Update")]
        protected ISpread<bool> FUpdate;

        [Output("Search Paths")]
        protected ISpread<string> FCollectedPaths;

        [Import]
        protected NodeCollection NodeCollection;
        #endregion fields & pins

        //called when data for any output pin is requested
        public void Evaluate(int SpreadMax)
        {
            if (FUpdate[0])
                FCollectedPaths.AssignFrom(NodeCollection.Paths.Select(sp => sp.Dir).Distinct().OrderBy(d => d));
        }
    }
}
