using SkyBrigade.Engine.GameEntity;
using SkyBrigade.Engine.Logging;
using SkyBrigade.Engine.OpenGL;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace SkyBrigade.Engine.Content
{
    public class ContentManager : Entity, IDisposable
    {
        private ConcurrentDictionary<string, Texture> namedTextures;
        private ConcurrentDictionary<string, Shader> namedShaders;
        private ConcurrentDictionary<string, Texture> unnamedTextures;
        private ConcurrentDictionary<string, Shader> unnamedShaders;

        public ContentManager()
        {
            namedTextures = new ConcurrentDictionary<string, Texture>();
            namedShaders = new ConcurrentDictionary<string, Shader>();
            unnamedShaders = new ConcurrentDictionary<string, Shader>();
            unnamedTextures = new ConcurrentDictionary<string, Texture>();
        }


        #region Textures

        public Texture LoadTexture(string path)
        {
            string internedPath = string.Intern(path);

            if (!unnamedTextures.TryGetValue(internedPath, out var texture))
            {
                texture = new Texture(GameManager.Instance.Gl, internedPath);
                unnamedTextures.TryAdd(internedPath, texture);
            }
            else
            {
                GameManager.Instance.Logger.Log(LogLevel.Warning, $"An attempt to load Texture({path}) was made even though an instance of Texture({path}) already exists, a reference to the already loaded texture will be returned.");
            }

            return texture;
        }

        public Texture GenerateNamedTexture(string name, Texture texture)
        {
            string internedName = string.Intern(name);

            namedTextures.TryAdd(internedName, texture);

            return texture;
        }

        public Texture GenerateNamedTexture(string name, string path)
        {
            string internedName = string.Intern(name);
            string internedPath = string.Intern(path);

            if (File.Exists(internedPath))
                return GenerateNamedTexture(internedName, new Texture(GameManager.Instance.Gl, internedPath));

            return GenerateNamedTexture(internedName, GetTexture("debug"));
        }

        public Texture GenerateNamedTexture(string name, Span<byte> data, uint w, uint h)
        {
            string internedName = string.Intern(name);
            return GenerateNamedTexture(internedName, new Texture(GameManager.Instance.Gl, data, w, h));
        }

        public Texture GetTexture(string name)
        {
            string internedName = string.Intern(name);
            return namedTextures.TryGetValue(internedName, out var texture) ? texture : throw new Exception($"Key {internedName} not found in stored textures.");
        }

        #endregion Textures

        #region Shaders
        public Shader LoadShader(string vertexPath, string fragmentPath)
        {
            string internedVertexPath = string.Intern(vertexPath);
            string internedFragmentPath = string.Intern(fragmentPath);
            string uniqueKey = internedVertexPath + internedFragmentPath;

            if (!unnamedShaders.TryGetValue(uniqueKey, out var shader))
            {
                shader = new Shader(GameManager.Instance.Gl, internedVertexPath, internedFragmentPath);
                unnamedShaders.TryAdd(uniqueKey, shader);
            }
            else
            {
                GameManager.Instance.Logger.Log(LogLevel.Fatal, $"An attempt to load Shader({internedVertexPath}, {internedFragmentPath}) was made even though an instance of Shader({internedVertexPath}, {internedFragmentPath}) already exists, a reference to the already loaded shader will be returned.");
            }

            return shader;
        }

        public Shader GenerateNamedShader(string name, Shader shader)
        {
            string internedName = string.Intern(name);

            namedShaders.TryAdd(internedName, shader);

            return shader;
        }

        public Shader GenerateNamedShader(string name, string vertexPath, string fragmentPath)
        {
            string internedName = string.Intern(name);
            string internedVertexPath = string.Intern(vertexPath);
            string internedFragmentPath = string.Intern(fragmentPath);
            return GenerateNamedShader(internedName, new Shader(GameManager.Instance.Gl, internedVertexPath, internedFragmentPath));
        }

        public Shader GetShader(string name)
        {
            string internedName = string.Intern(name);
            return namedShaders.TryGetValue(internedName, out var shader) ? shader : throw new Exception($"Key {internedName} not found in stored shaders.");
        }


        #endregion Shaders

        // Implement IDisposable pattern
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
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

                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Implement finalizer as backup for manual disposal
        ~ContentManager()
        {
            Dispose(false);
        }

        public void DeleteTexture(string name)
        {
            if (!namedTextures.TryRemove(name, out var _))
                GameManager.Instance.Logger.Log(LogLevel.Error, $"Attempt to delete nonexistent Texture({name})");
        }

        public void DeleteShader(string name)
        {
            if (!namedShaders.TryRemove(name, out var _))
                GameManager.Instance.Logger.Log(LogLevel.Error, $"Attempt to delete nonexistent Shader({name})");
        }
    }
}
