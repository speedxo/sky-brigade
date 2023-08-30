#version 410 core

uniform sampler2D uTexture;
uniform vec4 uColor;

out vec4 FragColor;
in vec2 texCoords;
in vec3 norm;

uniform bool useTexture;
uniform bool useNormalAsColor;

void main()
{
    vec4 oColor = vec4(1.0f);

    if (useTexture) 
        oColor = texture(uTexture, texCoords) * uColor;
    if (uColor.w > 0.0)
        oColor = uColor;
    if (useNormalAsColor)
        oColor = vec4(norm, 0.8f);

    FragColor = oColor;
}