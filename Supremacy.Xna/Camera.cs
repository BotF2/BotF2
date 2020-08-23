using Microsoft.Xna.Framework;

namespace Supremacy.Xna
{
    public class Camera
    {

        /// <summary>
        /// A global view matrix since it never changes
        /// </summary>
        private Matrix _view;

        /// <summary>
        /// The Camera position which never changes
        /// </summary>
        private Vector3 _viewPosition;

        public Matrix Projection { get; }

        public Matrix View
        {
            get => _view;
            set => _view = value;
        }

        public Vector3 ViewPosition
        {
            get => _viewPosition;
            set
            {
                _viewPosition = value;
                _view = Matrix.CreateLookAt(_viewPosition, Vector3.Zero, Vector3.Up);
            }
        }

        public Camera(Matrix projection)
        {
            Projection = projection;
        }

        public Camera(float fov, float aspectRatio, float nearPlane, float farPlane)
        {
            Projection = Matrix.CreatePerspectiveFieldOfView(fov, aspectRatio, nearPlane, farPlane);
        }
    }
}