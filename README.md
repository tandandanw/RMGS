# Random Map Generation System (2d)

RMGS is a random map generation tool. It is based on **Wave Function Collapse (WFC)** algorithm. See [mxgmn](https://github.com/mxgmn)/**[WaveFunctionCollapse](https://github.com/mxgmn/WaveFunctionCollapse)** for details. 

The workflow of RMGS shows below.

![RMGS](D:\Projects\RMGS\Screenshots\RMGS.JPG)

## RMGS structure

- **Projects** - 2 Unity projects show features of RMGS (a game and a GUI interface project).
- **RMGS.Console** - RMGS cml tool, you can assign arguments required by WFC to get image results.
- **RMGS.Core** - where WFC runs. It's a .Net Standard DLL.
- **RMGS.GUI** - useful code of RMGS GUI project.
- **Sample**
- **Screenshots**

## RMGS.Console Guide

Open a cml (maybe cmd.exe or powershell), cd to the directory contains *rmgs.exe*, then

```powershell
.\rmgs.exe -p <path-to-pattern-folder> -c <path-to-constraints-file> -o <path-to-output (optional)>
```

for example

```powershell
.\rmgs.exe -p ..\..\Samples\Grass\Patterns -c ..\..\Samples\Grass\Constraints.xml -o ..\..\Samples\Grass\Results  
```

## Screenshots

![RMGS.Console](D:\Projects\RMGS\Screenshots\RMGS.Console.JPG)

![RMGS.GUI](D:\Projects\RMGS\Screenshots\RMGS.GUI.JPG)

![Game](D:\Projects\RMGS\Screenshots\Game.JPG)