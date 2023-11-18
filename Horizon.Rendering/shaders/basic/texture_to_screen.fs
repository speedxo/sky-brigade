#version 410 core

layout(location = 0) out vec4 AlbedoColor;

layout(location = 0) in vec2 texCoords;

uniform sampler2D uTexture;

void main() {
  AlbedoColor = texture(uTexture, texCoords);
}
