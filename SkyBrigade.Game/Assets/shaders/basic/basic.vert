#version 410 core

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vNorm;
layout (location = 2) in vec2 vTexCoords;

uniform mat4 uView;
uniform mat4 uProjection;

out vec2 texCoords;

void main()
{
    texCoords = vTexCoords;

    //Multiplying our uniform with the vertex position, the multiplication order here does matter.
    gl_Position = uProjection * uView * vec4(vPos, 1.0);
} 