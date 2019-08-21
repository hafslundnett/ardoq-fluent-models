namespace ModelMaintainer.Tests.Model
{
    public class Role
    {
        public string Name { get; set; }

        public string GetNameOfIndustry()
        {
            return !string.IsNullOrWhiteSpace(Name) && Name.ToLower().Contains("sales")
                ? "Sales and marketing"
                : null;
        }
    }
}
