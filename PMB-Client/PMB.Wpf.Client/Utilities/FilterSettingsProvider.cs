using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Options;
using PMB.Application.Interfaces;
using PMB.Browsers.Common;
using PMB.Domain.BrowserModels;

namespace PMB.Wpf.Client.Utilities
{
    public class FilterSettingsProvider : ISettingsProvider
    {
        private readonly string _pathToSettings;
        private readonly SettingsOptions.BookmakerInfo[] _allBookmakers;
        private ForkSettings _settings;
        
        public FilterSettingsProvider(IOptions<SettingsOptions> options)
        {
            var settingsPath = options.Value.Path;
            _allBookmakers =  options.Value.Bookmakers;
            
            _pathToSettings = Path.Combine(Environment.CurrentDirectory, settingsPath);
        }
        
        public ForkSettings GetSettings()
        {
            if (_settings != null)
            {
                return _settings;
            }
            
            var settingsTxtExists = File.Exists(_pathToSettings);
            
            _settings = new ForkSettings
            {
                Bookmakers = _allBookmakers.Select(b => new Bookmaker { BookmakerName = b.Bookmaker.ToLower()}).ToList()
            };
            
            if (settingsTxtExists)
            {
                var body = File.ReadAllText(_pathToSettings);
                try
                {
                    _settings = JsonConvert.DeserializeObject<ForkSettings>(body);
                }
                catch (Exception)
                {
                    throw new ApplicationException("Фильтры не распарсились");
                }
            }

            return _settings;
        }

        public void Save(ForkSettings settings)
        {
            _settings = settings;
            File.WriteAllText(_pathToSettings, JsonConvert.SerializeObject(settings, Formatting.Indented));
        }
    }
}