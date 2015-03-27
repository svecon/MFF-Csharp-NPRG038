﻿namespace DiffAlgorithm.ThreeWay
{
    /// <summary>
    /// Which version of diff item should be kept and used?
    /// </summary>
    public enum Diff3ItemActionEnum
    {
        Default, RevertToBase, ApplyLocal, ApplyRemote
    }
}