namespace WebAPI_DiegoHiriart.Models
{
    public class DateFixableFilterRequest
    {
        public DateFixableFilterRequest() { }

        public DateFixableFilterRequest(DateTime startDate, DateTime endDate, bool showFixables)
        {
            this.startDate = startDate;
            this.endDate = endDate;
            this.showFixables = showFixables;
        }

        public DateTime startDate { set; get; }
        public DateTime endDate { set; get; }
        public bool showFixables { set; get; }
    }
}
