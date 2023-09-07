#version 410 core

out vec4 FragColor;

in vec2 texCoords;

uniform sampler2D uTexture;

void main() { FragColor = texture(uTexture, texCoords); }
