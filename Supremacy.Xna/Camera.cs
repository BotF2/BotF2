using Microsoft.Xna.Framework;

namespace Supremacy.Xna
{
    public class Camera
    {
        /// <summary>
        /// A global projection matrix since it never changes
        /// </summary>
        private readonly Matrix _projection;

        /// <summary>
        /// A global view matrix since it never changes
        /// </summary>
        private Matrix _view;

        /// <summary>
        /// The Camera position which never changes
        /// </summary>
        private Vector3 _viewPosition;

        public Matrix Projection
        {
            get { return _projection; }
        }

        public Matrix View
        {
            get { return _view; }
            set { _view = value; }
        }

        public Vector3 ViewPosition
        {
            get { return _viewPosition; }
            set
            {
                _viewPosition = value;
                _view = Matrix.CreateLookAt(_viewPosition, Vector3.Zero, Vector3.Up);
            }
        }

        public Camera(Matrix projection)
        {
            _projection = projection;
        }

        public Camera(float fov, float aspectRatio, float nearPlane, float farPlane)
        {
            _projection = Matrix.CreatePerspectiveFieldOfView(fov, aspectRatio, nearPlane, farPlane);
        }
    }
}