namespace WebAPI_DiegoHiriart.Models
{
    public class Model
    {
        public Model() { }
        public Model(Int64 modelid, int brandid, string modelnumber, string name, DateTime launch, bool discontinued)
        {
            this.ModelId = modelid;
            this.BrandId = brandid;
            this.ModelNumber = modelnumber;
            this.Name = name;
            this.Launch = launch;
            this.Discontinued = discontinued;
        }

        public Int64 ModelId { get; set; }
        public int BrandId { get; set; }
        public string ModelNumber { get; set; }
        public string Name { get; set; }
        public DateTime Launch { get; set; }
        public bool Discontinued { get; set; }
    }
}
