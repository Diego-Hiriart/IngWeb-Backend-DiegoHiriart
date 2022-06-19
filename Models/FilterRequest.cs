namespace WebAPI_DiegoHiriart.Models
{
    public class FilterRequest
    {
        public FilterRequest() { }

        public FilterRequest(int lifeSpanYears, int issueFreeYears, int maxIssuesAnyComponent)
        {
            this.lifeSpanYears = lifeSpanYears;
            this.issueFreeYears = issueFreeYears;
            this.maxIssuesAnyComponent = maxIssuesAnyComponent;
        }

        public int lifeSpanYears { set; get; }
        public int issueFreeYears { set; get; }
        public int maxIssuesAnyComponent { set; get; }
    }
}
