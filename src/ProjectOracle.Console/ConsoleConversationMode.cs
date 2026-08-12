namespace ProjectOracle.ConsoleApp;

/// <summary>
/// Keeps direct Yala conversation targeting separate from the editable command buffer.
/// Ctrl+Y enters this mode once; it remains active across replies until Escape exits it.
/// </summary>
public sealed class ConsoleConversationMode
{
    public bool YalaMode { get; private set; }
    public string Prompt => YalaMode ? "> (yala " : "> ";

    public void EnterYala() => YalaMode = true;

    public void Escape(ConsoleInputLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        line.Clear();
        YalaMode = false;
    }

    public string BuildCommand(string text)
    {
        string value = text ?? string.Empty;
        return YalaMode && !string.IsNullOrWhiteSpace(value)
            ? $"(yala {value}"
            : value;
    }
}
