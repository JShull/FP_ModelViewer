namespace FuzzPhyte.ModelViewer.Editor
{
    using UnityEditor;
    using UnityEngine;

    /// <summary>
    /// Keeps imported meshes runtime-readable when viewer items are added directly
    /// to a catalog through the Inspector.
    /// </summary>
    [CustomEditor(typeof(FP_ModelViewerCatalogData))]
    public sealed class FP_ModelViewerCatalogDataEditor : UnityEditor.Editor
    {
        private string _lastPreparationMessage;
        private MessageType _lastPreparationMessageType = MessageType.None;

        public override void OnInspectorGUI()
        {
            bool changed = DrawDefaultInspector();
            if (changed)
            {
                var catalog = (FP_ModelViewerCatalogData)target;
                bool success = FP_ModelViewerAssetUtility.EnsureCatalogMeshReadWriteEnabled(
                    catalog,
                    out int updatedImporterCount,
                    out _lastPreparationMessage);
                _lastPreparationMessageType = success
                    ? (updatedImporterCount > 0 ? MessageType.Info : MessageType.None)
                    : MessageType.Error;
                if (success && updatedImporterCount > 0)
                {
                    Debug.Log($"[FP Model Viewer] {_lastPreparationMessage}", catalog);
                }
                else if (!success)
                {
                    Debug.LogError(
                        $"[FP Model Viewer] Catalog mesh Read/Write preparation failed:\n" +
                        _lastPreparationMessage,
                        catalog);
                }
            }

            if (!string.IsNullOrWhiteSpace(_lastPreparationMessage) &&
                _lastPreparationMessageType != MessageType.None)
            {
                EditorGUILayout.HelpBox(
                    _lastPreparationMessage,
                    _lastPreparationMessageType);
            }
        }
    }
}
