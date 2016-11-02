type Graph<'LeafData, 'NodeData> =
  | Leaf of 'LeafData
  | Node of 'NodeData * Graph<'LeafData, 'NodeData> seq


module Graph =
  let rec cata fLeaf fNode (tree: Graph<'LeafData, 'NodeData>) : 'r =
    let recurse = cata fLeaf fNode
    match tree with
    | Leaf leaf ->
      fLeaf leaf
    | Node (node,subtrees) ->
      fNode node (subtrees |> Seq.map recurse)

  let rec fold fLeaf fNode acc (tree: Graph<'LeafData, 'NodeData>) : 'r =
    let recurse = fold fLeaf fNode
    match tree with
    | Leaf leaf ->
      fLeaf acc leaf
    | Node (node, subtrees) ->
      let localacc = fNode acc node
      let finalacc = subtrees |> Seq.fold recurse localacc
      finalacc

  let rec map fLeaf fNode (tree:Graph<'LeafData,'NodeData>) =
      let recurse = map fLeaf fNode
      match tree with
      | Leaf leafInfo -> 
          let newLeafInfo = fLeaf leafInfo
          Leaf newLeafInfo 
      | Node (nodeInfo,subtrees) ->
          let newSubtrees = subtrees |> Seq.map recurse
          let newNodeInfo = fNode nodeInfo
          Node (newNodeInfo, newSubtrees)

  let rec iter fLeaf fNode (tree:Graph<'LeafData,'INodeData>) =
      let recurse = iter fLeaf fNode
      match tree with
      | Leaf leafInfo -> 
          fLeaf leafInfo
      | Node (nodeInfo,subtrees) ->
          subtrees |> Seq.iter recurse
          fNode nodeInfo


module Shader =

  /// the leaf - value of a (input) socket
  type SocketValue = {Name: string; InfoData: float}
  /// Either a socket with a link or a node (with sockets).
  /// Form is [node] -> [socket: output] -> [socket: input] -> [node]
  ///      or [socket value] -> [socket: input] -> [node]
  type ShaderNode = {Name: string; NodeData: float}

  type Socket = string * float
  type Sockets = Socket seq

  type ShaderGraph = Graph<Sockets, ShaderNode>

  let createSocket (name:string) (value:float) : Socket = (name, value)
  let fromInfo (leaf:Sockets) =
    Leaf leaf

  let fromComponent(comp:ShaderNode) subitems =
    Node (comp, subitems)

  let countComponents item =
    let fLeaf acc (leaf:Sockets) =
      acc + 0
    let fNode acc (node:ShaderNode) =
      acc + 1
    Graph.fold fLeaf fNode 0 item

  let countSockets item =
    let fLeaf acc (leaf:Sockets) =
      acc + (Seq.length leaf)
    let fNode acc (nod:ShaderNode) =
      acc
    Graph.fold fLeaf fNode 0 item

  let socka = createSocket "sockA" 1.0
  let sockb = createSocket "sockB" 2.0

  let aleaf = fromInfo [socka; sockb]
  let bsdf = fromComponent {Name = "Diffuse"; NodeData=0.3} [aleaf]
  let sss = fromComponent {Name = "SSS"; NodeData=1.2} []
  let mix = fromComponent {Name="comp"; NodeData=3.0} [bsdf; sss]
  let outp = fromComponent {Name="outp"; NodeData=0.3} [mix]
  let bg = fromComponent {Name="bg"; NodeData = 0.11} [outp; mix]

  let nrcmps = countComponents outp
  let nrscks = countSockets outp
