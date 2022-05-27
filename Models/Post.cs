namespace WebAPI_DiegoHiriart.Models
{
    public class Post
    {
        public Post() { }

        public Post(UInt64 postId, UInt64 userid, UInt64 modelid, DateTime postdate, DateTime purchase, DateTime firstissues, DateTime innoperative, string review)
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

        public UInt64 PostId { get; set; }
        public UInt64 UserId { get; set; }
        public UInt64 ModelId { get; set; }
        public DateTime PostDate { get; set; }
        public DateTime Purchase { get; set; }
        public DateTime FirstIssues { get; set; }
        public DateTime Innoperative { get; set; }
        public string Review { get; set; }
    }
}
