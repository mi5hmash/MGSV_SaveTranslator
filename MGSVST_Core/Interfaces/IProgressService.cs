namespace MGSVST_Core.Interfaces;

public interface IProgressService
{
    int Numeric { get; }
    uint TasksTotal { get; set; }
    double Double { get; }
    string Text { get; }
    uint TasksDone { get; set; }
    void Reset();
    void Complete();
}