﻿namespace WebAPI_DiegoHiriart.Models
{
    public class Issue
    {
        public Issue() { }

        public Issue(Int64 issueid, Int64 postid, int componentid, DateTime issuedate, bool isfixable, string description)
        {
            this.IssueId = issueid;
            this.PostId = postid;
            this.ComponentId = componentid;
            this.IssueDate = issuedate;
            this.IsFixable = isfixable;
            this.Description = description;
        }

        public Int64 IssueId { get; set; }
        public Int64 PostId { get; set; }
        public int ComponentId { get; set; }
        public DateTime IssueDate { get; set; }
        public bool IsFixable { get; set; }
        public string Description { get; set; }
    }
}
