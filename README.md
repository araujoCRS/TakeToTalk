# TakeToTalk
Projeto simplificado de um chat multi usuários utilizando .Net Core 5 e protocolos de comunicação baseados em Websockets. Este projeto não faz persistência de dados em repositórios, porém a arquitetura utilizada permite que esse requisito possa ser implementado com facilidade e com pouco impacto. Todos os dados são mantidos em memória e são perdidos ao fim da execução do servidor.

# Requisitos
  - .Net Core 5

# Estrutura
  - Api Rest - Faz papel de servidor e deve ser executada na porta 5000.
  - Site     - Faz papel de cliente e não possui robustez de interface.

## Sugestão para build/run
  - Utilize o Visual Studio 2019 como IDE.
  - Configure a execução de múltiplos projetos e marque a API e o site.
  - O site espera que a porta 5000 do servidor esteja disponível para conexão.
  - Não force casos de uso na interface que não esteja de acordo com a proposta da aplicação, pois a UI desenvolvida não realiza validações.
  - Caso precise alterar a porta de comunicação entre o Client e o Server modifique o arquivo [Index](https://github.com/araujoCRS/TakeToTalk/blob/main/TakeToTalk/TakeToTalk.Chat.UI.MVC/Views/Home/Index.cshtml) na linha 19.

