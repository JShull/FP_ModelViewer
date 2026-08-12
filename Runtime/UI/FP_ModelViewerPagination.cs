namespace FuzzPhyte.ModelViewer
{
    public readonly struct FP_ModelViewerPageRange
    {
        public readonly int StartIndex;
        public readonly int Count;

        public int EndExclusive => StartIndex + Count;
        public bool IsEmpty => Count == 0;

        public FP_ModelViewerPageRange(int startIndex, int count)
        {
            StartIndex = startIndex;
            Count = count;
        }
    }

    public static class FP_ModelViewerPagination
    {
        public static int GetItemsPerPage(int rows, int columns)
        {
            int safeRows = rows < 1 ? 1 : rows;
            int safeColumns = columns < 1 ? 1 : columns;
            return safeRows * safeColumns;
        }

        public static int GetPageCount(int itemCount, int rows, int columns)
        {
            if (itemCount <= 0)
            {
                return 0;
            }

            int itemsPerPage = GetItemsPerPage(rows, columns);
            return (itemCount + itemsPerPage - 1) / itemsPerPage;
        }

        public static int ClampPageIndex(int pageIndex, int itemCount, int rows, int columns)
        {
            int pageCount = GetPageCount(itemCount, rows, columns);
            if (pageCount == 0 || pageIndex < 0)
            {
                return 0;
            }

            return pageIndex >= pageCount ? pageCount - 1 : pageIndex;
        }

        public static FP_ModelViewerPageRange GetPageRange(
            int itemCount,
            int rows,
            int columns,
            int pageIndex)
        {
            if (itemCount <= 0)
            {
                return new FP_ModelViewerPageRange(0, 0);
            }

            int itemsPerPage = GetItemsPerPage(rows, columns);
            int safePageIndex = ClampPageIndex(pageIndex, itemCount, rows, columns);
            int startIndex = safePageIndex * itemsPerPage;
            int remaining = itemCount - startIndex;
            int count = remaining < itemsPerPage ? remaining : itemsPerPage;
            return new FP_ModelViewerPageRange(startIndex, count);
        }
    }
}
