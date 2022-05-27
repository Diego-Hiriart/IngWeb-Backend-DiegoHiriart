namespace WebAPI_DiegoHiriart.Models
{
    public class Post
    {
        public Post() { }

        public Post(Int64 postId, Int64 userid, Int64 modelid, DateTime postdate, DateTime purchase, DateTime? firstissues, DateTime? innoperative, string review)
        {
            this.PostId = postId;
            this.UserId = userid;
            this.ModelId = modelid;
            this.PostDate = postdate;
            this.Purchase = purchase;
            this.FirstIssues = firstissues;
            this.Innoperative = innoperative;
            this.Review = review;
        }

        public Int64 PostId { get; set; }
        public Int64 UserId { get; set; }
        public Int64 ModelId { get; set; }
        public DateTime PostDate { get; set; }
        public DateTime Purchase { get; set; }
        public DateTime? FirstIssues { get; set; }
        public DateTime? Innoperative { get; set; }
        public string Review { get; set; }
    }
}
