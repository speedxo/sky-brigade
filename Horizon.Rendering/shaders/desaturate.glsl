vec3 desaturate(vec3 color, float factor)
{
	vec3 lum = vec3(0.299, 0.587, 0.114);
	vec3 gray = vec3(dot(lum, color));
	return mix(color, gray, factor);
}

vec4 desaturate(vec4 color, float factor)
{
	return vec4(desaturate(color.rgb, factor), color.a);
}