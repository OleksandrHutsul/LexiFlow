namespace LexiFlow.Services.State;

public class StatisticsChangeNotifier
{
    public event Action? Changed;

    public void NotifyChanged()
    {
        Changed?.Invoke();
    }
}