namespace FuzzPhyte.ModelViewer.Tests
{
    using NUnit.Framework;

    public sealed class FPModelViewerPaginationTests
    {
        [Test]
        public void ThreeByThree_WithFiftyItems_ReturnsSixPages()
        {
            int pageCount = FP_ModelViewerPagination.GetPageCount(50, 3, 3);

            Assert.That(pageCount, Is.EqualTo(6));
        }

        [Test]
        public void LastPage_WithFiftyItems_ReturnsFinalFiveItems()
        {
            FP_ModelViewerPageRange range =
                FP_ModelViewerPagination.GetPageRange(50, 3, 3, 5);

            Assert.That(range.StartIndex, Is.EqualTo(45));
            Assert.That(range.Count, Is.EqualTo(5));
            Assert.That(range.EndExclusive, Is.EqualTo(50));
        }

        [Test]
        public void HorizontalFiveItemStrip_ReturnsOnePage()
        {
            int pageCount = FP_ModelViewerPagination.GetPageCount(5, 1, 5);
            FP_ModelViewerPageRange range =
                FP_ModelViewerPagination.GetPageRange(5, 1, 5, 0);

            Assert.That(pageCount, Is.EqualTo(1));
            Assert.That(range.StartIndex, Is.Zero);
            Assert.That(range.Count, Is.EqualTo(5));
        }

        [Test]
        public void RequestedPage_IsClampedToAvailableRange()
        {
            FP_ModelViewerPageRange range =
                FP_ModelViewerPagination.GetPageRange(10, 2, 2, 99);

            Assert.That(range.StartIndex, Is.EqualTo(8));
            Assert.That(range.Count, Is.EqualTo(2));
        }

        [TestCase(1, 1, 1)]
        [TestCase(5, 3, 2)]
        [TestCase(14, 4, 4)]
        public void SquareGrid_ExpandsForItemCount(
            int itemCount,
            int expectedColumns,
            int expectedRows)
        {
            Assert.That(
                FP_ModelViewerPagination.GetSquareGridColumns(itemCount),
                Is.EqualTo(expectedColumns));
            Assert.That(
                FP_ModelViewerPagination.GetSquareGridRows(itemCount),
                Is.EqualTo(expectedRows));
        }
    }
}
