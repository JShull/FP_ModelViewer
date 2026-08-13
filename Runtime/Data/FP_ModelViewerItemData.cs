namespace FuzzPhyte.ModelViewer
{
    using System.Collections.Generic;
    using FuzzPhyte.Placement.OrbitalCamera;
    using FuzzPhyte.Utility;
    using FuzzPhyte.Utility.Meta;
    using UnityEngine;

    [CreateAssetMenu(
        fileName = "FP_ModelViewerItemData",
        menuName = "FuzzPhyte/Model Viewer/Item Data")]
    public sealed class FP_ModelViewerItemData : FP_Data
    {
        [Header("Identity")]
        [SerializeField] private string _displayName;
        [SerializeField, TextArea] private string _description;
        [SerializeField] private List<FP_Tag> _tags = new List<FP_Tag>();

        [Header("Model")]
        [SerializeField] private FP_ModelDisplayData _modelDisplayData;
        [SerializeField] private GameObject _includedPrefab;
        [SerializeField] private string _assetKey;
        [SerializeField] private string _downloadUrl;

        [Header("Presentation")]
        [SerializeField] private FP_ViewCubeHit _defaultView = FP_ViewCubeHit.TopFrontRight;
        [SerializeField] private FP_ViewCubeHit _coverView = FP_ViewCubeHit.TopFrontRight;
        [SerializeField] private List<FP_ModelThumbnailReference> _thumbnails =
            new List<FP_ModelThumbnailReference>();

        public string DisplayName => string.IsNullOrWhiteSpace(_displayName)
            ? (_modelDisplayData != null && !string.IsNullOrWhiteSpace(_modelDisplayData.DisplayName)
                ? _modelDisplayData.DisplayName
                : name)
            : _displayName;
        public string Description => _description;
        public IReadOnlyList<FP_Tag> Tags => _tags;
        public FP_ModelDisplayData ModelDisplayData => _modelDisplayData;
        public GameObject IncludedPrefab => _includedPrefab;
        public string AssetKey => _assetKey;
        public string DownloadUrl => _downloadUrl;
        public FP_ViewCubeHit DefaultView => _defaultView;
        public FP_ViewCubeHit CoverView => _coverView;
        public IReadOnlyList<FP_ModelThumbnailReference> Thumbnails => _thumbnails;
        public Texture2D CoverTexture => TryGetThumbnail(_coverView, out Texture2D texture)
            ? texture
            : null;

        public bool HasTag(FP_Tag tag)
        {
            return tag != null && _tags.Contains(tag);
        }

        public bool HasAnyTag(IReadOnlyList<FP_Tag> tags)
        {
            if (tags == null)
            {
                return false;
            }

            for (int i = 0; i < tags.Count; i++)
            {
                if (HasTag(tags[i]))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasAllTags(IReadOnlyList<FP_Tag> tags)
        {
            if (tags == null)
            {
                return false;
            }

            bool hasValidTag = false;
            for (int i = 0; i < tags.Count; i++)
            {
                if (tags[i] == null)
                {
                    continue;
                }

                hasValidTag = true;
                if (!HasTag(tags[i]))
                {
                    return false;
                }
            }

            return hasValidTag;
        }

        public bool TryGetThumbnail(FP_ViewCubeHit view, out Texture2D texture)
        {
            for (int i = 0; i < _thumbnails.Count; i++)
            {
                FP_ModelThumbnailReference thumbnail = _thumbnails[i];
                if (thumbnail != null && thumbnail.View == view)
                {
                    texture = thumbnail.Texture;
                    return texture != null;
                }
            }

            texture = null;
            return false;
        }

        public bool SetThumbnail(FP_ViewCubeHit view, Texture2D texture, string caption = "")
        {
            if (!FP_ModelViewerViewUtility.IsSupportedThumbnailView(view) || texture == null)
            {
                return false;
            }

            for (int i = 0; i < _thumbnails.Count; i++)
            {
                FP_ModelThumbnailReference thumbnail = _thumbnails[i];
                if (thumbnail != null && thumbnail.View == view)
                {
                    thumbnail.Update(texture, caption);
                    return true;
                }
            }

            _thumbnails.Add(new FP_ModelThumbnailReference(view, texture, caption));
            return true;
        }

        public bool RemoveThumbnail(FP_ViewCubeHit view)
        {
            for (int i = _thumbnails.Count - 1; i >= 0; i--)
            {
                FP_ModelThumbnailReference thumbnail = _thumbnails[i];
                if (thumbnail != null && thumbnail.View == view)
                {
                    _thumbnails.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        public bool SetCoverView(FP_ViewCubeHit view)
        {
            if (!FP_ModelViewerViewUtility.IsSupportedThumbnailView(view))
            {
                return false;
            }

            _coverView = view;
            return true;
        }
    }
}
