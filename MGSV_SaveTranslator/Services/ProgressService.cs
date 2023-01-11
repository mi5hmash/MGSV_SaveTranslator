using MGSVST_Core.Interfaces;

namespace MGSV_SaveTranslator.Services;

public class ProgressService : IProgressService
{
    private int _numeric;
    private uint _tasksDone;

    public int Numeric
    {
        get => _numeric;
        private set
        {
            _numeric = value; 
            Text = $"{_numeric}%";
        }
    }

    public uint TasksTotal { get; set; } = 1;

    public double Double { get; private set; }

    public string Text { get; private set; } = "0%";

    public uint TasksDone
    {
        get => _tasksDone;
        set
        {
            _tasksDone = value;
            Double = value / (double) TasksTotal;
            Numeric = (int)(Double * 100);
        }
    }
    
    public void Reset()
    {
        TasksTotal = 1;
        Numeric = 0;
    }

    public void Complete()
    {
        TasksDone = TasksTotal;
    }
}