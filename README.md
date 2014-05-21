UnityTileMap
============

A TileMap library for Unity written in C#.

It provides a method to stitch together many Sprites into a single large texture which is much more efficient to render rather than having each tile be a separate GameObject.

Limitations
===========

 * There isn't (yet) a decent GUI to create TileMaps in Unity, so it's better suited for games that generate levels through code (like a roguelike) rather than games where the level is created by hand (like a JRPG).

 * If you have enough tiles to make the texture larger than the maximum texture size the TileMeshGrid class is supposed to create a grid of textures, but that logic hasn't been written yet.
 
Credits
=======

Based on the technique in Quill18's [YouTube video](https://www.youtube.com/playlist?list=PLbghT7MmckI4qGA0Wm_TZS8LVrqS47I9R).
