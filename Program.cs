using System.Data;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace GetJiraIssues
{
    public class IssuesJira
    {
        public List<Issues> issues { get; set; } = new List<Issues>();
    }

    public class Issues
    {
        public string? id { get; set; }
        public string? key { get; set; }
        public Fields fields { get; set; } = new Fields();
        public Changelog changelog { get; set; } = new Changelog();
    }

    public class Fields
    {
        public string? summary { get; set; }
    }

    public class Changelog
    {
        public List<Histories> histories { get; set; } = new List<Histories>();

    }

    public class Histories
    {
        public string? id { get; set; }
        public string? created { get; set; }
        public Author? author { get; set; } = new Author();
        public List<Items> items { get; set; } = new List<Items>();
    }

    public class Author
    {
        public string? name { get; set; }
        public string? key { get; set; }
        public string? emailAddress { get; set; }
        public string? displayName { get; set; }
    }

    public class Items
    {
        public string? field { get; set; }
        public string? fieldtype { get; set; }
        public string? from { get; set; }
        public string? fromString { get; set; }
        public string? to { get; set; }
        public string? active { get; set; }
        public string? toString { get; set; }
    }    

    public class IssuesResult
    {
        public string? Id { get; set; }
        public string? Key { get; set; }
        public string? Summary { get; set; }
        public List<IssuesResultHistories> IssuesResultHistories { get; set; } = new List<IssuesResultHistories>();
    }

    public class IssuesResultHistories
    {
        public string? UserKey { get; set; }
        public string? UserName { get; set; }
        public string? DateChangeStatus { get; set; }
        public int CycleTime { get; set; }
        public int CycleTimeWorkDays { get; set; }
        public string? FromStatus { get; set; }
        public string? ToStatus { get; set; }
    }


    public class Program
    {
        public static void Main()
        {
            try
            {
                var fileNameDirecotry = AppDomain.CurrentDomain.BaseDirectory + "GetJiraIssue.json";

                Console.WriteLine("Digite o id do projeto: ");
                string projectId = Console.ReadLine();

                Console.WriteLine("Digite o número máximo de registro a retornar: ");
                string maxResults = Console.ReadLine();

                if (string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(maxResults))
                   throw new Exception("Projeto e/ou número máximo de registros não podem ser vazios");

                string URL = "https://nilteam.atlassian.net/rest/api/3/search?jql=" + 
                            "project=" + projectId + " and status!=Backlog" +
                            "&maxResults=" + maxResults +
                            "&expand=changelog&fields=summary,issuetype";
                        
                HttpWebRequest httpWReq = (HttpWebRequest)WebRequest.Create(URL);
                httpWReq.PreAuthenticate = true;
                httpWReq.Headers.Add("Authorization", "Basic " + getAuthorization());
                httpWReq.Headers.Add("Access-Control-Allow-Origin", "*");

                HttpWebResponse response = (HttpWebResponse)httpWReq.GetResponse();

                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("Carregando issues...");
                    Console.WriteLine();

                    StreamReader reader = new StreamReader(response.GetResponseStream());

                    var responseString = reader.ReadToEnd();
                    var issuesJira = JsonSerializer.Deserialize<IssuesJira>(responseString);

                    if (issuesJira == null || issuesJira.issues == null)
                        throw new Exception("Objects is null");

                    if (issuesJira.issues.Count == 0)
                        throw new Exception("Requisição não retornou nenhum registro.");

                    Console.WriteLine("Preparando históricos de issues...");
                    Console.WriteLine();

                    var issuesResultList = GetIssuesResult(issuesJira).Where(x => x.IssuesResultHistories.Count() > 0).OrderBy(x => x.Id);

                    var jsonSerializerOptions = new JsonSerializerOptions
                    {
                        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        WriteIndented = true,
                    };
                            
                    Console.WriteLine("Salvando arquivos json com históricos de issues...");
                    Console.WriteLine();
                    var issuesResultListJson = JsonSerializer.Serialize(issuesResultList, jsonSerializerOptions);
                    File.WriteAllText(fileNameDirecotry, issuesResultListJson);

                    Console.WriteLine("Seu arquivo foi gerado com sucesso. Digite qualquer tecla para sair!!!");
                    Console.ReadLine();
                }
                else 
                {
                    Console.WriteLine("Não foi possível executar a requisição " + response.StatusCode + "\n Pressione qualquer tecla para sair.");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
 
        }

        private static string getAuthorization()
        {
            var username = "niltonrochabarboza@gmail.com";
            var password = "ATATT3xFfGF09zpjhQewAEZNDuUt-8W9y8xcgagHqCTLbwRXnx4B2Z8TDSM50NIAHeIgDFKTz26OyjenR2rBUcsfnb-MSx9iTgZN-5PJRbmkQGJA_CzXnJ6I1tPJbmLN_Eo9W2_vK-d2HWvYa9Pf5ND3oakSL0A11bCBc-FersQojNy3SbPrDKs=43B4BD1F";
            return Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1").GetBytes(username + ":" + password));

        }         

        private static List<IssuesResult> GetIssuesResult(IssuesJira issuesJira)
        {
            var issuesResultList = new List<IssuesResult>();  

            foreach(var itemIssues in issuesJira.issues.OrderBy(x => x.id))
            {
                var issuesResult = new IssuesResult
                {
                    Id = itemIssues?.id,
                    Key = itemIssues?.key,
                    Summary = itemIssues?.fields?.summary,
                };

                if (itemIssues == null)
                    continue;


                string dateChangeStatusOld = "0";
                foreach(var itemHistories in itemIssues.changelog.histories.OrderBy(x => x.created))
                {
                    var itemsStatus = itemHistories?.items.Where(x => x.field == "status" && x.fromString != x.toString);
                    if (itemsStatus == null || itemsStatus.Count() == 0)
                        continue;

                    var dateChangeStatus = DateTime.SpecifyKind(Convert.ToDateTime(itemHistories?.created), DateTimeKind.Utc);
                    var dateFrom = (dateChangeStatusOld != "0") ? Convert.ToDateTime(dateChangeStatusOld) : DateTime.MinValue;
                    var dateTo = Convert.ToDateTime(dateChangeStatus.ToString("yyyy-MM-dd"));
            
                    var issuesResultHistories = new IssuesResultHistories
                    {
                        UserKey = itemHistories?.author?.name,
                        UserName = itemHistories?.author?.displayName,
                        DateChangeStatus = dateChangeStatus.ToString("yyyy-MM-dd"),
                        CycleTime = (dateFrom != DateTime.MinValue) ? (int)dateTo.Subtract(dateFrom).TotalDays : 0,
                        CycleTimeWorkDays = (dateFrom != DateTime.MinValue) ? GetWorkingDays(dateFrom, dateTo) : 0
                    };

                    foreach (var items in itemsStatus)
                    {
                        issuesResultHistories.FromStatus = items.fromString;
                        issuesResultHistories.ToStatus = items.toString;
                    }

                    issuesResult.IssuesResultHistories.Add(issuesResultHistories);
                        
                    dateChangeStatusOld = dateChangeStatus.ToString("yyyy-MM-dd");

                }
                issuesResultList.Add(issuesResult);
            }
            return issuesResultList;
        }

        private static int GetWorkingDays(DateTime dateFrom, DateTime dateTo)
        {
            var dayDifference = (int)dateTo.Subtract(dateFrom).TotalDays;
            return Enumerable
                    .Range(1, dayDifference)
                    .Select(x => dateFrom.AddDays(x))
                .Count(x => x.DayOfWeek != DayOfWeek.Saturday && x.DayOfWeek != DayOfWeek.Sunday);
        }

    }

}
