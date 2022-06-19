using Microsoft.AspNetCore.Mvc;

namespace WebAPI_DiegoHiriart.Models
{
    public class StatsInfo
    {
        public StatsInfo() 
        {
            this.componentIssues = new List<IssuesInfo>();
        }

        public StatsInfo(Model model, Brand brand, int totalReviews, double lifeSpan, double issueFree, List<IssuesInfo> componentIssues)
        {
            this.model = model;
            this.brand = brand;
            this.totalReviews = totalReviews;
            this.lifeSpan = lifeSpan;
            this.issueFree = issueFree;
            this.componentIssues = componentIssues;

        }

        public Model model { set; get; }
        public Brand brand { set; get; }
        public int totalReviews { set; get; }//Total number of reviews for the product (helps get a better idea about issue percentages)
        //Time span in days for better compatibility
        public double lifeSpan { set; get; }
        //Time span in days for better compatibility
        public double issueFree { set; get; }
        public List<IssuesInfo> componentIssues { set; get; }
    }
}
