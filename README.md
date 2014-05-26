UnityTileMap
============

A TileMap library for Unity written in C#.

It provides a method to stitch together many Sprites into a single large texture which is much more efficient to render rather than having each tile be a separate GameObject.

## Limitations

 * If you have enough tiles to make the texture larger than the maximum texture size the TileMeshGrid class is supposed to create a grid of textures, but that logic hasn't been written yet.
 * There is no built in support for layering, while there is nothing wrong with creating multiple TileMaps with different z coordinate I would like something more formal.
 
## Credits

 * In-scene tile editing by [EddyEpic](https://github.com/EddyEpic).
 * Based on the technique in Quill18's [YouTube video](https://www.youtube.com/playlist?list=PLbghT7MmckI4qGA0Wm_TZS8LVrqS47I9R).
