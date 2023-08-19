#version 410 core

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vNorm;
layout (location = 2) in vec2 vTexCoords;

uniform mat4 uView;
uniform mat4 uProjection;
uniform mat4 uModel;

out vec2 texCoords;
out vec3 normals;
out vec3 vertexPos;

void main()
{
    texCoords = vTexCoords;
    normals = vNorm;
    vertexPos = (vec4(vPos, 1.0f) * uModel).xyz;

    //Multiplying our uniform with the vertex position, the multiplication order here does matter.
    gl_Position = vec4(vPos, 1.0);
} 