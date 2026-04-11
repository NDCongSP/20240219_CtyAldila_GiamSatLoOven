namespace GiamSat.UI.Model
{
    public class OvenSystemModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsExpanded { get; set; }
    }

    public class OvenSystemList : List<OvenSystemModel>
    {
    }
}
