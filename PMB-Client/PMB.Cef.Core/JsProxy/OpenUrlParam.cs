namespace PMB.Cef.Core.JsProxy
{
    public record OpenUrlParam(string Url, string EventId = null, string SportId = null, string FirstTeam = null,
        string SecondTeam = null);
}