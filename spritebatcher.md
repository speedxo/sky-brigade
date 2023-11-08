# Intro
Im writing this mostly due to my incompetent memory but also because I wanna look back in the future and think "wow, this guys an idiot"

So first things first before we write any code we need to define what a spritebatch is and if its even useful what it accomplishes and how useful it actually is (why bother learning something impractical right?)

In the context of game engines, a spritebatch is the colloquial term for an object that batches together the rendering procedures associated with rendering a large quantity of smaller sprites, efficiently. And theres the rub, that word, 'efficiently', 11 letters that can drive a man insane, or rather drive him to write about it??? Anyways, lets get into the introduction.

# Actual Intro
A few years ago when I ditched SFML and went a level lower into OpenGL I gullably assumed a spritebatch simply takes the data required to render each individual sprite (vertex and element) and aggregates them into a single vertex array and just glDrawArray my way into high performance graphics. But this was barely enough, in fact it *wasn't* enough, so whats the next step? Well thats why im writing all of this, I need to log all my data somewhere, because between rewriting my rendering backend every other month and forgetting the reason I started to begin with, I need to serialize some actual data.
## Possible approaches
OpenGL offers several processes to actually render your geometry, in terms of ultra high performance, approaching nearly zero driver overhead, there are techniques such as indirect drawing and instancing, or hell the mother of all gl calls, glDrawArraysInstancedBaseInstance() which essentially combines instancing, element buffering and indirect drawing into a single beast of a call, but this function is for rendering a large quantity of an array of unique meshes with per instance data _indirectly_, which is absolutely stunning (in my eyes); so thats the answer my question right? Well no, in my specific case using too overkill a technique can result in lower throughput due to driver/OpenGL overhead. So ultimately this entire process comes down to benchmarking and combining different types of buffering, instancing (or not) or plain vertex array manipulation, so let me briefly list some ways ive thought off to solve this issue:

1. Use a uniform buffer to pass in a large array of transform matrices each frame
2. Streaming the transforms into a shader storage buffer.
3. Simply baking the model transforms into the vertex buffer each frame and streaming that.
4. Maybe instead of using a vertex array to aggregate all the individual meshes I should instead use instancing to minimize memory i/o because at a certain threshold amount of sprites surely the 4 vertices and 6 indices add up to more than simply passing in a single transformation matrix and texture coordinates.
5. Using a geometry shader to generate meshes on the fly, these days they are not as slow (they absolutely are I simply mentioned this to prove I looked at all angles).
6. Maybe baking the transformation matrix into the vertex buffer at render time each frame in a compute shader and using a shader storage to save on i/o.

#### Batching
Its the first word, however we can also design our sprite batch to specifically batch together groups of sprites, this may be to divide them into a chunk system (rather use a quad tree) therefor opening the door to full blown realtime dynamic culling for further optimization, with this said, at lower sprite counts is it more or less efficient to batch render sprites depending on texture, where is the critical point between memory speed limitations, GPU texture switching limitations and the dread of my soul bare, caching.

7. Splitting sprites into batches (such as by texture to avoid texture switching, proximity for culling, etc.) and render smaller groups with finer control of rendering parameters, hopefully increasing performance at higher sprite counts.
8. breaking my strict OOP conventions and using a more functional array-of-structs paradigm to greatly improve caching performance (remember this***)

X. Finally, using a more vanilla (lame) approach and simply baking everything into the vertex buffer for all sprites and simply stich a large texture atlas with each sprites sprite sheet.

# Current Implementation
### Obligatory rant
I started targeting OpenGL 3.3 core (because of my childish obsession with MacOSX compatibility) and hence was really set back by the lack of shader storage buffers (uniform buffers have a really disappointing size limit), but since then I have been (successfully) lead to temptation by seeing modern Vulkan, and decided to just give GL4.6 a shot, and whoah. Without accidentally ranting about things like direct memory access and compute shaders, nevermind, in around 2 hours I was able to not only learn enough about compute shaders to create a instanced particle renderer using shader storage buffers, but I was able to achieve over 300FPS on my 3060 with 1024^2 particles, which is inspiring, I plan to transition as much mesh generation to the GPU as possible. Man just thinking about all these new features just juices my dopamine receptors.

### Actual implementation
Horizon's current sprite batch implementation batches sprites by texture and only updates their vertex data when a sprite is added or remove from the sprite batches scope, on a per frame basis a shader storage buffer is updated with the model transformation matrices and frame offsets (for animated sprites), and with this I am able to render 2500 cats at around 70FPS and 1000 cats at 180FPS.

![1000 cats at 150FPS](image.png)
![2500 cats at 70FPS](image-1.png)