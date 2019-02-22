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
4. Add Tilemap with NavMeshModifier component, override the area.
5. In NavMeshSurface2d select Collection Object to Grid.
6. Bake.

How does it works:

1. It uses https://docs.unity3d.com/Manual/class-NavMeshSurface.html as base implementation.
2. Implements world bound calculation from Tilemap bounds.
3. Implements source collector of tiles, because NavMeshBuilder.CollectSources will not work
4. Creates walkable mesh box from world bounds.
5. Convert tiles to sources as NavMeshBuilder would do.
