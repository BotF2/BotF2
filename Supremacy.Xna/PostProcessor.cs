using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using System.Linq;

namespace Supremacy.Xna
{
    /// <summary>
    /// Handles rendering of various post-processing techniques,
    /// including bloom and tone mapping
    /// </summary>
    public class PostProcessor : IDisposable
    {
        private const float ToneMapKey = 3f;
        private const float MaxLuminance = 512.0f;
        private const float BloomThreshold = 0.4f;
        private const float BloomMultiplier = 1f;
        private const float BlurSigma = 2.5f;
        private readonly Effect _blurEffect;
        private readonly bool _canFilterFp16;
        private readonly RenderTarget2D _currentFrameLuminance;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly Effect _hdrEffect;
        private readonly List<IntermediateTexture> _intermediateTextures = new List<IntermediateTexture>();
        private readonly RenderTarget2D[] _luminanceChain;

        private readonly VertexDeclaration _quadDecl;
        private readonly VertexBuffer _quadVb;

        private readonly Effect _scalingEffect;
        private readonly Effect _thresholdEffect;
        private RenderTarget2D _currentFrameAdaptedLuminance;
        private RenderTarget2D _lastFrameAdaptedLuminance;

        /// <summary>
        /// The class constructor
        /// </summary>
        /// <param name="graphicsDevice">The GraphicsDevice to use for rendering</param>
        /// <param name="contentManager">The ContentManager from which to load Effects</param>
        public PostProcessor(GraphicsDevice graphicsDevice, ContentManager contentManager)
        {
            _graphicsDevice = graphicsDevice;

            // Load the effects
            _blurEffect = contentManager.Load<Effect>(@"Resources\Effects\pp_Blur");
            _thresholdEffect = contentManager.Load<Effect>(@"Resources\Effects\pp_Threshold");
            _scalingEffect = contentManager.Load<Effect>(@"Resources\Effects\pp_Scale");
            _hdrEffect = contentManager.Load<Effect>(@"Resources\Effects\pp_HDR");

            // Initialize our buffers
            float width = (float)graphicsDevice.PresentationParameters.BackBufferWidth;
            float height = (float)graphicsDevice.PresentationParameters.BackBufferHeight;

            // Two buffers we'll swap between, so we can adapt the luminance            
            _currentFrameLuminance = new RenderTarget2D(
                graphicsDevice, 1, 1, 1, SurfaceFormat.Single, RenderTargetUsage.DiscardContents);
            _currentFrameAdaptedLuminance = new RenderTarget2D(
                graphicsDevice, 1, 1, 1, SurfaceFormat.Single, RenderTargetUsage.DiscardContents);
            _lastFrameAdaptedLuminance = new RenderTarget2D(
                graphicsDevice, 1, 1, 1, SurfaceFormat.Single, RenderTargetUsage.DiscardContents);

            DepthStencilBuffer depthStencil = graphicsDevice.DepthStencilBuffer;
            graphicsDevice.DepthStencilBuffer = null;

            graphicsDevice.SetRenderTarget(0, _lastFrameAdaptedLuminance);
            graphicsDevice.Clear(Color.White);
            graphicsDevice.SetRenderTarget(0, null);

            graphicsDevice.DepthStencilBuffer = depthStencil;

            // We need a luminance chain
            int chainLength = 1;
            int startSize = (int)MathHelper.Min(width / 16, height / 16);
            
            int size;
            
            for (size = 16; size < startSize; size *= 4)
            {
                chainLength++;
            }

            _luminanceChain = new RenderTarget2D[chainLength];

            size /= 4;

            for (int i = 0; i < chainLength; i++)
            {
                _luminanceChain[i] = new RenderTarget2D(graphicsDevice, size, size, 1, SurfaceFormat.Single);
                size /= 4;
            }

            // Make our full-screen quad
            _quadDecl = CreateQuadVertexDeclaration();
            _quadVb = CreateFullScreenQuad(_quadDecl);

            // Check to see if we can filter fp16
            _canFilterFp16 = GraphicsAdapter.DefaultAdapter.CheckDeviceFormat(
                DeviceType.Hardware,
                SurfaceFormat.Color,
                TextureUsage.None,
                QueryUsages.Filter,
                ResourceType.Texture2D,
                SurfaceFormat.HalfVector4);
        }

        /// <summary>
        /// Applies a blur to the specified render target, writes the result
        /// to the specified render target.
        /// </summary>
        /// <param name="source">The render target to use as the source and result</param>
        /// <param name="result">The render target to use as the result</param>
        /// <param name="sigma">The standard deviation used for gaussian weights</param>
        /// <param name="encoded">If true, blurs using LogLuv encoding/decoding</param>
        public void Blur(
            RenderTarget2D source,
            RenderTarget2D result,
            float sigma,
            bool encoded)
        {
            IntermediateTexture blurH = GetIntermediateTexture(
                source.Width,
                source.Height,
                source.Format,
                source.MultiSampleType,
                source.MultiSampleQuality);

            string baseTechniqueName = "GaussianBlur";

            if (encoded)
            {
                baseTechniqueName += "Encode";
            }

            // Do horizontal pass first
            _blurEffect.CurrentTechnique = _blurEffect.Techniques[baseTechniqueName + "H"];
            _blurEffect.Parameters["g_fSigma"].SetValue(sigma);

            PostProcess(source, blurH.RenderTarget, _blurEffect);

            // Now the vertical pass 
            _blurEffect.CurrentTechnique = _blurEffect.Techniques[baseTechniqueName + "V"];

            PostProcess(blurH.RenderTarget, result, _blurEffect);

            blurH.InUse = false;
        }

        /// <summary>
        /// Downscales the source to 1/16th size, using software(shader) filtering
        /// </summary>
        /// <param name="source">The source to be downscaled</param>
        /// <param name="result">The RT in which to store the result</param>
        /// <param name="encoded">If true, the source is encoded in LogLuv format</param>
        protected void GenerateDownscaleTargetSW(RenderTarget2D source, RenderTarget2D result, bool encoded)
        {
            string techniqueName = "Downscale4";
            if (encoded)
            {
                techniqueName += "Encode";
            }

            IntermediateTexture downscale1 = GetIntermediateTexture(source.Width / 4, source.Height / 4, source.Format);
            _scalingEffect.CurrentTechnique = _scalingEffect.Techniques[techniqueName];
            PostProcess(source, downscale1.RenderTarget, _scalingEffect);

            _scalingEffect.CurrentTechnique = _scalingEffect.Techniques[techniqueName];
            PostProcess(downscale1.RenderTarget, result, _scalingEffect);
            downscale1.InUse = false;
        }

        /// <summary>
        /// Downscales the source to 1/16th size, using hardware filtering
        /// </summary>
        /// <param name="source">The source to be downscaled</param>
        /// <param name="result">The RT in which to store the result</param>
        protected void GenerateDownscaleTargetHW(RenderTarget2D source, RenderTarget2D result)
        {
            IntermediateTexture downscale1 = GetIntermediateTexture(source.Width / 2, source.Height / 2, source.Format);
            _scalingEffect.CurrentTechnique = _scalingEffect.Techniques["ScaleHW"];
            PostProcess(source, downscale1.RenderTarget, _scalingEffect);

            IntermediateTexture downscale2 = GetIntermediateTexture(source.Width / 2, source.Height / 2, source.Format);
            _scalingEffect.CurrentTechnique = _scalingEffect.Techniques["ScaleHW"];
            PostProcess(downscale1.RenderTarget, downscale2.RenderTarget, _scalingEffect);
            downscale1.InUse = false;

            IntermediateTexture downscale3 = GetIntermediateTexture(source.Width / 2, source.Height / 2, source.Format);
            _scalingEffect.CurrentTechnique = _scalingEffect.Techniques["ScaleHW"];
            PostProcess(downscale2.RenderTarget, downscale3.RenderTarget, _scalingEffect);
            downscale2.InUse = false;

            _scalingEffect.CurrentTechnique = _scalingEffect.Techniques["ScaleHW"];
            PostProcess(downscale3.RenderTarget, result, _scalingEffect);
            downscale3.InUse = false;
        }

        /// <summary>
        /// Calculates the average luminance of the scene
        /// </summary>
        /// <param name="downscaleBuffer">The scene texure, downscaled to 1/16th size</param>
        /// <param name="dt">The time delta</param>
        /// <param name="encoded">If true, the image is encoded in LogLuv format</param>
        protected void CalculateAverageLuminance(RenderTarget2D downscaleBuffer, float dt, bool encoded)
        {
            // Calculate the initial luminance
            _hdrEffect.CurrentTechnique = encoded ? _hdrEffect.Techniques["LuminanceEncode"] : _hdrEffect.Techniques["Luminance"];
            PostProcess(downscaleBuffer, _luminanceChain[0], _hdrEffect);

            // Repeatedly downscale            
            _scalingEffect.CurrentTechnique = _scalingEffect.Techniques["Downscale4"];
            for (int i = 1; i < _luminanceChain.Length; i++)
            {
                PostProcess(_luminanceChain[i - 1], _luminanceChain[i], _scalingEffect);
            }

            // Final downscale            
            _scalingEffect.CurrentTechnique = _scalingEffect.Techniques["Downscale4Luminance"];
            PostProcess(_luminanceChain[_luminanceChain.Length - 1], _currentFrameLuminance, _scalingEffect);

            // Adapt the luminance, to simulate slowly adjust exposure
            _hdrEffect.Parameters["g_fDT"].SetValue(dt);
            _hdrEffect.CurrentTechnique = _hdrEffect.Techniques["CalcAdaptedLuminance"];
            RenderTarget2D[] sources = new RenderTarget2D[2];
            sources[0] = _currentFrameLuminance;
            sources[1] = _lastFrameAdaptedLuminance;
            PostProcess(sources, _currentFrameAdaptedLuminance, _hdrEffect);
        }

        /// <summary>
        /// Performs tone mapping on the specified render target
        /// </summary>
        /// <param name="source">The source render target</param>
        /// <param name="result">The render target to which the result will be output</param>
        /// <param name="dt">The time elapsed since the last frame</param>
        /// <param name="encoded">If true, use LogLuv encoding</param>
        /// <param name="preferHWScaling">If true, will attempt to use hardware filtering</param>
        public void ToneMap(RenderTarget2D source, RenderTarget2D result, float dt, bool encoded, bool preferHWScaling)
        {
            // Downscale to 1/16 size
            IntermediateTexture downscaleTarget = GetIntermediateTexture(
                source.Width / 16, source.Height / 16, source.Format);
            if (preferHWScaling && (encoded || _canFilterFp16))
            {
                GenerateDownscaleTargetHW(source, downscaleTarget.RenderTarget);
            }
            else
            {
                GenerateDownscaleTargetSW(source, downscaleTarget.RenderTarget, encoded);
            }

            // Get the luminance
            CalculateAverageLuminance(downscaleTarget.RenderTarget, dt, encoded);

            // Do the bloom first
            IntermediateTexture threshold = GetIntermediateTexture(
                downscaleTarget.RenderTarget.Width, downscaleTarget.RenderTarget.Height, source.Format);
            _thresholdEffect.Parameters["g_fThreshold"].SetValue(BloomThreshold);
            _thresholdEffect.Parameters["g_fMiddleGrey"].SetValue(ToneMapKey);
            _thresholdEffect.Parameters["g_fMaxLuminance"].SetValue(MaxLuminance);
            _thresholdEffect.CurrentTechnique = encoded ? _thresholdEffect.Techniques["ThresholdEncode"] : _thresholdEffect.Techniques["Threshold"];
            RenderTarget2D[] sources2 = new RenderTarget2D[2];
            sources2[0] = downscaleTarget.RenderTarget;
            sources2[1] = _currentFrameAdaptedLuminance;
            PostProcess(sources2, threshold.RenderTarget, _thresholdEffect);

            IntermediateTexture postBlur = GetIntermediateTexture(
                downscaleTarget.RenderTarget.Width, downscaleTarget.RenderTarget.Height, SurfaceFormat.Color);
            Blur(threshold.RenderTarget, postBlur.RenderTarget, BlurSigma, encoded);
            threshold.InUse = false;

            // Scale it back to half of full size (will do the final scaling step when sampling
            // the bloom texture during tone mapping).
            IntermediateTexture upscale1 = GetIntermediateTexture(
                source.Width / 8, source.Height / 8, SurfaceFormat.Color);
            _scalingEffect.CurrentTechnique = _scalingEffect.Techniques["ScaleHW"];
            PostProcess(postBlur.RenderTarget, upscale1.RenderTarget, _scalingEffect);
            postBlur.InUse = false;

            IntermediateTexture upscale2 = GetIntermediateTexture(
                source.Width / 4, source.Height / 4, SurfaceFormat.Color);
            PostProcess(upscale1.RenderTarget, upscale2.RenderTarget, _scalingEffect);
            upscale1.InUse = false;

            IntermediateTexture bloom = GetIntermediateTexture(source.Width / 2, source.Height / 2, SurfaceFormat.Color);
            PostProcess(upscale2.RenderTarget, bloom.RenderTarget, _scalingEffect);
            upscale2.InUse = false;

            // Now do tone mapping on the main source image, and add in the bloom
            _hdrEffect.Parameters["g_fMiddleGrey"].SetValue(ToneMapKey);
            _hdrEffect.Parameters["g_fMaxLuminance"].SetValue(MaxLuminance);
            _hdrEffect.Parameters["g_fBloomMultiplier"].SetValue(BloomMultiplier);
            RenderTarget2D[] sources3 = new RenderTarget2D[3];
            sources3[0] = source;
            sources3[1] = _currentFrameAdaptedLuminance;
            sources3[2] = bloom.RenderTarget;
            _hdrEffect.CurrentTechnique = encoded ? _hdrEffect.Techniques["ToneMapEncode"] : _hdrEffect.Techniques["ToneMap"];
            PostProcess(sources3, result, _hdrEffect);

            // Flip the luminance textures
            Swap(ref _currentFrameAdaptedLuminance, ref _lastFrameAdaptedLuminance);

            bloom.InUse = false;
            downscaleTarget.InUse = false;
        }

        /// <summary>
        /// Disposes all intermediate textures in the cache
        /// </summary>
        public void FlushCache()
        {
            foreach (IntermediateTexture intermediateTexture in _intermediateTextures)
            {
                intermediateTexture.RenderTarget.Dispose();
            }

            _intermediateTextures.Clear();
        }

        /// <summary>
        /// Performs a post-processing step using a single source texture
        /// </summary>
        /// <param name="source">The source texture</param>
        /// <param name="result">The output render target</param>
        /// <param name="effect">The effect to use</param>
        protected void PostProcess(RenderTarget2D source, RenderTarget2D result, Effect effect)
        {
            RenderTarget2D[] sources = new RenderTarget2D[1];
            sources[0] = source;
            PostProcess(sources, result, effect);
        }

        /// <summary>
        /// Performs a post-processing step using multiple source textures
        /// </summary>
        /// <param name="sources">The source textures</param>
        /// <param name="result">The output render target</param>
        /// <param name="effect">The effect to use</param>
        protected void PostProcess(RenderTarget2D[] sources, RenderTarget2D result, Effect effect)
        {
            _graphicsDevice.SetRenderTarget(0, result);
            _graphicsDevice.Clear(Color.Black);

            for (int i = 0; i < sources.Length; i++)
            {
                effect.Parameters["SourceTexture" + i].SetValue(sources[i].GetTexture());
            }

            effect.Parameters["g_vSourceDimensions"].SetValue(new Vector2(sources[0].Width, sources[0].Height));

            if (result == null)
            {
                effect.Parameters["g_vDestinationDimensions"].SetValue(
                    new Vector2(
                        _graphicsDevice.PresentationParameters.BackBufferWidth,
                        _graphicsDevice.PresentationParameters.BackBufferHeight));
            }
            else
            {
                effect.Parameters["g_vDestinationDimensions"].SetValue(new Vector2(result.Width, result.Height));
            }

            _graphicsDevice.VertexDeclaration = _quadDecl;
            _graphicsDevice.Vertices[0].SetSource(_quadVb, 0, _quadDecl.GetVertexStrideSize(0));

            // Begin effect
            effect.Begin(SaveStateMode.None);
            effect.CurrentTechnique.Passes[0].Begin();

            // Draw primitives
            _graphicsDevice.DrawPrimitives(PrimitiveType.TriangleStrip, 0, 2);

            // We're done
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }

        /// <summary>
        /// Checks the cache to see if a suitable rendertarget has already been created
        /// and isn't in use.  Otherwise, creates one according to the parameters
        /// </summary>
        /// <param name="width">Width of the RT</param>
        /// <param name="height">Height of the RT</param>
        /// <param name="format">Format of the RT</param>
        /// <returns>The suitable RT</returns>
        protected IntermediateTexture GetIntermediateTexture(int width, int height, SurfaceFormat format)
        {
            return GetIntermediateTexture(width, height, format, MultiSampleType.None, 0);
        }

        protected IntermediateTexture GetIntermediateTexture(
            int width,
            int height,
            SurfaceFormat format,
            MultiSampleType msType,
            int msQuality)
        {
            width = Math.Max(1, width);
            height = Math.Max(1, height);

            // Look for a matching rendertarget in the cache

            IntermediateTexture match = _intermediateTextures.FirstOrDefault(
                t => !t.InUse &&
                     height == t.RenderTarget.Height &&
                     format == t.RenderTarget.Format &&
                     width == t.RenderTarget.Width &&
                     msType == t.RenderTarget.MultiSampleType &&
                     msQuality == t.RenderTarget.MultiSampleQuality);

            if (match != null)
            {
                match.InUse = true;
                return match;
            }

            // We didn't find one, let's make one
            IntermediateTexture newTexture = new IntermediateTexture
                             {
                                 RenderTarget = new RenderTarget2D(
                                     _graphicsDevice,
                                     width,
                                     height,
                                     1,
                                     format,
                                     msType,
                                     msQuality,
                                     RenderTargetUsage.DiscardContents)
                             
                             };

            _intermediateTextures.Add(newTexture);

            newTexture.InUse = true;

            return newTexture;
        }

        /// <summary>
        /// Creates a VertexDeclaration suitable for a full-screen quad
        /// </summary>
        /// <returns></returns>
        protected VertexDeclaration CreateQuadVertexDeclaration()
        {
            VertexElement[] declElements = new VertexElement[2];
            declElements[0].Offset = 0;
            declElements[0].Stream = 0;
            declElements[0].UsageIndex = 0;
            declElements[0].VertexElementFormat = VertexElementFormat.Vector3;
            declElements[0].VertexElementMethod = VertexElementMethod.Default;
            declElements[0].VertexElementUsage = VertexElementUsage.Position;
            declElements[1].Offset = 12;
            declElements[1].Stream = 0;
            declElements[1].UsageIndex = 0;
            declElements[1].VertexElementFormat = VertexElementFormat.Vector2;
            declElements[1].VertexElementMethod = VertexElementMethod.Default;
            declElements[1].VertexElementUsage = VertexElementUsage.TextureCoordinate;
            return new VertexDeclaration(_graphicsDevice, declElements);
        }

        /// <summary>
        /// Creates a full-screen Quad VB
        /// </summary>
        /// <param name="vertexDeclaration">The VertexDeclaration to use</param>
        /// <returns>The VB for the quad</returns>
        protected VertexBuffer CreateFullScreenQuad(VertexDeclaration vertexDeclaration)
        {
            // Create a vertex buffer for the quad, and fill it in
            VertexBuffer vertexBuffer = new VertexBuffer(
                _graphicsDevice, typeof(QuadVertex), vertexDeclaration.GetVertexStrideSize(0) * 4, BufferUsage.None);
            QuadVertex[] vbData = new QuadVertex[4];

            // Upper right
            vbData[0].Position = new Vector3(1, 1, 1);
            vbData[0].TexCoord = new Vector2(1, 0);

            // Lower right
            vbData[1].Position = new Vector3(1, -1, 1);
            vbData[1].TexCoord = new Vector2(1, 1);

            // Upper left
            vbData[2].Position = new Vector3(-1, 1, 1);
            vbData[2].TexCoord = new Vector2(0, 0);

            // Lower left
            vbData[3].Position = new Vector3(-1, -1, 1);
            vbData[3].TexCoord = new Vector2(0, 1);

            vertexBuffer.SetData(vbData);
            return vertexBuffer;
        }

        /// <summary>
        /// Swaps two RenderTarget's
        /// </summary>
        /// <param name="rt1">The first RT</param>
        /// <param name="rt2">The second RT</param>
        protected void Swap(ref RenderTarget2D rt1, ref RenderTarget2D rt2)
        {
            RenderTarget2D temp = rt1;
            rt1 = rt2;
            rt2 = temp;
        }

        #region Nested type: IntermediateTexture
        /// <summary>
        /// Used for textures that store intermediate results of
        /// passes during post-processing
        /// </summary>
        public class IntermediateTexture
        {
            public bool InUse;
            public RenderTarget2D RenderTarget;
        }
        #endregion

        #region Nested type: QuadVertex
        /// <summary>
        /// Vertex for full-screen quad, used for post-processing
        /// </summary>
        public struct QuadVertex
        {
            public Vector3 Position;
            public Vector2 TexCoord;
        }
        #endregion

        #region Implementation of IDisposable

        public void Dispose()
        {
            if (_blurEffect != null && !_blurEffect.IsDisposed)
            {
                _blurEffect.Dispose();
            }

            if (_currentFrameLuminance != null && !_currentFrameLuminance.IsDisposed)
            {
                _currentFrameLuminance.Dispose();
            }

            if (_hdrEffect != null && !_hdrEffect.IsDisposed)
            {
                _hdrEffect.Dispose();
            }

            while (_intermediateTextures.Count != 0)
            {
                IntermediateTexture intermediateTexture = _intermediateTextures[0];

                if (intermediateTexture.RenderTarget != null && !intermediateTexture.RenderTarget.IsDisposed)
                {
                    intermediateTexture.RenderTarget.Dispose();
                }

                _intermediateTextures.RemoveAt(0);
            }

            foreach (RenderTarget2D renderTarget in _luminanceChain)
            {
                if (renderTarget != null && !renderTarget.IsDisposed)
                {
                    renderTarget.Dispose();
                }
            }

            if (_quadVb != null && !_quadVb.IsDisposed)
            {
                _quadVb.Dispose();
            }

            if (_scalingEffect != null && !_scalingEffect.IsDisposed)
            {
                _scalingEffect.Dispose();
            }

            if (_thresholdEffect != null && !_thresholdEffect.IsDisposed)
            {
                _thresholdEffect.Dispose();
            }

            if (_currentFrameAdaptedLuminance != null && !_currentFrameAdaptedLuminance.IsDisposed)
            {
                _currentFrameAdaptedLuminance.Dispose();
            }

            if (_lastFrameAdaptedLuminance != null && !_lastFrameAdaptedLuminance.IsDisposed)
            {
                _lastFrameAdaptedLuminance.Dispose();
            }
        }

        #endregion
    }
}