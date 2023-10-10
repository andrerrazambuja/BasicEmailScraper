# Projeto Gmail API

Este projeto permite a interação com a API do Gmail para busca e leitura de e-mails. Antes de iniciar, é necessário configurar as credenciais de autenticação com a API do Google.

## Configurando `credentials.json` para a API do Google

1. Acesse o [Google Cloud Console](https://console.cloud.google.com/).
2. Selecione ou crie um projeto.
3. Vá para **Biblioteca** e procure por **Gmail API**.
4. Clique em **Ativar** para ativar a Gmail API para o seu projeto.
5. No painel de controle da Gmail API, clique em **Criar credenciais** e selecione **ID do cliente OAuth**.
6. Selecione o tipo de aplicação adequado (por exemplo, Aplicação de Desktop) e clique em **Criar**.
7. Baixe o arquivo JSON das credenciais.
8. Renomeie este arquivo para `credentials.json` e coloque-o na raiz do seu projeto.

## Modelo JSON para Requisições

```json
{
    "Assunto": "Exemplo de Assunto",
    "CorpoDoTexto": "Texto do corpo do e-mail",
    "Destinatario": "destinatario@example.com",
    "Remetente": "remetente@example.com",
    "DataInicio": "2023-10-10T00:00:00",
    "DataFim": "2023-10-11T00:00:00",
    "PossuiAnexos": true,
    "ContemPalavras": ["palavra1", "palavra2"],
    "TamanhoMinimo": 10,
    "TamanhoMaximo": 1000,
    "Tags": ["Tag1", "Tag2"]
}
```

## Como Usar

1. Garanta que o arquivo `credentials.json` está na raiz do projeto.
2. Compile e execute o projeto.
3. Faça requisições para a API usando o modelo JSON acima como base para os parâmetros desejados.
4. A resposta fornecerá os e-mails correspondentes com base nos filtros aplicados na requisição.
