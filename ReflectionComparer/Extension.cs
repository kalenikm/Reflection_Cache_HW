namespace ReflectionComparer
{
    public static class Extension
    {
        public static bool DeepCompare<T>(this T x, T y)
        {
            var comparer = new Comparer<T>();
            return comparer.Equals(x, y);
        }
    }
}