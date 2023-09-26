using System.Net.Http.Headers;
using System.Text;

public class FormartterHttpClient
{
    // Build string by autorization - Check if token is expired
    private static string GetAuthorization(string userName, string accessToken)
    {
        var authenticationString = $"{userName}:{accessToken}";

        return Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
    }

    // Get response body jira
    public string GenerateResponseJira(string url, string userName, string accessToken)
    {
        try
        {
            if (string.IsNullOrEmpty(url))
               throw new Exception("URL não podem ser vazia");

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetAuthorization(userName, accessToken));
            var httpClientResponse = httpClient.GetAsync(url).Result;
            httpClientResponse.EnsureSuccessStatusCode();

            return httpClientResponse.Content.ReadAsStringAsync().Result;          

        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }
}