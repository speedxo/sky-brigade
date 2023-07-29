using System;
using System.IO;
using SkyBrigade.Engine.Logging;
using SkyBrigade.Engine.OpenGL;

namespace SkyBrigade.Engine.Content
{
    public class ContentManager : IDisposable
    {
        private Dictionary<string, Texture> namedTextures;
        private Dictionary<string, Shader> namedShaders;

        private Dictionary<string, Texture> unnamedTextures;
        private Dictionary<string, Shader> unnamedShaders;

        public ContentManager()
        {
            namedTextures = new Dictionary<string, Texture>();
            namedShaders = new Dictionary<string, Shader>();

            unnamedShaders = new Dictionary<string, Shader>();
            unnamedTextures = new Dictionary<string, Texture>();
        }

        #region Textures

        public Texture LoadTexture(string path)
        {
            if (!unnamedTextures.TryGetValue(path, out var texture))
            {
                texture = new Texture(GameManager.Instance.Gl, path);
                unnamedTextures.Add(path, texture);
            }
            else
            {
                GameManager.Instance.Logger.Log(LogLevel.Warning, $"An attempt to load Texture({path}) was made even though an instance of Texture({path}) already exists, a reference to the already loaded texture will be returned.");
            }

            return texture;
        }

        public Texture GenerateNamedTexture(string name, Texture texture)
        {
            if (!namedTextures.ContainsKey(name))
            {
                namedTextures.Add(name, texture);
            }

            return namedTextures[name];
        }

        public Texture GenerateNamedTexture(string name, string path)
        {
            if (File.Exists(path))
                return GenerateNamedTexture(name, new Texture(GameManager.Instance.Gl, path));

            return GenerateNamedTexture(name, GetTexture("debug"));
        }
        public Texture GenerateNamedTexture(string name, Span<byte> data, uint w, uint h) => GenerateNamedTexture(name, new Texture(GameManager.Instance.Gl, data, w, h));
        public Texture GetTexture(string name) => namedTextures.TryGetValue(name, out var texture) ? texture : throw new Exception($"Key {name} not found in stored textures.");
        #endregion

        #region Shaders
        public Shader LoadShader(string vertexPath, string fragmentPath)
        {
            string uniqueKey = vertexPath + fragmentPath;

            if (!unnamedShaders.TryGetValue(uniqueKey, out var shader))
            {
                shader = new Shader(GameManager.Instance.Gl, vertexPath, fragmentPath);
                unnamedShaders.Add(uniqueKey, shader);
            }
            else
            {
                GameManager.Instance.Logger.Log(LogLevel.Fatal, $"An attempt to load Shader({vertexPath}, {fragmentPath}) was made even though an instance of Shader({vertexPath}, {fragmentPath}) already exists, a reference to the already loaded shader will be returned.");
            }

            return shader;
        }

        public Shader GenerateNamedShader(string name, Shader shader)
        {
            if (!namedShaders.ContainsKey(name))
            {
                namedShaders.Add(name, shader);
            }
            else
            {
                throw new ArgumentException("Key already in use.");
            }

            return namedShaders[name];
        }

        public Shader GenerateNamedShader(string name, string vertexPath, string fragmentPath) => GenerateNamedShader(name, new Shader(GameManager.Instance.Gl, vertexPath, fragmentPath));
        public Shader GetShader(string name) => namedShaders.TryGetValue(name, out var shader) ? shader : throw new Exception($"Key {name} not found in stored shaders.");
        #endregion

        /*  Make sure we cleanup nicely.
         *  update: yea right
         *  TODO: fix
         */
        public void Dispose()
        {
            foreach (var item in namedShaders.Values)
                item.Dispose();
            namedShaders.Clear();

            foreach (var item in namedTextures.Values)
                item.Dispose();
            namedTextures.Clear();

            foreach (var item in unnamedTextures.Values)
                item.Dispose();
            unnamedTextures.Clear();

            foreach (var item in unnamedShaders.Values)
                item.Dispose();
            unnamedShaders.Clear();
        }
    }

}

