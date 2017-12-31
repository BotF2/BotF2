using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using Supremacy.Universe;

namespace Supremacy.Xna
{
    public class Sun : IDisposable
    {
        private const int XCount = 1;
        private const int YCount = 1;
        private const int NumFrames = 5;
        private const float FrameTransitionTime = 2000.0f;
        private const float RotationRate = 20.0f;
        private EffectParameter _blendFactor;
        private VertexBuffer _buffer;
        private Camera _camera;
        private ContentManager _contentManager;

        private int _currentFrame;
        private double _currentTime;
        private Effect _effect;
        private IGraphicsDeviceService _graphicsService;
        private Texture2D[] _starMaps;
        private EffectParameter _sun0TextureParam;
        private EffectParameter _sun1TextureParam;
        private VertexDeclaration _vertexDecl;
        private EffectParameter _worldParam;
        private EffectParameter _worldViewProjectionParam;
        private Vector3 _rotation;
        private Vector3 _center;
        private Vector3 _scale;
        private Vector3 _position;
        private readonly StarType _starType;

        private bool _isDisposed;

        public Vector3 Position { get; set; }
        public Matrix World { get; set; }

        public Sun(StarType starType)
        {
            if (starType >= StarType.Nebula)
                throw new ArgumentOutOfRangeException("starType");

            _starType = starType;
        }

        private void ClearStarMaps()
        {
            if (_starMaps != null)
            {
                foreach (var starMap in _starMaps)
                    starMap.Dispose();

                _starMaps = null;
            }
        }

        public void Create(IGraphicsDeviceService graphicsService, ContentManager contentManager, Camera camera)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("This Sun object has been disposed.");

            _camera = camera;
            _contentManager = contentManager;
            _graphicsService = graphicsService;

            _buffer = Plane(XCount, YCount);

            _effect = contentManager.Load<Effect>(@"Resources\Effects\sun");

            _worldParam = _effect.Parameters["world"];
            _worldViewProjectionParam = _effect.Parameters["worldViewProjection"];
            _sun0TextureParam = _effect.Parameters["Sun_Tex0"];
            _sun1TextureParam = _effect.Parameters["Sun_Tex1"];
            _blendFactor = _effect.Parameters["blendFactor"];

            _vertexDecl = new VertexDeclaration(graphicsService.GraphicsDevice, VertexPositionColor.VertexElements);

            _position = new Vector3(0f, 0f, 0f);
            _center = new Vector3(0.5f, 0.5f, 0f);
            _scale = new Vector3(1f, 1f, 1f);

            //World = Matrix.CreateWorld(new Vector3(-.5f, -.5f, 0), Vector3.Forward, Vector3.Up);
            Position = Vector3.Zero;
            
            CreateStarMaps();
        }

        private void CreateStarMaps()
        {
            var starType = _starType;
            var starMaps = new Texture2D[NumFrames];

            starMaps[0] = _contentManager.Load<Texture2D>(@"Resources\Textures\" + starType + "1");
            starMaps[1] = _contentManager.Load<Texture2D>(@"Resources\Textures\" + starType + "2");
            starMaps[2] = _contentManager.Load<Texture2D>(@"Resources\Textures\" + starType + "3");
            starMaps[3] = _contentManager.Load<Texture2D>(@"Resources\Textures\" + starType + "4");
            starMaps[4] = _contentManager.Load<Texture2D>(@"Resources\Textures\" + starType + "5");

            _starMaps = starMaps;
        }

        /// <summary>
        /// Creates a rectangle with a certain number of rows and columns. The buffer is PositionOnly and always 0.0-1.0
        /// </summary>
        /// <param name="columns">Number of columns</param>
        /// <param name="rows">Number of Rows</param>
        /// <returns>Populated Vertex buffer</returns>
        public VertexBuffer Plane(int columns, int rows)
        {
            //TODO: Would be better to have a PositionOnly vertex type here but that's not in this build
            //Its used for the backdrop of evolved which is a full screen quad with texture coordinates derived from
            //other shader variables
            //TODO: Fix up shader for evolved backdrop to ignore the color in the vertex format

            var buffer = new VertexBuffer(
                _graphicsService.GraphicsDevice,
                typeof(VertexPositionColor),
                columns * rows * 6,
                BufferUsage.WriteOnly);

            //VertexPositionColor data = Buffer.Lock<PositionOnly>(0, 0, LockFlags.None);
            var data = new VertexPositionColor[columns * rows * 6];
            
            //Buffer coordinates are 0.0-1.0 so we can use them as the base texture coordinates too
            var pointCount = 0;
            for (var x = 0; x < columns; x++)
            {
                for (var y = 0; y < rows; y++)
                {
                    data[pointCount + 0] = new VertexPositionColor(new Vector3(x / (float)columns, y / (float)rows, 0), Color.White);
                    data[pointCount + 1] = new VertexPositionColor(new Vector3((x + 1) / (float)columns, y / (float)rows, 0), Color.White);
                    data[pointCount + 2] = new VertexPositionColor(new Vector3((x + 1) / (float)columns, (y + 1) / (float)rows, 0), Color.White);
                    data[pointCount + 3] = new VertexPositionColor(new Vector3(x / (float)columns, y / (float)rows, 0), Color.White); //same as 0
                    data[pointCount + 4] = new VertexPositionColor(new Vector3((x + 1) / (float)columns, (y + 1) / (float)rows, 0), Color.White); //same as 2
                    data[pointCount + 5] = new VertexPositionColor(new Vector3(x / (float)columns, (y + 1) / (float)rows, 0), Color.White);

                    pointCount += 6;
                }
            }

            buffer.SetData(data);

            return buffer;
        }

        public void Update(TimeSpan elapsedTime)
        {
            // change frames every second.
            _currentTime += elapsedTime.TotalMilliseconds;

            if (_currentTime > FrameTransitionTime)
            {
                _currentTime = 0.0f;
                _currentFrame++;
            }

            if (_currentFrame > 4)
                _currentFrame = 0;

            _rotation.Z += (float)(elapsedTime.TotalSeconds / RotationRate);

            World = Matrix.CreateTranslation(-_center) *
                         Matrix.CreateScale(_scale) *
                         Matrix.CreateRotationX(_rotation.X) *
                         Matrix.CreateRotationY(_rotation.Y) *
                         Matrix.CreateRotationZ(_rotation.Z) *
                         Matrix.CreateTranslation(_position);
        }

        public void Render()
        {
            if (_isDisposed || _starMaps == null)
                return;

            var device = _graphicsService.GraphicsDevice;

            device.VertexDeclaration = _vertexDecl;
            device.Vertices[0].SetSource(_buffer, 0, VertexPositionColor.SizeInBytes);
            
            _worldParam.SetValue(World);
            _worldViewProjectionParam.SetValue(World * _camera.View * _camera.Projection);

            var currentFactor = (float)(_currentTime / FrameTransitionTime);

            _blendFactor.SetValue(currentFactor);

            var sun = _starMaps;

            _sun1TextureParam.SetValue(sun[_currentFrame]);

            if (_currentFrame < 4)
                _sun0TextureParam.SetValue(sun[_currentFrame + 1]);
            else
                _sun0TextureParam.SetValue(sun[0]);

            _effect.Begin();
            _effect.Techniques[0].Passes[0].Begin();
            _effect.CommitChanges();

            device.DrawPrimitives(PrimitiveType.TriangleList, 0, XCount * YCount * 2);

            _effect.Techniques[0].Passes[0].End();
            _effect.End();
        }

/*
        private void RenderReference(StarType starType, GraphicsDevice device)
        {
            var sun = _starMaps[starType];

            var spriteBatch = new SpriteBatch(device);
            var currentFactor = (float)(_currentTime / FrameTransitionTime);

            var texture1 = sun[_currentFrame];
            var texture2 = (_currentFrame < 4) ? sun[_currentFrame + 1] : sun[0];

            var scale = (float)device.Viewport.Width / texture1.Width;
            var sourceRectangle = new Rectangle(0, 0, texture1.Width, texture1.Height);
            var destinationRectangle = new Rectangle(0, 0, device.Viewport.Width, device.Viewport.Height);
            var rotationOrigin = new Vector2(sourceRectangle.Width / 2f, sourceRectangle.Height / 2f);
            var position = new Vector2(destinationRectangle.Width / 2f, destinationRectangle.Height / 2f);

            spriteBatch.Begin(SpriteBlendMode.Additive);
            spriteBatch.Draw(sun[_currentFrame], position, null, Color.White, _rotation.Z, rotationOrigin, scale, SpriteEffects.None, 0);
            spriteBatch.Draw(texture2, position, null, new Color(Color.White, currentFactor), _rotation.Z, rotationOrigin, scale, SpriteEffects.None, 0);
            spriteBatch.End();
        }
*/

        #region Implementation of IDisposable
        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;

            if (_effect != null)
            {
                _effect.Dispose();
                _effect = null;
            }

            if (_buffer != null)
            {
                _buffer.Dispose();
                _buffer = null;
            }

            if (_vertexDecl != null)
            {
                _vertexDecl.Dispose();
                _vertexDecl = null;
            }

            ClearStarMaps();
        }
        #endregion
    }
}