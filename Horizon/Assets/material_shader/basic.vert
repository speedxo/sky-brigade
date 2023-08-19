    #version 410 core

layout (location = 0) in vec3 vPos;
layout (location = 1) in vec3 vNorm;
layout (location = 2) in vec2 vTexCoords;

uniform mat4 uView;
uniform mat4 uProjection;
uniform mat4 uModel;

out vec3 fNorm;
out vec2 fTexCoords;
out vec3 fragPos;

void main()
{
    fNorm = vNorm;
    fTexCoords = vTexCoords;
    fragPos = vec3(uModel * vec4(vPos, 1.0));

    //Multiplying our uniform with the vertex position, the multiplication order here does matter.
    gl_Position = uProjection * uView * uModel * vec4(vPos, 1.0);
} 