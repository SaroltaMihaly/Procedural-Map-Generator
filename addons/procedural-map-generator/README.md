# Procedural-Map-Generator

![Initial](https://i.ibb.co/wpmnBm0/initial.png)

The plugin is capable of modifying a selected TileMap in Godot using various algorithms. These algorithms include Cellular Automata, Wave Function Collapse, and Gram-Elites. Different parameters can be specified for these algorithms. The plugin contains an Examples folder to test the map generation.

On the initial interface of the plugin, it is visible that you have the option to select which algorithm you would like to use.

## Cellular Automata:

![Cellular](https://i.ibb.co/GTpKBpL/cellular.png)

You can see a section marked with green text "Drop a TileMap here...", where you can drag the TileMap you want to modify. You can specify the width and height of the map, the width of its border, its density, the smoothness value, and whether to generate it randomly or based on a given seed. Additionally, you can set which TileSet to apply to the TileMap, and specifically which tile type to choose for the walls, as well as the tile type that will be placed next to the walkable areas. By pressing the generate button, you can generate your map with the given parameters.

## Wave Function Collapse:

![Cellular](https://i.ibb.co/0QXMMj9/wfc.png)

You can see the user interface of Wave Function Collapse in the image. Here, you can similarly specify the width and height of your map, and the TileSet used for the TileMap. For the rule set, you need to provide the path to a JSON file where the rule set that constructs your map is described. The dragging and regenerating of the TileMap is the same as mentioned in the Cellular Automata. There is an example JSON file in the plugin’s Examples folder.

## Gram-Elites:

![Gram](https://i.ibb.co/jz4fv6g/gram.png)

To run Gram-Elites, you need a Python interpreter, as our implementation is written in Python, and it is required for execution. Additionally, you can specify the height of the map (the width is derived from the parent maps), the seed, the initial size of the population, the number of iterations, the TileSet to be used, and the tile type for the walls from the TileSet. In the lower section, you can drag TileMaps that you want the algorithm to use for generation, and by pressing the "Save tilemap to txt" button, the plugin saves the maps you have drawn to a specific folder, from where the algorithm processes them during map generation. By pressing the generate button, the plugin runs the Python script, reads the generated map, and modifies the TileMap. If you want a new type of map, delete the previous maps from the MapGenerator/GramElites/vglc_levels/Icarus folder. The Python code belongs to Colan Biemer. (https://github.com/bi3mer/GramElites)

## License

MIT



