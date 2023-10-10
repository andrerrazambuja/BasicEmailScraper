namespace TesteConceitoEmailScraper.Models
{
    public class EmailFilters
    {
        public string Assunto { get; set; } = "";
        public string CorpoDoTexto { get; set; } = "";
        public string Destinatario { get; set; } = "";
        public string Remetente { get; set; } = "";
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool? PossuiAnexos { get; set; }
        public List<string> ContemPalavras { get; set; } = new List<string>();
        public int? TamanhoMinimo { get; set; }
        public int? TamanhoMaximo { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }
}
