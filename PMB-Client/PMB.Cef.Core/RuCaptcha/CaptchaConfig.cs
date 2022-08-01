using System;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace PMB.Cef.Core.RuCaptcha
{
    public class CaptchaConfig
    {
        private readonly Dictionary<string, string> _parameters;

        public Dictionary<string, string> Parameters => _parameters;

        public NameValueCollection GetParameters()
        {
            NameValueCollection nameValueCollection = new NameValueCollection();
            foreach (KeyValuePair<string, string> parameter in this._parameters)
                nameValueCollection.Add(parameter.Key, parameter.Value);
            return nameValueCollection;
        }

        public CaptchaConfig()
        {
            this._parameters = new Dictionary<string, string>();
        }

        public CaptchaConfig SetIsPhrase(bool value)
        {
            this._parameters["phrase"] = value ? "1" : "0";
            return this;
        }

        public CaptchaConfig SetRegisterSensitive(bool value)
        {
            this._parameters["regsense"] = value ? "1" : "0";
            return this;
        }

        public CaptchaConfig SetNeedCalc(bool value)
        {
            this._parameters["calc"] = value ? "1" : "0";
            return this;
        }

        public CaptchaConfig SetCharType(CaptchaCharTypeEnum value)
        {
            if (value == CaptchaCharTypeEnum.Default)
                this._parameters.Remove("numeric");
            else
                this._parameters["numeric"] = ((int)value).ToString();
            return this;
        }

        public CaptchaConfig SetMinLen(int? minLen)
        {
            if (minLen.HasValue)
            {
                if (minLen.Value >= 1)
                {
                    int? nullable = minLen;
                    int num = 20;
                    if (!(nullable.GetValueOrDefault() > num & nullable.HasValue))
                    {
                        this._parameters["min_len"] = minLen.Value.ToString();
                        goto label_6;
                    }
                }
                throw new ArgumentOutOfRangeException(nameof(minLen), (object)minLen.Value, "Количество знаков в отчете может быть в диапазоне от 1 до 20 символов.");
            }
            this._parameters.Remove("min_len");
        label_6:
            return this;
        }

        public CaptchaConfig SetMaxLen(int? maxLen)
        {
            if (maxLen.HasValue)
            {
                if (maxLen.Value >= 1)
                {
                    int? nullable = maxLen;
                    int num = 20;
                    if (!(nullable.GetValueOrDefault() > num & nullable.HasValue))
                    {
                        this._parameters["max_len"] = maxLen.Value.ToString();
                        goto label_6;
                    }
                }
                throw new ArgumentOutOfRangeException(nameof(maxLen), (object)maxLen.Value, "Количество знаков в отчете может быть в диапазоне от 1 до 20 символов.");
            }
            this._parameters.Remove("max_len");
        label_6:
            return this;
        }

        public CaptchaConfig SetLanguage(CaptchaLanguageEnum lang)
        {
            if (lang == CaptchaLanguageEnum.Default)
                this._parameters.Remove("language");
            else
                this._parameters["language"] = ((int)lang).ToString();
            return this;
        }

        public CaptchaConfig SetSoftId(string softId)
        {
            this._parameters["soft_id"] = softId;
            return this;
        }
    }
}
