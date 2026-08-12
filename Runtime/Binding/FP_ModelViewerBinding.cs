namespace FuzzPhyte.ModelViewer
{
    using FuzzPhyte.Placement.OrbitalCamera;
    using UnityEngine;

    [DisallowMultipleComponent]
    public sealed class FP_ModelViewerBinding : MonoBehaviour
    {
        [SerializeField] private FP_ModelViewerItemData _item;
        [SerializeField] private FP_ModelDisplayBinding _modelDisplayBinding;

        public FP_ModelViewerItemData Item => _item;
        public FP_ModelDisplayBinding ModelDisplayBinding => _modelDisplayBinding;
        public bool IsConfigured => _item != null && _modelDisplayBinding != null;

        public bool TryGetWorldBounds(out Bounds bounds)
        {
            if (_modelDisplayBinding == null)
            {
                bounds = default;
                return false;
            }

            bounds = _modelDisplayBinding.GetWorldBounds();
            return true;
        }

        private void Reset()
        {
            if (_modelDisplayBinding == null)
            {
                _modelDisplayBinding = GetComponent<FP_ModelDisplayBinding>();
            }
        }
    }
}
