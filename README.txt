Pré-requistos para compilação/execução

1 - Instalar VS Code com extensões para suportar C# ou qualquer outra ferramenta capaz de compilar código em C#;
2 - Adicionar pacote Newtonsoft 
3 - Instalar o .NET 6.0 Runtime
4 - Gerar um token no jira https://support.atlassian.com/atlassian-account/docs/manage-api-tokens-for-your-atlassian-account/

Instruções de uso
Ao executar o projeto, o console vai solicitar as credenciais de autorização (Basic Auth) para acesso ao Jira, portanto informe como login seu email de acesso 
e o token gerado no Jira.
A seguir será solicitada a url do jira que pode ser encontrada no próprio browser ao utilizar a ferramenta (Será algo parecido com 
https://SUA_CONTA.atlassian.net/)
Por fim informe o id do seu projeto no jira e a quantidade máxima de registros que deseja retornar.
