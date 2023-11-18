#version 410 core

layout(location = 0) in vec2 texCoords;

uniform sampler2D uTexAlbedo;

out vec4 FragColor;

void main() {
    FragColor = texture(uTexAlbedo, texCoords);
}
