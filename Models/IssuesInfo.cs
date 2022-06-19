namespace WebAPI_DiegoHiriart.Models
{
    public class IssuesInfo
    {
        public IssuesInfo()
        {
            this.component = new Component();
        }

        public IssuesInfo(Component component, double percentIssues, double percentFixable)
        {
            this.component = component;
            this.percentIssues = percentIssues;
            this.percentFixable = percentFixable;
        }

        public Component component { set; get; }
        public double percentIssues { set; get; }//Percentage of reviews (owners) with issues in that component
        public double percentFixable { set; get; }//Percentage of issues for the component that are fixable 
    }
}
