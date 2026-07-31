namespace ilyvion.Laboratory;

// Source: <https://github.com/LogosBible/Logos.Utility/blob/master/src/Logos.Utility/OrderedEnumerable.cs>
// OrderedEnumerable<TSource> implements a lazy "OrderBy" for IEnumerable<TSource>. This class is instantiated by the
// EnumerableUtility.LazyOrder* methods.
// For more information, see http://code.logos.com/blog/2010/04/a_truly_lazy_orderby_in_linq.html
internal class OrderedEnumerable<TSource>(
    IEnumerable<TSource> source,
    ElementComparer<TSource> elementComparer
) : IOrderedEnumerable<TSource>
{
    public IOrderedEnumerable<TSource> CreateOrderedEnumerable<TKey>(
        Func<TSource, TKey> keySelector,
        IComparer<TKey> comparer,
        bool descending
    )
    {
        ElementComparer<TSource, TKey> elementComparer = new(
            keySelector,
            comparer ?? Comparer<TKey>.Default,
            descending,
            null
        );
        return new OrderedEnumerable<TSource>(m_source, m_elementComparer.Append(elementComparer));
    }

    public IEnumerator<TSource> GetEnumerator()
    {
        // build an array containing the input sequence
        var array = m_source.ToArray();

        // from the input array, create the sort keys used by each ordering
        m_elementComparer.CreateSortKeys(array);

        // instead of sorting the actual source array, we sort an array of indexes into that sort array; when the
        //   sort is finished, this array (when read in order) gives the indexes of the source array in sorted order
        var sourceIndexes = new int[array.Length];
        for (var i = 0; i < sourceIndexes.Length; i++)
        {
            sourceIndexes[i] = i;
        }

        // use a random number generator to pick a pivot for partitioning
        var random = new System.Random();

        // track the index of the item that is next to be returned
        var index = 0;

        // use a stack to simulate the recursive quicksort algorithm iteratively
        Stack<Range> stack = new();
        stack.Push(new Range(0, array.Length - 1));
        while (stack.Count > 0)
        {
            // get the range that needs to be sorted
            var currentRange = stack.Pop();

            if (currentRange.Last - currentRange.First <= 8)
            {
                // if the range is small enough, terminate the recursion and use insertion sort instead
                if (currentRange.First != currentRange.Last)
                {
                    InsertionSort(sourceIndexes, currentRange.First, currentRange.Last);
                }

                // yield all the items in this sorted sub-array
                System.Diagnostics.Debug.Assert(
                    currentRange.First == index,
                    "currentRange.First != index"
                );
                while (index <= currentRange.Last)
                {
                    yield return array[sourceIndexes[index]];
                    index++;
                }
            }
            else
            {
                // recursive case: pick a pivot in the array and partition the array around it
                var pivotIndex = Partition(
                    random,
                    sourceIndexes,
                    currentRange.First,
                    currentRange.Last
                );

                // "recurse" by pushing the ranges that still need to be processed (in reverse order) on to the stack
                stack.Push(new Range(pivotIndex + 1, currentRange.Last));
                stack.Push(new Range(pivotIndex, pivotIndex));
                stack.Push(new Range(currentRange.First, pivotIndex - 1));
            }
        }
    }

    // Performs a simple insertion sort of the values identified by the indexes in 'sourceIndexes' in the
    // inclusive range [first, last].
    // Algorithm taken from http://en.wikipedia.org/wiki/Insertion_sort.
    private void InsertionSort(int[] sourceIndexes, int first, int last)
    {
        // assume the first item is already sorted, then process all other indexes in the range
        for (var index = first + 1; index <= last; index++)
        {
            // find the place where value can be inserted into the already sorted elements
            var valueIndex = sourceIndexes[index];
            var insertIndex = index - 1;
            bool done;
            do
            {
                if (m_elementComparer.Compare(sourceIndexes[insertIndex], valueIndex) > 0)
                {
                    // move the elements to make room
                    sourceIndexes[insertIndex + 1] = sourceIndexes[insertIndex];
                    insertIndex--;
                    done = insertIndex < first;
                }
                else
                {
                    done = true;
                }
            } while (!done);

            // insert it in the correct location
            sourceIndexes[insertIndex + 1] = valueIndex;
        }
    }

    // Partitions the 'sourceIndexes' array into two halves around a randomly-selected pivot.
    // Returns the index of the pivot in the partitioned array.
    // Algorithm taken from: http://en.wikipedia.org/wiki/Quicksort
    private int Partition(System.Random random, int[] sourceIndexes, int first, int last)
    {
        // use random choice to pick the pivot
#pragma warning disable CA5394 // Do not use insecure randomness
        var randomPivotIndex = random.Next(first, last + 1);
#pragma warning restore CA5394 // Do not use insecure randomness

        // move the pivot to the end of the array
        Swap(ref sourceIndexes[randomPivotIndex], ref sourceIndexes[last]);
        var pivotIndex = sourceIndexes[last];

        // process all the items, moving them before/after the pivot
        var storeIndex = first;
        for (var i = first; i < last; i++)
        {
            if (m_elementComparer.Compare(sourceIndexes[i], pivotIndex) <= 0)
            {
                Swap(ref sourceIndexes[i], ref sourceIndexes[storeIndex]);
                storeIndex++;
            }
        }

        // move the pivot into the "middle" of the array; return its location
        Swap(ref sourceIndexes[storeIndex], ref sourceIndexes[last]);
        return storeIndex;
    }

    // Swaps two items.
    private static void Swap<T>(ref T first, ref T second) => (second, first) = (first, second);

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // Range represents an inclusive range of indexes into the array being sorted.
    private readonly struct Range(int first, int last)
    {
        public int First { get; } = first;

        public int Last { get; } = last;
    }

    private readonly IEnumerable<TSource> m_source = source;
    private readonly ElementComparer<TSource> m_elementComparer = elementComparer;
}

// ElementComparer is a helper class for OrderedEnumerable that can compare items in a source array given their indexes.
// It supports chaining multiple comparers together to allow secondary, tertiary, etc. sorting.
internal abstract class ElementComparer<TSource>
{
    // Compares the two items at the specified indexes.
    public abstract int Compare(int left, int right);

    // Creates all the keys needed to compare items.
    public abstract void CreateSortKeys(TSource[] source);

    // Creates a new ElementComparer by appending the specified ordering to this ordering, chaining them together.
    public abstract ElementComparer<TSource> Append(ElementComparer<TSource> next);
}

// Source: <https://github.com/LogosBible/Logos.Utility/blob/master/src/Logos.Utility/ElementComparer.cs>
internal class ElementComparer<TSource, TKey>(
    Func<TSource, TKey> keySelector,
    IComparer<TKey> comparer,
    bool descending,
    ElementComparer<TSource>? next
) : ElementComparer<TSource>
{
    public override void CreateSortKeys(TSource[] source)
    {
        // create the sort key from each item in the source array
        m_keys = new TKey[source.Length];
        for (var index = 0; index < source.Length; index++)
        {
            m_keys[index] = m_keySelector(source[index]);
        }

        // delegate to next if necessary
        m_next?.CreateSortKeys(source);
    }

    public override int Compare(int left, int right)
    {
        // invoke this level's comparer to get a basic result
        var result = m_comparer.Compare(m_keys![left], m_keys[right]);

        // if elements are different, return their relative order (inverting if descending)
        if (result != 0)
        {
            return m_isDescending ? -result : result;
        }

        // if there is a chained ordering, delegate to it
        if (m_next != null)
        {
            return m_next.Compare(left, right);
        }

        // elements are otherwise equal; to preserve stable sort, sort by original index
        return left - right;
    }

    public override ElementComparer<TSource> Append(ElementComparer<TSource> next)
    {
        // append the new ordering to the tail of the current chain
        var newNext = m_next == null ? next : m_next.Append(next);
        return new ElementComparer<TSource, TKey>(
            m_keySelector,
            m_comparer,
            m_isDescending,
            newNext
        );
    }

    private readonly Func<TSource, TKey> m_keySelector = keySelector;
    private readonly IComparer<TKey> m_comparer = comparer;
    private readonly bool m_isDescending = descending;
    private readonly ElementComparer<TSource>? m_next = next;
    private TKey[]? m_keys;
}
