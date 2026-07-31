namespace ilyvion.Laboratory;

public static class IlyvionArray
{
    public static void SortBy<T, TSortBy>(this T[] list, Func<T, TSortBy> selector)
        where TSortBy : IComparable<TSortBy>
    {
        if (list == null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        if (list.Length > 1)
        {
            Array.Sort(list, (T a, T b) => selector(a).CompareTo(selector(b)));
        }
    }

    public static void SortBy<T, TSortBy>(
        this T[] list, int index, int length, Func<T, TSortBy> selector)
        where TSortBy : IComparable<TSortBy>
    {
        if (list == null)
        {
            throw new ArgumentNullException(nameof(list));
        }

        if (list.Length > 1)
        {
            Array.Sort(list, index, length, Comparer<T>.Create((T a, T b) =>
                selector(a).CompareTo(selector(b))));
        }
    }

}
