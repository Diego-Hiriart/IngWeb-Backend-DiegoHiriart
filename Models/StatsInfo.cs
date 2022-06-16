namespace WebAPI_DiegoHiriart.Models
{
    public class StatsInfo
    {
        public StatsInfo() 
        {
            this.lifespan = new TimeSpan();
            this.issueFree = new TimeSpan();
            this.componentIssues = new List<IssuesInfo>();
        }

        public StatsInfo(int totalReviews, TimeSpan lifespan, TimeSpan issueFree, List<IssuesInfo> componentIssues)
        {
            this.totalReviews = totalReviews;
            this.lifespan = lifespan;
            this.issueFree = issueFree;
            this.componentIssues = componentIssues;

        }

        public int totalReviews { set; get; }//Total number of reviews for the product (helps get a better idea about issue percentages)
        public TimeSpan lifespan { set; get; }
        public TimeSpan issueFree { set; get; }
        public List<IssuesInfo> componentIssues { set; get; }
    }
}
