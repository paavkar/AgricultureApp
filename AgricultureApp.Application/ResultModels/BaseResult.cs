namespace AgricultureApp.Application.ResultModels
{
    public class BaseResult
    {
        public bool Succeeded { get; set; }
        public int? StatusCode { get; set; }
        public IEnumerable<string>? Errors { get; set; }
    }
}
