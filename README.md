# Notice
This project is unmaintained, but feel free to fork it.

![](https://raw.githubusercontent.com/mrzapp/openpath/master/Images/logo.png)

### What is it?
It's an automated, very simple path finding package for Unity. It's based on A* and written in UnityScript and C#. I am doing this in tandem with our Deus Ex themed game The Vongott Chronicles

### How to use it?
Refer to the example project for a demonstration. You can select the map type in the "Scanner" object, and then activate the corresponding object (NavMeshTest, WayPointTest or GridTest).

### Screenshots
#### Grid
![](https://raw.githubusercontent.com/mrzapp/openpath/master/Images/grid.jpg)
#### Waypoint
![](https://raw.githubusercontent.com/mrzapp/openpath/master/Images/waypoint.jpg)
#### Navmesh
![](https://raw.githubusercontent.com/mrzapp/openpath/master/Images/navmesh.jpg)

### WARNING!
For reasons unbeknownst to me, when you select an object with the OPPathFinder component on it, and the Unity Inspector is visible, there is a huge memory leak in Unity, causing the whole environment to come to a crawl. Select any other object in the scene (or unfocus/disable the inspector temporarily) and it's fine.

### Features:
- Automatic world bounds calculation
- Raycast scanning and node placement
- Easily adjustable scanning parameters
- Multithreading
- Waypoint, grid and NavMesh based types
- Free & open source!

### License
<a rel="license" href="http://creativecommons.org/licenses/by/4.0/"><img alt="Creative Commons License" style="border-width:0" src="http://i.creativecommons.org/l/by/4.0/88x31.png" /></a><br /><span xmlns:dct="http://purl.org/dc/terms/" property="dct:title">
