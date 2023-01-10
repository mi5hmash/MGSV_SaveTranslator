namespace MGSVST_Core.Models;

public class ProgressModel
{
    public string Text { get; private set; } = "0%";

    private int _numeric;
    private uint _tasksDone;

    public int Numeric
    {
        get => _numeric;
        set
        {
            _numeric = value; 
            Text = $"{_numeric}%";
        }
    }

    public uint TasksTotal { get; set; } = 0;

    public uint TasksDone
    {
        get => _tasksDone;
        set
        {
            _tasksDone = value;
            Numeric = (int)((value / (double) TasksTotal) * 100);
        }
    }
}