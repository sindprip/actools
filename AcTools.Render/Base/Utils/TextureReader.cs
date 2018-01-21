﻿using System;
using System.Drawing;
using System.Runtime.CompilerServices;
using AcTools.Render.Base.TargetTextures;
using AcTools.Render.Shaders;
using AcTools.Utils;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace AcTools.Render.Base.Utils {
    public class TextureReader : IDisposable {
        private readonly Renderer _renderer;

        private class Renderer : BaseRenderer {
            protected override FeatureLevel FeatureLevel => FeatureLevel.Level_10_0;
            protected override void InitializeInner() {}
            protected override void ResizeInner() {}
            protected override void OnTickOverride(float dt) { }
        }

        public TextureReader() {
            _renderer = new Renderer();
            _renderer.Initialize();
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public byte[] ToPngNoFormat(byte[] bytes, bool ignoreAlpha = false, Size? downsize = null) {
            return ToPng(bytes, ignoreAlpha, downsize);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public byte[] ToPng(byte[] bytes, bool ignoreAlpha = false, Size? downsize = null) {
            return ToPng(_renderer.DeviceContextHolder, bytes, ignoreAlpha, downsize, out _);
        }

        public byte[] ToPng(byte[] bytes, bool ignoreAlpha, out Format format) {
            return ToPng(_renderer.DeviceContextHolder, bytes, ignoreAlpha, null, out format);
        }

        public byte[] ToPng(byte[] bytes, bool ignoreAlpha, Size? downsize, out Format format) {
            return ToPng(_renderer.DeviceContextHolder, bytes, ignoreAlpha, downsize, out format);
        }

        public static byte[] ToPng(DeviceContextHolder holder, byte[] bytes, bool ignoreAlpha = false, Size? downsize = null) {
            return ToPng(holder, bytes, ignoreAlpha, downsize, out _);
        }

        public static byte[] ToPng(DeviceContextHolder holder, byte[] bytes, bool ignoreAlpha, out Format format) {
            return ToPng(holder, bytes, ignoreAlpha, null, out format);
        }

        public static byte[] ToPng(DeviceContextHolder holder, byte[] bytes, bool ignoreAlpha, Size? downsize, out Format format) {
            Viewport[] viewports = null;

            try {
                using (var stream = new System.IO.MemoryStream())
                using (var effect = new EffectPpBasic())
                using (var output = TargetResourceTexture.Create(Format.R8G8B8A8_UNorm))
                using (var resource = ShaderResourceView.FromMemory(holder.Device, bytes)) {
                    var texture = (Texture2D)resource.Resource;
                    var loaded = texture.Description;

                    int width, height;
                    if (downsize.HasValue) {
                        var scale = Math.Min((double)downsize.Value.Width / loaded.Width, (double)downsize.Value.Height / loaded.Height);
                        width = (loaded.Width * scale).RoundToInt();
                        height = (loaded.Height * scale).RoundToInt();
                    } else {
                        width = loaded.Width;
                        height = loaded.Height;
                    }

                    effect.Initialize(holder.Device);

                    format = loaded.Format;
                    output.Resize(holder, width, height, null);

                    holder.DeviceContext.ClearRenderTargetView(output.TargetView, Color.Transparent);
                    holder.DeviceContext.OutputMerger.SetTargets(output.TargetView);

                    viewports = holder.DeviceContext.Rasterizer.GetViewports();
                    holder.DeviceContext.Rasterizer.SetViewports(new Viewport(0, 0, width, height, 0f, 1f));

                    holder.DeviceContext.OutputMerger.BlendState = null;
                    holder.QuadBuffers.Prepare(holder.DeviceContext, effect.LayoutPT);

                    effect.FxInputMap.SetResource(resource);
                    holder.PrepareQuad(effect.LayoutPT);

                    if (ignoreAlpha) {
                        effect.TechCopyNoAlpha.DrawAllPasses(holder.DeviceContext, 6);
                    } else {
                        effect.TechCopy.DrawAllPasses(holder.DeviceContext, 6);
                    }

                    Texture2D.ToStream(holder.DeviceContext, output.Texture, ImageFileFormat.Png, stream);
                    stream.Position = 0;
                    return stream.GetBuffer();
                }
            } finally {
                if (viewports != null) {
                    holder.DeviceContext.Rasterizer.SetViewports(viewports);
                }
            }
        }

        public void Dispose() {
            _renderer?.Dispose();
        }
    }
}