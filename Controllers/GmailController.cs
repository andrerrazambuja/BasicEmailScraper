using Microsoft.AspNetCore.Mvc;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Gmail.v1;
using Google.Apis.Services;
using System.IO;
using TesteConceitoEmailScraper.Models;
using System.Text;
using TesteConceitoEmailScraper.Models.TesteConceitoEmailScraper.Models;
using System.Globalization;

namespace TesteConceitoEmailScraper.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class GmailController : ControllerBase
    {
        private readonly GmailService _gmailService;
        private readonly GmailSettings _gmailSettings;

        public GmailController(GmailSettings gmailSettings)
        {
            _gmailSettings = gmailSettings;
            UserCredential credential;

            using (var stream = new FileStream("credentials.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = "token.json";

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { GmailService.Scope.GmailReadonly },
                    "user",
                    System.Threading.CancellationToken.None,
                    new Google.Apis.Util.Store.FileDataStore(credPath, true)).Result;
            }

            _gmailService = new GmailService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "TesteConceitoEmailScraper",
            });
        }

        [HttpPost("search-emails")]
        public IActionResult SearchEmails([FromBody] EmailFilters filters)
        {
            string query = BuildGmailQuery(filters);

            var request = _gmailService.Users.Messages.List("me");
            request.Q = query;
            var response = request.Execute();

            if (response != null && response.Messages != null)
            {
                var simpleEmails = new List<SimpleEmailData>();
                foreach (var msg in response.Messages.Take(_gmailSettings.MaxEmailsToRetrieve)) // Limite de emails que uma req pode retornar, ajustavel no appsettings
                {
                    var emailDetail = _gmailService.Users.Messages.Get("me", msg.Id).Execute();
                    var simpleEmail = ConvertToSimpleEmail(emailDetail);
                    simpleEmails.Add(simpleEmail);
                }

                return Ok(simpleEmails);
            }
            else
            {
                return NoContent();
            }
        }

        private string BuildGmailQuery(EmailFilters filters)
        {
            List<string> queries = new List<string>();

            if (!string.IsNullOrEmpty(filters.CorpoDoTexto))
                queries.Add(filters.CorpoDoTexto);

            if (!string.IsNullOrEmpty(filters.Assunto))
                queries.Add($"subject:{filters.Assunto}");

            if (!string.IsNullOrEmpty(filters.Destinatario))
                queries.Add($"to:{filters.Destinatario}");

            if (!string.IsNullOrEmpty(filters.Remetente))
                queries.Add($"from:{filters.Remetente}");

            if (filters.DataInicio.HasValue)
                queries.Add($"after:{filters.DataInicio.Value.ToString("yyyy-MM-dd")}");

            if (filters.DataFim.HasValue)
                queries.Add($"before:{filters.DataFim.Value.ToString("yyyy-MM-dd")}");

            if (filters.PossuiAnexos.HasValue && filters.PossuiAnexos.Value)
                queries.Add($"has:attachment");

            if (filters.ContemPalavras.Any())
                queries.AddRange(filters.ContemPalavras.Select(word => $"\"{word}\""));

            if (filters.TamanhoMinimo.HasValue)
                queries.Add($"larger:{filters.TamanhoMinimo.Value}");

            if (filters.TamanhoMaximo.HasValue)
                queries.Add($"smaller:{filters.TamanhoMaximo.Value}");

            if (filters.Tags.Any())
                queries.AddRange(filters.Tags.Select(tag => $"label:{tag}"));

            return string.Join(" ", queries);
        }


        private SimpleEmailData ConvertToSimpleEmail(Google.Apis.Gmail.v1.Data.Message message)
        {
            var email = new SimpleEmailData();

            ParseHeaders(message.Payload.Headers, email);
            SetReceivedDate(message, email);
            SetEmailBody(message, email);

            return email;
        }

        private void ParseHeaders(IList<Google.Apis.Gmail.v1.Data.MessagePartHeader> headers, SimpleEmailData email)
        {
            if (headers == null) return;

            foreach (var header in headers)
            {
                switch (header.Name)
                {
                    case "From":
                        email.From = header.Value;
                        break;
                    case "To":
                        email.To = header.Value;
                        break;
                    case "Subject":
                        email.Subject = header.Value;
                        break;
                }
            }
        }

        private void SetReceivedDate(Google.Apis.Gmail.v1.Data.Message message, SimpleEmailData email)
        {
            if (message.InternalDate.HasValue)
            {
                email.ReceivedDate = DateTimeOffset.FromUnixTimeMilliseconds((long)message.InternalDate).DateTime;
            }
        }

        private void SetEmailBody(Google.Apis.Gmail.v1.Data.Message message, SimpleEmailData email)
        {
            if (message.Payload.Parts != null && message.Payload.Parts.Any(p => p.MimeType == "text/plain"))
            {
                var part = message.Payload.Parts.First(p => p.MimeType == "text/plain");
                email.Body = DecodeBase64String(part.Body.Data);
            }
            else if (message.Payload.Body != null && !string.IsNullOrEmpty(message.Payload.Body.Data))
            {
                email.Body = DecodeBase64String(message.Payload.Body.Data);
            }
        }

        private string DecodeBase64String(string base64)
        {
            string standardBase64 = base64.Replace('-', '+').Replace('_', '/');
            int mod4 = standardBase64.Length % 4;
            if (mod4 > 0)
            {
                standardBase64 += new string('=', 4 - mod4);
            }

            byte[] data = Convert.FromBase64String(standardBase64);
            return Encoding.UTF8.GetString(data);
        }


    }
}
