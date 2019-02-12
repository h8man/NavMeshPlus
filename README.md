# NavMeshPlus

NavMesh building components provide you with ability to create navigation meshes that are generated automatically from your Scene
geometry, which allows characters to move intelligently around the game world.

# Unity NavMesh 2D Pathfinding 

This repo is a proof of concept of Unity NavMesh and Pathfinding in 2D. It is explores NavMeshComponents abilities for Unity 2D. (https://docs.unity3d.com/Manual/class-NavMeshSurface.html)

# 2D NavMesh

In repo you will find implementation of NavMeshSurface2d for tilemap top down games.

To use it in your project:

1. Copy repo into your Asset folder 
2. Create Empty Object in scene root and rotated respectively to Tilemap (x-90;y0;z0)
3. Add NavMeshSurface2d component to Empty Object
4. Tilemap with TilemapCollider2d will be carved out as Unwalkable, unless overriden by NavMeshModifier 
5. In NavMeshSurface2d select Collection Object to Grid, Default Area to Unwalkable.

How does it works:

1. It uses https://docs.unity3d.com/Manual/class-NavMeshSurface.html as base implementation
2. Implements world bound calculation from Tilemap bounds (seems to be the same within Grid)
3. Implements source collector of tiles, because NavMeshBuilder.CollectSources will not work
4. Creates walkable mesh box from world bounds.
5. It uses tiles from Tilemap with TilemapCollider2d component as a unwalkable source.
6. Use X and Z axis for NavMeshBuildSource(), becase of how NavMeshBuilder works
