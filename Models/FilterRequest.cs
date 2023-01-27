namespace WebAPI_DiegoHiriart.Models
{
    public class FilterRequest
    {
        public FilterRequest() { }

        public FilterRequest(int minLifeSpanYears, int minIssueFreeYears, double maxPercentIssues, double minPercentFixableIssues)
        {
            this.minLifeSpanYears = minLifeSpanYears;
            this.minIssueFreeYears = minIssueFreeYears;
            this.maxPercentIssues = maxPercentIssues;
            this.minPercentFixableIssues = minPercentFixableIssues;

        }

        public int minReviews { set; get; }//So that you can search for models with more reviews; satistically, more reviews = more trustworthy stats
        public int minLifeSpanYears { set; get; }
        public int minIssueFreeYears { set; get; }
        public double maxPercentIssues { set; get; }
        public double minPercentFixableIssues { set; get; }
    }
}
