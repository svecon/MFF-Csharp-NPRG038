
namespace DiffAlgorithm.Diff
{
    /// <summary>
    /// Short Middle Snake point
    /// 
    /// Strucure to hold a SMS point in the Diff algorithm.
    /// </summary>
    internal struct SmsPoint
    {
        internal int X, Y;

        public SmsPoint(int x, int y)
        {
            X = x;
            Y = y;
        }
    }
}
