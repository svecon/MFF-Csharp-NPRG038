
namespace DiffAlgorithm
{
    /// <summary>
    /// Short Middle Snake point
    /// 
    /// Strucure to hold a SMS point in the Diff algorithm.
    /// </summary>
    internal struct SMSPoint
    {
        internal int x, y;

        public SMSPoint(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
