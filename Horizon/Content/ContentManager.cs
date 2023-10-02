﻿using System.IO;
using Horizon.GameEntity;
using Horizon.Logging;
using Horizon.OpenGL;

namespace Horizon.Content
{
    public class ContentManager : Entity, IDisposable
    {
        private Dictionary<string, Texture> namedTextures;
        private Dictionary<string, Texture> unnamedTextures;
        private List<Texture> unmanagedTextures;

        public ShaderContentManager Shaders { get; private set; }

        public void AddShader(in string name, in Shader shader)
            => Shaders.AddNamed(name, shader);

        public override void Initialize()
        {
            Shaders = AddEntity<ShaderContentManager>();

            namedTextures = new Dictionary<string, Texture>();
            unnamedTextures = new Dictionary<string, Texture>();
            unmanagedTextures = new List<Texture>();
        }

        public int TotalTextures
        {
            get => namedTextures.Count + unnamedTextures.Count + unmanagedTextures.Count;
        }

        public IEnumerable<Texture> GetTextures()
        {
            foreach (var texture in namedTextures.Values)
            {
                yield return texture;
            }

            foreach (var texture in unnamedTextures.Values)
            {
                yield return texture;
            }

            foreach (var texture in unmanagedTextures)
            {
                yield return texture;
            }
        }

     
        #region Textures

        public Texture LoadTexture(string path)
        {
            string internedPath = string.Intern(path);

            if (!unnamedTextures.ContainsKey(internedPath))
                unnamedTextures.TryAdd(internedPath, new Texture(internedPath));
            else
                Engine.Logger.Log(
                    LogLevel.Warning,
                    $"An attempt to load Texture({path}) was made even though an instance of Texture({path}) already exists, a reference to the already loaded texture will be returned."
                );

            return unnamedTextures[internedPath];
        }

        public Texture GenerateNamedTexture(string name, Texture texture)
        {
            string internedName = string.Intern(name);

            namedTextures.TryAdd(internedName, texture);
            texture.Name = name;

            return texture;
        }

        public Texture GenerateNamedTexture(string name, string path)
        {
            string internedName = string.Intern(name);
            string internedPath = string.Intern(path);

            if (File.Exists(internedPath))
                return GenerateNamedTexture(internedName, new Texture(internedPath));

            return GenerateNamedTexture(internedName, GetTexture("debug"));
        }

        public Texture GenerateNamedTexture(string name, Span<byte> data, uint w, uint h)
        {
            string internedName = string.Intern(name);
            return GenerateNamedTexture(internedName, new Texture(data, w, h));
        }

        public Texture GetTexture(string name)
        {
            string internedName = string.Intern(name);
            return namedTextures.TryGetValue(internedName, out var texture)
                ? texture
                : throw new Exception($"Key {internedName} not found in stored textures.");
        }

        #endregion Textures


        // Implement IDisposable pattern
        private bool disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    foreach (var item in namedTextures.Values)
                        item.Dispose();
                    namedTextures.Clear();

                    foreach (var item in unnamedTextures.Values)
                        item.Dispose();
                    unnamedTextures.Clear();

                    foreach (var item in unmanagedTextures)
                        item.Dispose();
                    unmanagedTextures.Clear();
                }

                disposed = true;
            }
        }

        public void Dispose()
        {
            Shaders.Dispose();

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
            if (!namedTextures.Remove(name, out var texture))
                Engine.Logger.Log(
                    LogLevel.Error,
                    $"Attempt to delete nonexistent Texture({name})"
                );
            else
                texture?.Dispose();
        }

        public Texture AddUnmanagedTexture(uint texture)
        {
            unmanagedTextures.Add(new Texture(texture));

            return unmanagedTextures.Last();
        }

        public Texture AddTexture(Texture texture)
        {
            unnamedTextures.TryAdd(texture.Handle.ToString(), texture);
            return unnamedTextures[texture.Handle.ToString()];
        }
    }
}