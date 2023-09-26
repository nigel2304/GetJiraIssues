﻿using System.Data;
using Newtonsoft.Json;
using static IssuesJiraModel;

namespace GetJiraIssues
{
    public class GetJiraIssues
    {
        public static void Main()
        {
            try
            {
                var fileNameDirecotry = AppDomain.CurrentDomain.BaseDirectory + "GetJiraIssue.json";

                Console.WriteLine("Insira o login: ");
                var userName = Console.ReadLine();

                Console.WriteLine("Insira o acces token: ");
                var accessToken = Console.ReadLine();

                Console.WriteLine("Digite o id do projeto: ");
                var projectId = Console.ReadLine();

                Console.WriteLine("Digite o número máximo de registro a retornar: ");
                var maxResults = Console.ReadLine();

                if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(projectId) || string.IsNullOrEmpty(maxResults))
                   throw new Exception("Login, access token, projeto e/ou número máximo de registros não podem ser vazios");

                string URL = "https://nilteam.atlassian.net/rest/api/3/search?jql=" + 
                            "project=" + projectId + " and status!=Backlog" +
                            "&maxResults=" + maxResults +
                            "&expand=changelog&fields=summary,issuetype,assignee,resolutiondate";

                Console.WriteLine("Carregando issues...");
                Console.WriteLine();

                // Get response body by jira                  
                var formartterHttpClient = new FormartterHttpClient();
                var responseString = formartterHttpClient.GenerateResponseJira(URL, userName, accessToken);
                var issuesJira = JsonConvert.DeserializeObject<IssuesJira>(responseString);

                if (issuesJira == null || issuesJira.issues == null)
                    throw new Exception("Objects is null");

                if (issuesJira.issues.Count == 0)
                    throw new Exception("Requisição não retornou nenhum registro.");

                Console.WriteLine("Preparando históricos de issues...");
                Console.WriteLine();

                var issuesResultList = new FormatterIssuesJira().GetIssuesResult(issuesJira).Where(x => x.IssuesResultHistories.Count() > 0).OrderBy(x => x.Id);

                Console.WriteLine("Salvando arquivos json com históricos de issues...");
                Console.WriteLine();
                
                var issuesResultListJson = JsonConvert.SerializeObject(issuesResultList);
                File.WriteAllText(fileNameDirecotry, issuesResultListJson);

                Console.WriteLine("Seu arquivo foi gerado com sucesso. Digite qualquer tecla para sair!!!");
                Console.ReadLine();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
            }
 
        }

    }
}
