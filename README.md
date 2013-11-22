### What is it?
It's an automated, very simple path finding package for Unity. It's based on A* and written in UnityScript. I am doing this in tandem with our Deus Ex themed game The Vongott Chronicles

### How to use it?
Refer to the sample project for a demonstration. You can select the map type in the "Scanner" object, and then activate the corresponding object (NavMeshTest, WayPointTest or GridTest).

### WARNING!
For reasons unbeknownst to me, when you select an object with the OPPathFinder component on it, and the Unity Inspector is visible, there is a huge memory leak in Unity, causing the whole environment to come to a crawl. Select any other object in the scene (or unfocus/disable the inspector temporarily) and it's fine.

### Features:
- Automatic world bounds calculation
- Raycast scanning and node placement
- Easily adjustable scanning parameters
- Multithreading
- Waypoint, grid and NavMesh based types
- Free & open source!

### How to get it?
Get the latest package [here](https://www.dropbox.com/s/vp7iws704kkykow/openpath.zip)

### Why is this not C#?
I know, I know, it's more popular, but I prefer the UnityScript syntax. When I am done with my project and have some free time in 6-7 years or so, when Unity has probably become completely irrelevant, I'll rewrite it in C# If anyone wants to rewrite it, I'll gladly do a fork of it on GitHub and make them a contributor.
