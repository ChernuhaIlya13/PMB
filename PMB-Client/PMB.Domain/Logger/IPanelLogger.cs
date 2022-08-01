using System.ComponentModel;
using System.Threading.Tasks;

namespace PMB.Domain.Logger
{
    public interface IPanelLogger : INotifyPropertyChanged
    {
        Task AddInfoLog(string message);

        Task AddForkInfo(string info, string team, string teams, string bookmakerName, string sportType);

        Task Clear();
    }
}