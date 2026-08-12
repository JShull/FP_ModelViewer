namespace FuzzPhyte.ModelViewer
{
    using System.Collections.Generic;
    using FuzzPhyte.Utility;
    using UnityEngine;

    [CreateAssetMenu(
        fileName = "FP_ModelViewerCatalogData",
        menuName = "FuzzPhyte/Model Viewer/Catalog Data")]
    public sealed class FP_ModelViewerCatalogData : FP_Data
    {
        [SerializeField] private string _displayName;
        [SerializeField, TextArea] private string _description;
        [SerializeField] private List<FP_ModelViewerItemData> _items =
            new List<FP_ModelViewerItemData>();

        public string DisplayName => string.IsNullOrWhiteSpace(_displayName) ? name : _displayName;
        public string Description => _description;
        public IReadOnlyList<FP_ModelViewerItemData> Items => _items;
        public int Count => _items.Count;

        public bool TryGetItem(int index, out FP_ModelViewerItemData item)
        {
            if (index >= 0 && index < _items.Count)
            {
                item = _items[index];
                return item != null;
            }

            item = null;
            return false;
        }

        public bool TryGetItem(string uniqueId, out FP_ModelViewerItemData item)
        {
            if (!string.IsNullOrWhiteSpace(uniqueId))
            {
                for (int i = 0; i < _items.Count; i++)
                {
                    FP_ModelViewerItemData candidate = _items[i];
                    if (candidate != null && candidate.UniqueID == uniqueId)
                    {
                        item = candidate;
                        return true;
                    }
                }
            }

            item = null;
            return false;
        }
    }
}
