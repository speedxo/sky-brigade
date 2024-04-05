#version 410 core

uniform sampler2D uTexAlbedo;
out vec4 AlbedoColor;

layout(location = 0) in vec2 texCoord;

void main() {
  AlbedoColor = texture(uTexAlbedo, texCoord);
}
