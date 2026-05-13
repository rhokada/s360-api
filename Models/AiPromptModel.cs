namespace WebApi.Models
{
    public class AiPromptCreateModel
    {
        public string AiProcessCd { get; set; }
        public string Context     { get; set; }
        public string Prompt      { get; set; }
        public string Engine      { get; set; }
        public string Log         { get; set; }
    }

    public class AiPromptUpdateModel
    {
        public int    AiPromptId  { get; set; }
        public string AiProcessCd { get; set; }
        public string Context     { get; set; }
        public string Prompt      { get; set; }
        public string Engine      { get; set; }
        public string Log         { get; set; }
    }
}
