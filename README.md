ShaderGraph plug-in for Grasshopper
===================================

This repository contains  a solution with three sub-projects, two of which are
plug-ins.

NOTE: Beware that the project file(s) contain conditionally referenced
assemblies, so they don't show up in the Visual Studio editor Solution
Explorer. This means that adding new assemblies is to be a MANUAL process
(at least until the day that Visual Studio IDE properly can handle these).

FsShaderGraphComponents
=======================

This plug-in provides components with which shader graphs can be build. This
plug-in is written in F#.

CyclesRenderer
==============

This plug-in provides a component that integrates the Cycles render engine as
a renderer onto the Grasshopper canvas. This plug-in is written in F#.

ShaderGraphResources
====================

Provides shared resources for the two plug-ins. Currently it provides the icons
for components and plug-ins.
