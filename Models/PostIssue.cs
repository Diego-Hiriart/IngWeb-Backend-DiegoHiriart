namespace WebAPI_DiegoHiriart.Models
{
    public class PostIssue
    {
        public PostIssue(){
            this.issues = new List<Issue>();
        }

        public PostIssue(Post post, List<Issue> issues)
        {
            this.post = post;
            this.issues = issues;
        }

        public Post post { set; get; }
        public List<Issue> issues { set; get; }
        
    }
}
