namespace OpenDay
{
    /// <summary>
    /// The database tables the Level 3 keys and doors belong to. A door is a
    /// foreign key referencing one of these; the matching primary key opens it.
    /// </summary>
    public enum TableKey
    {
        None,
        Customers,
        Orders
    }
}
