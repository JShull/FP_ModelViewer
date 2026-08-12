namespace FuzzPhyte.ModelViewer
{
    using System;
    using FuzzPhyte.Placement.OrbitalCamera;
    using UnityEngine;

    [Serializable]
    public sealed class FP_ModelThumbnailReference
    {
        [SerializeField] private FP_ViewCubeHit _view;
        [SerializeField] private Texture2D _texture;
        [SerializeField] private string _caption;

        public FP_ViewCubeHit View => _view;
        public Texture2D Texture => _texture;
        public string Caption => _caption;

        public FP_ModelThumbnailReference(
            FP_ViewCubeHit view,
            Texture2D texture,
            string caption = "")
        {
            _view = view;
            _texture = texture;
            _caption = caption;
        }

        public void Update(Texture2D texture, string caption = "")
        {
            _texture = texture;
            _caption = caption;
        }
    }
}
