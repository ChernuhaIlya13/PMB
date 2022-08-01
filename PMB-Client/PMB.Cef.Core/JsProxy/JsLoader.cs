using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PMB.Cef.Core.JsProxy
{
    public class JsLoader
    {
        private readonly Dictionary<string, string> _allCodes;
        
        public JsLoader(params string[] files)
        {
            var filesList = files.ToList();
            filesList.Add("fakeinjection.js");
            files = filesList.ToArray();
            _allCodes = new Dictionary<string, string>();
            LoadAllPacks(files);
        }

        public string GetJsCode(string bookmaker)
        {
            if (!_allCodes.ContainsKey(bookmaker)) 
                return string.Empty;

            return _allCodes.TryGetValue(bookmaker, out var value) ? value : string.Empty;
        }

        private void AddCode(string bookmaker,string code) => _allCodes.Add(bookmaker, code);
        
        private void LoadAllPacks(IEnumerable<string> files)
        {
            foreach (var file in files)
            {
                LoadResource(file);
            }
        }
        
        private void LoadResource(string file)
        {
            var key = file.Split('.').First();
            var assembly = Assembly.GetExecutingAssembly();
            var jsFile = assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(file));
            string js;
            
            using (var s = assembly.GetManifestResourceStream(jsFile))
            {
                if (s == null)
                {
                    return;
                }
                
                using (var reader = new StreamReader(s))
                {
                    js = reader.ReadToEnd();
                }
            }

            if (string.IsNullOrEmpty(js))
            {
                throw new FileNotFoundException(file);
            }
            
            AddCode(key, js);
        }
    }
}
