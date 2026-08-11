using System.Text;

namespace ProjectOracle.ConsoleApp;

/// <summary>
/// Owns the command buffer independently of whatever is painted in the terminal.
/// Live-status refreshes are never allowed to mutate this buffer.
/// </summary>
public sealed class ConsoleInputLine
{
    private readonly StringBuilder _buffer = new();

    public int Length => _buffer.Length;
    public bool IsEmpty => _buffer.Length == 0;
    public string Text => _buffer.ToString();

    public void Append(char value)
    {
        if (char.IsControl(value)) return;
        _buffer.Append(value);
    }

    public bool Backspace()
    {
        if (_buffer.Length == 0) return false;
        _buffer.Remove(_buffer.Length - 1, 1);
        return true;
    }
}
