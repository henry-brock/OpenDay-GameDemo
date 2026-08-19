namespace OpenDay
{
    /// <summary>
    /// The Computer Science modules the open-day demo is built around.
    /// Each module becomes a themed feature somewhere in the level
    /// (e.g. Databases -> a physical key you must collect).
    /// Scope is fixed at three levels, each pairing related modules; Team
    /// Project was repurposed into the Title Screen rather than built out as
    /// its own level.
    /// </summary>
    public enum CSModule
    {
        ObjectOrientedProgramming,      // Level 1
        DataStructuresAndAlgorithms,    // Level 1
        SoftwareEngineering,            // Level 2
        SoftwareProjectManagement,      // Level 2
        Databases                       // Level 3
    }
}
