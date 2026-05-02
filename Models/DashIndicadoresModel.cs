namespace WebApi.Models
{
    public class DashIndicadoresRow
    {
        public int     SubmittedAnswerDetailId { get; set; }
        public int     IdQuestionario          { get; set; }
        public string  TipoQuestionario        { get; set; }
        public string  Data                    { get; set; }
        public string  Supervisor              { get; set; }
        public int     IdUsuario               { get; set; }
        public string  CodVendedor             { get; set; }
        public int     IdVendedor              { get; set; }
        public string  Vendedor                { get; set; }
        public string  CodCliente              { get; set; }
        public string  Questao                 { get; set; }
        public int?    Rank                    { get; set; }
        public bool    MetricaPadrao           { get; set; }
        public bool    TipoSN                  { get; set; }
        public string  Resposta                { get; set; }
        public int     Sim                     { get; set; }
        public int     Nao                     { get; set; }
        public int     NaoRespondido           { get; set; }
        public int     Justificado             { get; set; }
        public string  Grupo                   { get; set; }
        public string  Metrica                 { get; set; }
    }
}
