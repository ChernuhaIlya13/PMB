using System;
using System.Collections.Generic;
using System.Linq;

namespace PMB.Cef.Core.FakeConfig
{
      public static class ChromeLanguageHelper
  {
    public static readonly Dictionary<ChromeLanguage, ChromeLanguageInfo> Languages = new Dictionary<ChromeLanguage, ChromeLanguageInfo>()
    {
      {
        ChromeLanguage.Ru,
        new ChromeLanguageInfo()
        {
          AcceptList = "ru-Ru,ru",
          Local = "ru-Ru",
          Name = "Русский",
          Language = ChromeLanguage.Ru
        }
      },
      {
        ChromeLanguage.EnUsa,
        new ChromeLanguageInfo()
        {
          AcceptList = "en-US,en",
          Local = "en-US",
          Name = "Английский(США)",
          Language = ChromeLanguage.EnUsa
        }
      },
      {
        ChromeLanguage.EnGb,
        new ChromeLanguageInfo()
        {
          AcceptList = "en-GB,en",
          Local = "en-GB",
          Name = "Английский(Англия)",
          Language = ChromeLanguage.EnGb
        }
      },
      {
        ChromeLanguage.Sw,
        new ChromeLanguageInfo()
        {
          AcceptList = "sv-SE,sv",
          Local = "sv-SE",
          Name = "Шведский",
          Language = ChromeLanguage.Sw
        }
      },
      {
        ChromeLanguage.De,
        new ChromeLanguageInfo()
        {
          AcceptList = "de-DE,de",
          Local = "de-DE",
          Name = "Германский",
          Language = ChromeLanguage.De
        }
      },
      {
        ChromeLanguage.Fr,
        new ChromeLanguageInfo()
        {
          AcceptList = "fr-FR,fr",
          Local = "fr-FR",
          Name = "Французский",
          Language = ChromeLanguage.Fr
        }
      },
      {
        ChromeLanguage.It,
        new ChromeLanguageInfo()
        {
          AcceptList = "it-IT,it",
          Local = "it-IT",
          Name = "Итальянский",
          Language = ChromeLanguage.It
        }
      },
      {
        ChromeLanguage.Kz,
        new ChromeLanguageInfo()
        {
          AcceptList = "kk-KZ,kk",
          Local = "kk-KZ",
          Name = "Казахский",
          Language = ChromeLanguage.Kz
        }
      }
    };

    static ChromeLanguageHelper()
    {
      Array values = Enum.GetValues(typeof (ChromeLanguage));
      if (ChromeLanguageHelper.Languages.Count != values.Length)
        throw new ArgumentException("Не все языки учтенны!");
    }

    public static ChromeLanguageInfo GetFullInfo(ChromeLanguage language)
    {
      if (!ChromeLanguageHelper.Languages.ContainsKey(language))
        throw new ArgumentOutOfRangeException(string.Format("Этот язык {0} не поддерживается", (object) language));
      return ChromeLanguageHelper.Languages[language];
    }

    public static string ToAcceptList(this ChromeLanguage lang)
    {
      List<ChromeLanguage> echromeLanguageList = new List<ChromeLanguage>()
      {
        lang
      };
      if (lang != ChromeLanguage.EnUsa)
        echromeLanguageList.Add(ChromeLanguage.EnUsa);
      return ChromeLanguageHelper.GetAcceptList((IEnumerable<ChromeLanguage>) echromeLanguageList);
    }

    private static string GetAcceptList(IEnumerable<ChromeLanguage> langs)
    {
      string str = "";
      foreach (ChromeLanguage lang in langs)
        str = !string.IsNullOrWhiteSpace(str) ? str + "," + ChromeLanguageHelper.Languages[lang].AcceptList : ChromeLanguageHelper.Languages[lang].AcceptList;
      return str;
    }

    public static string ToNormalString(this ChromeLanguage lang)
    {
      return ChromeLanguageHelper.Languages[lang].Name;
    }

    public static ChromeLanguage FindLang(string langstr)
    {
      foreach (KeyValuePair<ChromeLanguage, ChromeLanguageInfo> language in ChromeLanguageHelper.Languages)
      {
        if (language.Value.Name.ToLower() == langstr.ToLower().Trim())
          return language.Key;
      }
      return ChromeLanguage.EnUsa;
    }

    public static string ToLocal(this ChromeLanguage lang)
    {
      return ChromeLanguageHelper.Languages[lang].Local;
    }

    public static List<ChromeLanguage> GetAllLanguages()
    {
      return ChromeLanguageHelper.Languages.Keys.ToList<ChromeLanguage>();
    }

    public static List<ChromeLanguageInfo> GetAllLanguagesInfo()
    {
      return ChromeLanguageHelper.Languages.Values.ToList<ChromeLanguageInfo>();
    }
  }
}