# NavMeshPlus

This repo is a proof of concept of NavMeshComponents for Unity 2D
https://docs.unity3d.com/Manual/class-NavMeshSurface.html

# 2D NavMesh

In repo you will find implementation of NavMeshSurface2d for tilemap top down games.

To use it in your project:

1. Copy repo into your Asset folder 
2. Create Empty Object in scene root and rotated respectively to Tilemap (x90;y0;z0)
3. It is mandatory that at least one Tilemap has TilemapCollider2d and CompositeCollider2d components
4. Add NavMeshSurface2d component to Empty Object
5. In NavMeshSurface2d select Collection Object to Grid, Default Area to Unwalkable.

How does it works:

1. It uses https://docs.unity3d.com/Manual/class-NavMeshSurface.html as base implementation
2. Implements world bound calculation for NavMeshBuilder from CompositeCollider2d bounds
3. Implements source collector of tiles, because NavMeshBuilder.CollectSources will not work
4. Creates walkable mesh box from CompositeCollider2d bounds.
5. It uses tiles from Tilemap with CompositeCollider2d component as a unwalkable source.
6. Use X and Z axis for NavMeshBuildSource(), becase of how NavMeshBuilder works
