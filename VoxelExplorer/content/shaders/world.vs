#version 410 core

// vertex data
layout(location = 0) in vec3 vPosition;
layout(location = 1) in vec2 vTexCoords;

// instance data
layout(location = 2) in int iPackedData;

uniform mat4 uCameraView;
uniform mat4 uCameraProjection;

layout(location = 0) out vec2 oTexCoords;

void main() {
	// pas through data to fs
	oTexCoords = vTexCoords;

	// unpack position offset in ranges 0 - 16
	vec3 aOffset = vec3(iPackedData & 15, (iPackedData >> 4) & 15, (iPackedData >> 8) & 15);

	// transform vertices to match the correct face orientation
	vec3 position = vPosition;
	int face = (iPackedData >> 12) & 7;
	if (face == 1) {
		position.y++;
	}
	else if (face == 2) {
		position.xy = position.yx;
		position.x++;
	}
	else if (face == 3) {
		position.xy = position.yx;
	}
	else if (face == 4) {
		position.xzy = position.xyz;
		position.z++;
	}
	else if (face == 5) {
		position.xzy = position.xyz;
	}

	gl_Position = uCameraProjection * uCameraView * vec4(position + aOffset, 1.0);
} 