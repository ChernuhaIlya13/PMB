using System.Reactive.Subjects;

namespace PMB.Utils;

public static class ViewModelUpdater
{
    public static BehaviorSubject<float> ForkProfit = new(0);

    public static Subject<string> BrowserRemove = new();
}