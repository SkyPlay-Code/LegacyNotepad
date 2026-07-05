namespace Notepad;

public static class SearchState
{
    public static string LastSearchText { get; set; } = "";
    public static string LastReplaceText { get; set; } = "";
    public static bool LastMatchCase { get; set; } = false;
}
