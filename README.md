# Procedural planets generator

My butchered take on procedural planets generator.
Iam somehow ashamed to say that this is my diploma thesis.
After I turned the thesis in, there was driver update thanks to which some of the GLSL shaders failed to compile or worked incorrectly (uninitialized variables were no longer zeroed).
Crunching on this was the most fun I ever had.

Engine code is based on my [Lego game](https://github.com/aeroson/lego-game-and-Unity-like-engine)

## Key notes
* Height of any point on planet is proceduraly calculated from single function, float getPlanetHeight(float3 directionFromPlanetCenter)
* getPlanetHeight is combination of alot of Perlin and Worley noises.
* Planet is seperated into chunk LOD tree.
* Each chunk can be seperated into another 4 chunks thus increasing detail at that position.
* Chunks have shape of triangle (this turned out to be burden while generating and using textures).
* Root chunks are created from icosahedron.
* To hide cracks on the boundary of chunks in differend LODs, every chunk has a skirt.
* Sea sphere is at fixed height.
* Biome is a surface texture and normal (contains 6 biomes).
* Biome is selected based on temperature (distance to poles and altitude) and humidity (distance from sea), 2d control map is used for this.
* Each chunk has mesh, normal map and biomes splat maps. All of which is generated on GPU using compute shaders.
* Is capable of generating and displaying planets with the size of Earth and more.
* Uses Floating Camera Origin to improve rendering precision close to camera.
* Uses View Frustum Culling and Software Z Buffer for occlusion culling (Software Z Buffer is disabled by default because it didn't work 100%).

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

