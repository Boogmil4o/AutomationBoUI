using System.Net;
using System.Net.Security;
using System.Text;
using System.Text.Json;

namespace AutomationBoUI;

internal class GetData
{
    private HttpClient client;

    public GetData()
    {
        var handler = new HttpClientHandler();
        handler.ClientCertificateOptions = ClientCertificateOption.Manual;
        handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => true;

        ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) =>
        {
            return errors == SslPolicyErrors.None;
        };

        client = new HttpClient(handler);
        client.DefaultRequestHeaders.Add("User-Agent", "Automation QA");
    }

    public async Task<string?> FetchToken(User user)
    {
        var url = "https://STLogin.naxex-tech.com/loginService.svc/json/Login";
        JsonSerializerOptions jso = new JsonSerializerOptions();
        jso.Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
        var json = JsonSerializer.Serialize(user, jso);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var response = await client.PostAsync(url, content);
        var responseString = await response.Content.ReadAsStringAsync();
        var auth = JsonSerializer.Deserialize<Auth>(responseString);
        if (auth == null || auth.Token == null) return null;
        return auth.Token;
    }

    public async Task<LegalDataConfig?> GetLegalData(string legalDataUrl)
    {
        var legalDataResponse = await client.GetAsync(legalDataUrl);
        if (legalDataResponse.StatusCode != HttpStatusCode.OK)
        {
            return new LegalDataConfig(new LegalDataKeys(null, null));
        }
        var legalDataResponseString = await legalDataResponse.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<LegalDataConfig>(legalDataResponseString);
    }

    public Tuple<AcademyVideosItem[]?, AppCuesWalkThroughItem[]?> GetItems(LegalDataConfig? legalDataConfig)
    {
        var defaultResponse = new Tuple<AcademyVideosItem[]?, AppCuesWalkThroughItem[]?>(null, null);
        if (legalDataConfig == null)
        {
            return defaultResponse;
        }
        var videosRawValue = legalDataConfig.keys?.AcademyVideos?.value;
        var walkThroughRawValue = legalDataConfig.keys?.AppCuesWalkThrough?.value;

        AcademyVideos? videosValue = null;
        try
        {
            videosValue = videosRawValue != null ? JsonSerializer.Deserialize<AcademyVideos>((string)videosRawValue) : null;
        }
        catch (Exception) { }

        AppCuesWalkThrough? walkThroughValue = null;
        try
        {
            walkThroughValue = walkThroughRawValue != null ? JsonSerializer.Deserialize<AppCuesWalkThrough>((string)walkThroughRawValue) : null;
        }
        catch (Exception) { }

        return new Tuple<AcademyVideosItem[]?, AppCuesWalkThroughItem[]?>(videosValue?.academyVideos, walkThroughValue?.walkThrough);
    }

    public record AcademyVideosItem(string title, int[] groups, string videoId, string url);
    public record AppCuesWalkThroughItem(string title, int[] groups, string contentId, string target);
    public record AcademyVideos(AcademyVideosItem[] academyVideos);

    public record AppCuesWalkThrough(AppCuesWalkThroughItem[] walkThrough);
    public record LegalDataKeyObject(string value, string type);

    public record LegalDataKeys(LegalDataKeyObject? AppCuesWalkThrough, LegalDataKeyObject? AcademyVideos);

    public record LegalDataConfig(LegalDataKeys keys);

    public record Auth(string Token);

    public record User(string Username, string Password, int BrandId);

}
