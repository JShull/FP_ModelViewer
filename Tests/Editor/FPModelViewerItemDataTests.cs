namespace FuzzPhyte.ModelViewer.Tests
{
    using System.Collections.Generic;
    using FuzzPhyte.Placement.OrbitalCamera;
    using FuzzPhyte.Utility.Meta;
    using NUnit.Framework;
    using UnityEditor;
    using UnityEngine;

    public sealed class FPModelViewerItemDataTests
    {
        [Test]
        public void SupportedThumbnailViews_ContainsSixFacesAndEightCorners()
        {
            IReadOnlyList<FP_ViewCubeHit> views =
                FP_ModelViewerViewUtility.SupportedThumbnailViews;
            var uniqueViews = new HashSet<FP_ViewCubeHit>(views);

            Assert.That(views.Count, Is.EqualTo(14));
            Assert.That(uniqueViews.Count, Is.EqualTo(14));
            Assert.That(uniqueViews, Does.Contain(FP_ViewCubeHit.Front));
            Assert.That(uniqueViews, Does.Contain(FP_ViewCubeHit.BottomBackLeft));
        }

        [Test]
        public void SetThumbnail_UpdatesCoverTextureWithoutCreatingDuplicateView()
        {
            var item = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var firstTexture = new Texture2D(8, 8);
            var replacementTexture = new Texture2D(16, 16);

            try
            {
                Assert.That(item.SetCoverView(FP_ViewCubeHit.TopFrontRight), Is.True);
                Assert.That(
                    item.SetThumbnail(FP_ViewCubeHit.TopFrontRight, firstTexture),
                    Is.True);
                Assert.That(
                    item.SetThumbnail(FP_ViewCubeHit.TopFrontRight, replacementTexture),
                    Is.True);

                Assert.That(item.Thumbnails.Count, Is.EqualTo(1));
                Assert.That(item.CoverTexture, Is.SameAs(replacementTexture));
            }
            finally
            {
                Object.DestroyImmediate(firstTexture);
                Object.DestroyImmediate(replacementTexture);
                Object.DestroyImmediate(item);
            }
        }

        [Test]
        public void SetThumbnail_RejectsEdgeView()
        {
            var item = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var texture = new Texture2D(8, 8);

            try
            {
                bool result = item.SetThumbnail(FP_ViewCubeHit.TopFront, texture);

                Assert.That(result, Is.False);
                Assert.That(item.Thumbnails, Is.Empty);
            }
            finally
            {
                Object.DestroyImmediate(texture);
                Object.DestroyImmediate(item);
            }
        }

        [Test]
        public void Tags_UseFPTagAssetIdentityForMembership()
        {
            var item = ScriptableObject.CreateInstance<FP_ModelViewerItemData>();
            var furniture = ScriptableObject.CreateInstance<FP_Tag>();
            var outdoor = ScriptableObject.CreateInstance<FP_Tag>();

            try
            {
                furniture.TagName = "Furniture";
                outdoor.TagName = "Outdoor";

                var serializedItem = new SerializedObject(item);
                SerializedProperty tags = serializedItem.FindProperty("_tags");
                tags.arraySize = 2;
                tags.GetArrayElementAtIndex(0).objectReferenceValue = furniture;
                tags.GetArrayElementAtIndex(1).objectReferenceValue = null;
                serializedItem.ApplyModifiedPropertiesWithoutUndo();

                Assert.That(item.Tags.Count, Is.EqualTo(2));
                Assert.That(item.Tags[0], Is.SameAs(furniture));
                Assert.That(item.HasTag(furniture), Is.True);
                Assert.That(item.HasTag(outdoor), Is.False);
                Assert.That(item.HasTag(null), Is.False);
                Assert.That(
                    item.HasAnyTag(new FP_Tag[] { null, outdoor, furniture }),
                    Is.True);
                Assert.That(item.HasAnyTag(new[] { outdoor }), Is.False);
                Assert.That(item.HasAnyTag(null), Is.False);
            }
            finally
            {
                Object.DestroyImmediate(item);
                Object.DestroyImmediate(furniture);
                Object.DestroyImmediate(outdoor);
            }
        }
    }
}
