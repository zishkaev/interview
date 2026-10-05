using UnityEngine;

public static class StringSplitter
{
    /// <summary>
    /// Делит строку на две части по разделителю " — " (тире с пробелами).
    /// </summary>
    public static (string before, string after) SplitByDash(string input)
    {
        string separator = " — ";
        int index = input.IndexOf(separator);
        
        if (index < 0)
            return (input, string.Empty);
        
        string before = input.Substring(0, index).Trim();
        string after = input.Substring(index + separator.Length).Trim();
        
        return (before, after);
    }
}