using System;
using System.Threading.Tasks;
using CefSharp;
using Newtonsoft.Json;

namespace PMB.Cef.Core.JsProxy
{
    public static class JsProxyExtensions
    {
        /// <summary>
        /// Acинхронный метод c возвратом результата
        /// Если TResult - примитив, то десериализации не будет
        /// </summary>
        /// <returns>Возвращаемое значение</returns>
        public static async Task<TResult> ExecuteAsync<TResult>(
            this BotBrowser botBrowser, string methodName, params object[] methodArgs)
        {
            var script = WebBrowserExtensions.GetScriptForJavascriptMethodWithArgs(methodName, methodArgs);
            var result = await botBrowser.EvaluateScriptAsPromiseAsync($"return await {script}");
            
            if (!result.Success || result.Result == null)
            {
                return default;
            }

            if (result.Result.GetType() == typeof(TResult))
            {
                return (TResult)result.Result;
            }
            
            var stringValue = JsonConvert.SerializeObject(result.Result);

            if (string.IsNullOrEmpty(stringValue))
                return default;

            if (ValidateType(typeof(TResult)))
                return JsonConvert.DeserializeObject<TResult>(stringValue);

            return default;
        }

        /// <summary>
        /// Acинхронный метод без возврата результата
        /// </summary>
        /// <returns>Исполнился ли метод в js</returns>
        public static async Task<bool> ExecuteAsync(
            this BotBrowser botBrowser, string methodName, params object[] methodArgs)
        {
            var script = WebBrowserExtensions.GetScriptForJavascriptMethodWithArgs(methodName, methodArgs);
            var result = await botBrowser.EvaluateScriptAsPromiseAsync($"await {script}");
            
            return result.Success;
        }

        /// <summary>
        /// Синхронный метод c возвратом результата
        ///  Если TResult - примитив, то десериализации не будет
        /// </summary>
        /// <returns>Возвращаемое значение</returns>
        public static async Task<TResult> Execute<TResult>(
            this BotBrowser botBrowser, string methodName, params object[] methodArgs)
        {
            var script = WebBrowserExtensions.GetScriptForJavascriptMethodWithArgs(methodName, methodArgs);
            var result = await botBrowser.EvaluateScriptAsync($"{script}");
            
            if (!result.Success || result.Result == null)
            {
                return default;
            }

            if (result.Result.GetType() == typeof(TResult))
            {
                return (TResult)result.Result;
            }
            
            var stringValue = JsonConvert.SerializeObject(result.Result);

            if (string.IsNullOrEmpty(stringValue))
                return default;

            if (ValidateType(typeof(TResult)))
                return JsonConvert.DeserializeObject<TResult>(stringValue);

            return default;
        }

        /// <summary>
        /// Синхронный метод без возврата результата
        /// </summary>
        /// <returns>Исполнился ли метод в js</returns>
        public static async Task<bool> Execute(
            this BotBrowser botBrowser, string methodName, params object[] methodArgs)
        {
            var script = WebBrowserExtensions.GetScriptForJavascriptMethodWithArgs(methodName, methodArgs);
            var result = await botBrowser.EvaluateScriptAsync($"{script};");

            return result.Success;
        }
        
        private static bool ValidateType(Type type)
        {
            return type.FullName != "System.Boolean" &&
                   type.FullName != "System.String" &&
                   type.FullName != "System.Int32" &&
                   type.FullName != "System.Int64" &&
                   type.FullName != "System.Float" &&
                   type.FullName != "System.Double" &&
                   type.FullName != "System.Decimal";
        }
    }
}