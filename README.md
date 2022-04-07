# NavMeshPlus

NavMesh Extensions building components provide you with ability to create navigation meshes that are generated automatically from your Scene
geometry, which allows characters to move intelligently around the game world.

![NavMesh](https://github.com/h8man/NavMeshPlus/wiki/images/NavMesh-04.png)

# Unity 2D Pathfinding 

This repo is fork of Unity NavMeshComponents enhanced with Extensions system for 2d Pathfinding and more. [[link]](https://docs.unity3d.com/Manual/class-NavMeshSurface.html)

## Wiki [[here]](https://github.com/h8man/NavMeshPlus/wiki)

See [how-to](https://github.com/h8man/NavMeshPlus/wiki/HOW-TO) for full tutorial

# Setup

You can use this in two different ways: downloading this repository or adding it to your project's Package Manager manifest.
Alternatively, you can pick scripts and place in your project's `Assets` folder.

### Variant 1. Download
Download or clone this repository into your project in the folder `Packages/com.h8man.2d.navmeshplus`.

### Variant 2. Package Manager Manifest
[Git](https://git-scm.com/) must be installed and added to your path.

The following line needs to be added to your `Packages/manifest.json` file in your Unity Project under the `dependencies` section:

```
"com.h8man.2d.navmeshplus": "https://github.com/h8man/NavMeshPlus.git#master"
```
# Misc

How To [[pdf]](https://github.com/h8man/NavMeshPlus/blob/master/navmeshplus.pdf ).

Demo [[github]](https://github.com/h8man/RedHotSweetPepper ).

Discuss [[unityforum]](https://forum.unity.com/threads/2d-navmesh-pathfinding.503596/ ).

# 2D NavMesh

In repo you will find implementation of NavMeshSurface and 2d Extensions for tilemap, sprites and collider2d top down games.

To use it in your project:

1. Copy repo into your `Asset` folder (or install as a package).
2. Create `Empty Object` in scene root.
3. Add `NavMeshSurface` component to `Empty Object` and add `NavMeshCollecSources2d` component after.
4. Click `Rotate Surface to XY` (to face surface toward standard 2d camera x-90;y0;z0)
4. Add `NavMeshModifier` component to scene objects obstacles, override the area.
5. In `NavMeshSurface` hit Bake.

How does it works:

1. It uses https://docs.unity3d.com/Manual/class-NavMeshSurface.html as base implementation.
2. Implements world bound calculation.
3. Implements source collector of tiles, sprites and 2d colliders
4. Creates walkable mesh box from world bounds.
5. Convert tiles, sprites and 2d colliders to sources as `NavMeshBuilder` would do.
