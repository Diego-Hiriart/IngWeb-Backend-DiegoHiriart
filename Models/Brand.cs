namespace WebAPI_DiegoHiriart.Models
{
    public class Brand
    {
        public Brand() { }

        public Brand(int brandid, string name, bool isdefunct)
        {
            this.BrandId = brandid;
            this.Name = name;
            this.IsDefunct = isdefunct;
        }

        public int BrandId { get; set; }
        public string Name { get; set; }
        public bool IsDefunct { get; set; }
    }
}
