namespace WebAPI_DiegoHiriart.Models
{
    public class DateFixableFilterResponse
    {
        public DateFixableFilterResponse() {
            this.postsIssues = new List<PostIssue>();
        }

        public DateFixableFilterResponse(List<PostIssue> postsIssues)
        {
            this.postsIssues = postsIssues;
        }

        public List<PostIssue> postsIssues { set; get; }
    }

}
