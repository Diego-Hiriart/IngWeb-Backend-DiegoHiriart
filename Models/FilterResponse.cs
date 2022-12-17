namespace WebAPI_DiegoHiriart.Models
{
    public class FilterResponse
    {
        public FilterResponse() { }

        public FilterResponse(int modelsFound, List<StatsInfo> results)
        {
            this.modelsFound = modelsFound;
            this.results = results;
        }

        public int modelsFound { set; get; }
        public List<StatsInfo> results { set; get; }
    }
}
