namespace WebAPI_DiegoHiriart.Models
{
    public class Issues
    {
        public Issues() { }

        public Issues(UInt64 issueid, UInt64 postid, int componentid, DateTime issuedate, bool isfixable, string description)
        {
            this.IssueId = issueid;
            this.PostId = postid;
            this.ComponentId = componentid;
            this.IssueDate = issuedate;
            this.IsFixable = isfixable;
            this.Description = description;
        }

        public UInt64 IssueId { get; set; }
        public UInt64 PostId { get; set; }
        public int ComponentId { get; set; }
        public DateTime IssueDate { get; set; }
        public bool IsFixable { get; set; }
        public string Description { get; set; }
    }
}
