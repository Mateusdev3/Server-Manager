


# Server Manager

O **Server Manager** é um aplicativo em **C# WPF** desenvolvido para gerenciar servidores do jogo **DayZ**, integrando controle de processos, gerenciamento de whitelist, execução automática de mods e monitoramento via servidor HTTP com integração a banco de dados **SQLite**.

---

## Funcionalidades Principais

- Controle de execução do **DayZ** e do **B.E.C. (BattleEye Client)**.
- Monitoramento e reinício automático de processos.
- Suporte a **mods** automatizado.
- Habilitação e desabilitação de **whitelist** no servidor.
- Servidor HTTP integrado para gerenciar whitelist e consultar status de jogadores.
- Registro e gerenciamento de informações de jogadores no **SQLite**.

---

## Estrutura do Projeto

- **MainWindow.xaml.cs**: Controla a interface principal e gerencia loops de monitoramento.  
- **ManagerConfig.json**: Arquivo de configuração com paths, nomes de arquivos, porta do servidor HTTP e banco de dados.  
- **ServerHttp**: Classe responsável pelo servidor HTTP, manipulação da whitelist e conexão com o banco de dados.  
- **PlayerInfo**: Estrutura de dados para armazenar informações de cada jogador (NICK, HWID, RAMID, COMPUTERID, CPUID, GPUID, BOARDID).  
- **Config**: Estrutura para mapear as configurações do JSON.

---

## Configurações

Exemplo de **ManagerConfig.json**:

```json
{
  "Diretoriomain": "C:\\DayZServer",
  "Dayzexename": "DayZServer_x64.exe",
  "Dayzcfgname": "serverDZ.cfg",
  "Becname": "BEC.exe",
  "Processbec": "BEC",
  "Becpath": "C:\\DayZServer\\BEC",
  "Nomedayzprocess": "DayZServer_x64",
  "Port": "2302",
  "Whitelistpath": "C:\\DayZServer\\whitelist.txt",
  "Configdayzpath": "C:\\DayZServer\\serverDZ.cfg",
  "Dbpath": "C:\\DayZServer\\Players.db",
  "Dbsource": "SQLite"
}
````

---

## Como Funciona

* Ao iniciar, o aplicativo verifica se já existe uma instância em execução. Caso exista, encerra.
* Lê o arquivo **ManagerConfig.json** e carrega as variáveis.
* Inicia loops para:

  * Verificar e reiniciar automaticamente o **DayZ** e o **BEC**.
  * Atualizar **mods** e configurações.
* Permite iniciar e parar manualmente o servidor pelo botão **Iniciar** e **Fechar**.
* Modifica o arquivo de configuração do DayZ para habilitar/desabilitar **whitelist** com base no checkbox correspondente.

### Rotas do Servidor HTTP

* **GET /add/{steamID}**: Adiciona um Steam ID à whitelist.
* **GET /remove/{steamID}**: Remove um Steam ID da whitelist.
* **POST /status**: Recebe JSON com informações de jogador e retorna status (**ATIVO** ou **BANIDO**).

---

## Estrutura de Dados de Jogador

* **NICK**: Nome do jogador.
* **HWID**: ID da memória RAM.
* **RAMID**: ID específico da RAM.
* **COMPUTERID**: ID do computador.
* **CPUID**: ID do processador.
* **GPUID**: ID da GPU.
* **BOARDID**: ID da placa-mãe.

---

## Funcionalidades do Servidor HTTP

* Gerenciamento da **whitelist**: Adição e remoção de IDs.
* Consulta de **status do jogador**: Verifica se jogador está ativo ou banido com base no SQLite.
* Conexão e manipulação do **banco de dados SQLite**.
* Suporte a múltiplas requisições simultâneas.

---

## Observações

* O programa depende de arquivos externos como **ManagerConfig.json**, **serverDZ.cfg** e banco de dados **SQLite**.
* O loop de monitoramento do DayZ ocorre a cada **15 segundos**.
* O loop do **B.E.C.** ocorre a cada **7 minutos**.
* Todos os processos são iniciados com verificação de existência para evitar múltiplas instâncias.
* A aplicação oferece **feedback via MessageBox** em caso de erro ou ausência de arquivos.

---

## Dependências

* **.NET Framework** (versão compatível com WPF)
* **SQLite**
* Sistema operacional **Windows**

```

