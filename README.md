# Procedural planets generator

My somehow butchered take on procedural planets generation.

Engine code is based on my [Lego game](https://github.com/aeroson/lego-game-and-Unity-like-engine)

## Key notes
* height of any point on planet is proceduraly calculated from single function, float getPlanetHeight(float3 directionFromPlanetCenter)
* getPlanetHeight is combination of alot of Perlin and Worley noises
* planet is seperated into chunk LOD tree
* each chunk can be seperated into another 4 chunks thus increasing detail at that position
* chunks have shape of triangle (this turned out to be burden while generating and using textures)
* root chunks are created from icosahedron
* to hide cracks on the boundary of chunks in differend LODs, every chunk has a skirt
* sea sphere is at fixed height
* biome is a surface texture and normal (contains 6 biomes)
* biome is selected based on temperature (distance to poles and altitude) and humidity (distance from sea), 2d control map is used for this
* each chunk has mesh, splat map and normal map, all is generated on GPU using compute shaders
* is capable of generating and displaying planets with the size of Earth and more
* uses Floating Camera Origin to improve rendering precision
* uses View Frustum Culling and Software Z Buffer for occlusion culling

## Screenshots

![](http://i.imgur.com/u4oUq7J.jpg)
![](http://i.imgur.com/PXYALlO.jpg)
![](http://i.imgur.com/ndK0nZM.jpg)


# Links

Good papers

http://www.terrain.dk/terrain.pdf
http://web.mit.edu/cesium/Public/terrain.pdf
http://vertexasylum.com/2010/07/11/oh-no-another-terrain-rendering-paper/
http://hhoppe.com/gpugcm.pdf

Nice tutorials

http://www.neilblevins.com/cg_education/procedural_noise/procedural_noise.html
http://www-cs-students.stanford.edu/~amitp/game-programming/polygon-map-generation/
http://www.decarpentier.nl/scape-procedural-basics

Nice examples & other

http://squall-digital.com/ProceduralGeneration.html
http://orbit.medphys.ucl.ac.uk/news.html
https://smcameron.github.io/space-nerds-in-space/
https://www.gamedev.net/topic/643870-what-terrain-rendering-technique-does-the-outerra-engine-use/
http://outerra.blogspot.cz/2012/11/maximizing-depth-buffer-range-and.html
http://www.planetside.co.uk/forums/index.php?topic=20752.0
https://dip.felk.cvut.cz/browse/pdfcache/lindao1_2007bach.pdf

