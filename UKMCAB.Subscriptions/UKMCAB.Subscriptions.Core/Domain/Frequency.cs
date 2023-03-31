/// <summary>
/// The frequency of email notifications on a given subscription
/// </summary>
public enum Frequency
{
    /// <summary>
    /// The user wants to be notified in (pseudo)  real-time
    /// </summary>
    Realtime,

    /// <summary>
    /// The user wants to be notified on a daily basis about changes
    /// </summary>
    Daily,

    /// <summary>
    /// The user wants to be notified weekly
    /// </summary>
    Weekly
}
