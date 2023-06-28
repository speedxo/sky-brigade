#version 410 core

uniform sampler2D uTexture;

out vec4 FragColor;
in vec2 texCoords;

void main()
{
    FragColor = texture(uTexture, texCoords);
}