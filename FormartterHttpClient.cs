using System.Net.Http.Headers;
using System.Text;

public class FormartterHttpClient
{
    // Build string by autorization - Check if token is expired
    private static string GetAuthorization()
    {
        var userName = "niltonrochabarboza@gmail.com";
        var userPassword = "ATATT3xFfGF0dNg6botaOkA5TmFYnAAysXUGoZdieFTYaFvroi7UhpMvhHiRiSeNe2sxeysFDBVJjUk6OMYJhLUhSfX8AHqeSwrM1FXHKs3eFr4oFh8ro6dGpvgxaoMmvuew2X4q3ZZIFuF5EI3vaSFCePiTFleaLleaxE3BLVwoGvdh_ZBAmcc=72DDD77B";
        var authenticationString = $"{userName}:{userPassword}";

        return Convert.ToBase64String(Encoding.ASCII.GetBytes(authenticationString));
    }

    // Get response body jira
    public string GenerateResponseJira(string url)
    {
        try
        {
            if (string.IsNullOrEmpty(url))
               throw new Exception("URL não podem ser vazia");

            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", GetAuthorization());
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