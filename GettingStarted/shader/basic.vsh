// the first line of a shader must always be the version specifier, in this case we are targeting the latest OpenGL 4.6 using the core profile (meaning no backwards compatability or deprecated methods)
#version 460 core

// next we specify the attributes, in this case since the data is tightly packed, we can simply accept a vec3. It is standard nomenclature to name attribute values starting with v.
layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec2 vTextureCoordinates;

// also specify a model matrix to transform, scale and rotate our vertices.
uniform mat4 uModelMatrix;

// specify an output to the fragment shader
layout(location = 0) out vec2 oTexCoords;

// next we define the entry point for the shader, this shader will simply set the gl_Position (a built in variable that must contain the final vertex position in any vertex shader).
void main()
{
    // it is a 4d vector
    gl_Position = uModelMatrix * vec4(vPosition, 1.0);

    // forward the UV
    oTexCoords = vTextureCoordinates;
}