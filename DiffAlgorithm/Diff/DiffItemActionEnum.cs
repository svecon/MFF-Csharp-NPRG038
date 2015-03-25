namespace DiffAlgorithm.Diff
{
    /// <summary>
    /// Which version of diff item should be kept and used?
    /// </summary>
    public enum DiffItemActionEnum
    {
        Default, RevertToLocal, ApplyRemote
    }
}