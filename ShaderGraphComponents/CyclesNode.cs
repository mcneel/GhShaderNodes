using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rhino.Geometry;

using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using ccl;
using ccl.ShaderNodes;
using System.Drawing;
using Grasshopper.Kernel.Special;

namespace ShaderGraphComponents
{
	public class CyclesNode : GH_Component
	{

		public ShaderNode ShaderNode { get; private set;  } = null;


		public CyclesNode(string name, string nickname, string description, string category, string subcategory, Type nodetype)
			: base(name, nickname, description, category, subcategory)
		{
			string[] p = { Utils.cleanName(nickname) };
			ShaderNode = Activator.CreateInstance(nodetype, p) as ShaderNode;
			base.PostConstructor();
		}

		protected override void PostConstructor()
		{
			// empty
		}
		/// Iterate over the ShaderNode inputs and register them with the GH component

		protected override void RegisterInputParams(GH_InputParamManager mgr)
		{
			if (ShaderNode.inputs is null) return;
			foreach(var socket in ShaderNode.inputs.Sockets)
			{
				mgr.AddGenericParameter(socket.Name, socket.Name, socket.Name, GH_ParamAccess.item);
			}
			ParameterSetup();
		}

		/// If ShaderNode has outputs register them here
		protected override void RegisterOutputParams(GH_OutputParamManager mgr)
		{
			if (ShaderNode.outputs is null) return;

			foreach (var socket in ShaderNode.outputs.Sockets)
			{
				mgr.AddGenericParameter(socket.Name, socket.Name, socket.Name, GH_ParamAccess.item);
			}
		}

		/// <summary>
		/// Set all input parameters to optional.
		/// </summary>
		protected void ParameterSetup()
		{
			foreach(var p in Params.Input)
			{
				p.Optional = true;
			}
		}


		public override Guid ComponentGuid => Guid.Empty;
		protected override Bitmap Icon => base.Icon;
		private void setdata(int i, ccl.ShaderNodes.Sockets.ISocket s, IGH_DataAccess DA)
		{
			switch (s)
			{
				case ccl.ShaderNodes.Sockets.ClosureSocket a:
				case ccl.ShaderNodes.Sockets.ColorSocket b:
					DA.SetData(i, Color.Gray);
					break;
				case ccl.ShaderNodes.Sockets.VectorSocket c:
				case ccl.ShaderNodes.Sockets.Float4Socket d:
					DA.SetData(i, Vector3d.Zero);
					break;
				case ccl.ShaderNodes.Sockets.FloatSocket e:
				case ccl.ShaderNodes.Sockets.IntSocket f:
					DA.SetData(i, 0.0);
					break;
				case ccl.ShaderNodes.Sockets.StringSocket g:
					DA.SetData(i, "");
					break;
				default:
					throw new InvalidCastException("Unknown socket type");
			}
		}

		private void getdata (int i, ccl.ShaderNodes.Sockets.ISocket s, IGH_DataAccess DA)
		{
			switch (s)
			{
				case ccl.ShaderNodes.Sockets.ClosureSocket a:
					var clos = Utils.readColor(this, DA, i, "couldn't read closure");
					a.Value = clos as object;
					break;
				case ccl.ShaderNodes.Sockets.ColorSocket b:
					var col = Utils.readColor(this, DA, i, "couldn't read color");
					b.Value = Utils.float4FromColor(col);
					break;
				case ccl.ShaderNodes.Sockets.VectorSocket c:
					var vec = Utils.readVector(this, DA, i, "couldn't read vector");
					c.Value = Utils.float4FromVector(vec);
					break;
				case ccl.ShaderNodes.Sockets.Float4Socket d:
					var vecd = Utils.readVector(this, DA, i, "couldn't read vector");
					d.Value = Utils.float4FromVector(vecd);
					break;
				case ccl.ShaderNodes.Sockets.FloatSocket e:
					var fl = Utils.readFloat(this, DA, i, "couldn't read float");
					e.Value = fl;
					break;
				case ccl.ShaderNodes.Sockets.IntSocket f:
					var inr  = Utils.readInt(this, DA, i, "couldn't read integer");
					f.Value = inr;
					break;
				case ccl.ShaderNodes.Sockets.StringSocket g:
					var str = Utils.readString(this, DA, i, "couldn't read string");
					g.Value = str;
					break;
				default:
					throw new InvalidCastException("Unknown socket type");
			}

		}

		protected override void SolveInstance(IGH_DataAccess DA)
		{
			ShaderNode.Name = Utils.cleanName(NickName);
		
			// iterSources
			for(int idx = 0; idx < Params.Input.Count; idx++)
			{
				if (idx >= ShaderNode.inputs.Sockets.Count()) continue;
				var item = Params.Input[idx];
				var tosocket = ShaderNode.inputs[idx];
				tosocket.ClearConnections();
				if (item.SourceCount == 0) continue;
				var isource = item.Sources[0];
				switch (isource.Attributes.Parent)
				{
					case null:
						{
							switch (isource)
							{
								case GH_Param<GH_Number> nr:
									if (nr.NickName.Contains(".")) tosocket.SetValueCode = nr.NickName;
									break;
								case GH_Param<GH_Colour> cl:
									if (cl.NickName.Contains(".")) tosocket.SetValueCode = cl.NickName;
									break;
								default:
									break;
							}
						}
						break;
					default:
						switch (isource.Attributes.Parent.DocObject)
						{
							case CyclesNode cn:
								var cngh = cn as GH_Component;
								var sidx = cngh.Params.Output.FindIndex(p => isource.InstanceGuid == p.InstanceGuid);
								if (sidx > -1)
								{
									var fromsock = cn.ShaderNode.outputs[sidx];
									fromsock.Connect(tosocket);
								}
								break;
							case GH_Cluster cl:
								break;
							case GH_Component gh:
								{
									if (gh.NickName.Contains(".")) tosocket.SetValueCode = gh.NickName;
								}
								break;
						}
						break;
				}
			}


			var inputParameters = from inp in ShaderNode.inputs.Sockets select inp as ccl.ShaderNodes.Sockets.ISocket;
			int inpi = 0;
			foreach(var inpar in inputParameters)
			{
				getdata(inpi, inpar, DA);
				inpi++;
			}

			var outputParameters = from inp in ShaderNode.outputs.Sockets select inp as ccl.ShaderNodes.Sockets.ISocket;

			int outpi = 0;
			foreach(var inpar in outputParameters)
			{
				setdata(outpi, inpar, DA);
				outpi++;
			}
		}
	}
}
