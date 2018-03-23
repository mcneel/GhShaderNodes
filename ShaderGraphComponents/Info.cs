using System;
using System.Drawing;
using Grasshopper;
using Grasshopper.Kernel;
using ShaderGraphResources;

namespace ShaderGraphComponents
{
	public class Info : GH_AssemblyInfo
	{
		public Info()
		{
			Instances.ComponentServer.GHAFileLoaded += ComponentServer_GHAFileLoaded;
		}

		private void ComponentServer_GHAFileLoaded(object sender, GH_GHALoadingEventArgs e)
		{
			if (!e.Id.Equals(Id)) return;

			foreach(var et in e.Assembly.ExportedTypes)
			{
				try
				{
					if (Activator.CreateInstance(et) is object i)
					{
					}
				} catch (Exception) { }
			}
		}

		//let loaded = Instances.ComponentServer.GHAFileLoaded

		//let cb (x:GH_GHALoadingEventArgs) =
		//  if x.Id.Equals(it.Id) then
		//    printfn "Match: %A %A" System.DateTime.Now x
		//    printfn "%A" x.FileName
		//    x.Assembly.ExportedTypes
		//    |> Seq.filter (
		//        fun et ->
		//            try
		//              let i = Activator.CreateInstance(et) :?> CyclesNode
		//              true
		//            with
		//              | x -> false
		//      )
		//    |> Seq.map (fun t ->

		//                  let i = Activator.CreateInstance(t) :?> CyclesNode
		//                  i.ShaderNode.GetType(), i.ComponentGuid
		//                )
		//    |> Seq.iter (fun (k, v) ->
		//                  Utils.node_componentmapping.Add(k, v)
		//                  printfn "mapped n %A guid %A" k v)

		//do
		//  loaded |> Observable.subscribe cb |> ignore

		public override string Name => "Shader Nodes";
		public override string Description => "Create shader graphs for Cycles for Rhino";
		public override Guid Id => new Guid("6a051e83-3727-465e-b5ef-74d027a6f73b");
		public override string AuthorName => "Nathan 'jesterKing' Letwory";
		public override string AuthorContact => "nathan@mcneel.com";
		public override Bitmap Icon => Icons.ShaderGraph;
	}
}
