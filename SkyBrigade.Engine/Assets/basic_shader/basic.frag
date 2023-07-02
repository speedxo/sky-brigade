#version 410 core

uniform sampler2D uTexture;
uniform vec4 uColor;

out vec4 FragColor;
in vec2 texCoords;

uniform bool useTexture;

void main()
{
    FragColor = uColor;

    if (useTexture) 
        FragColor = texture(uTexture, texCoords) * uColor;
}