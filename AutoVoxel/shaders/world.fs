#version 410 core

layout(location = 0) out vec4 AlbedoColor;

layout(location = 0) in vec2 texCoords;

void main() {
  AlbedoColor = vec4(vec3(texCoords.x), 1.0);
}
