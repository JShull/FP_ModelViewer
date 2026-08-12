namespace FuzzPhyte.ModelViewer
{
    using System.Collections.Generic;
    using FuzzPhyte.Placement.OrbitalCamera;

    public static class FP_ModelViewerViewUtility
    {
        private static readonly FP_ViewCubeHit[] ThumbnailViews =
        {
            FP_ViewCubeHit.Front,
            FP_ViewCubeHit.Back,
            FP_ViewCubeHit.Left,
            FP_ViewCubeHit.Right,
            FP_ViewCubeHit.Top,
            FP_ViewCubeHit.Bottom,
            FP_ViewCubeHit.TopFrontRight,
            FP_ViewCubeHit.TopFrontLeft,
            FP_ViewCubeHit.TopBackRight,
            FP_ViewCubeHit.TopBackLeft,
            FP_ViewCubeHit.BottomFrontRight,
            FP_ViewCubeHit.BottomFrontLeft,
            FP_ViewCubeHit.BottomBackRight,
            FP_ViewCubeHit.BottomBackLeft
        };

        public static IReadOnlyList<FP_ViewCubeHit> SupportedThumbnailViews => ThumbnailViews;

        public static bool IsSupportedThumbnailView(FP_ViewCubeHit view)
        {
            for (int i = 0; i < ThumbnailViews.Length; i++)
            {
                if (ThumbnailViews[i] == view)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
