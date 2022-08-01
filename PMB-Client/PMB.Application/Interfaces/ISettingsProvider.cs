using PMB.Domain.BrowserModels;

namespace PMB.Application.Interfaces
{
    public interface ISettingsProvider
    {
        ForkSettings GetSettings();

        void Save(ForkSettings settings);
    }
}