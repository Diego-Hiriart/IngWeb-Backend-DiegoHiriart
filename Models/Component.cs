namespace WebAPI_DiegoHiriart.Models
{
    public class Component
    {
        public Component() { }

        public Component(int componentId, string name, string description)
        {
            this.ComponentId = componentId;
            this.Name = name;
            this.Description = description;
        }

        public int ComponentId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }                                   
}
