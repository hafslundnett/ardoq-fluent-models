namespace ArdoqFluentModels.Mapping.ComponentHierarchy
{
    public class NamedReferenceAccessSpecification
    {
        private readonly string _name;

        public NamedReferenceAccessSpecification(string name)
        {
            _name = name;
        }

        public bool IsNamed => true;
        public bool IsChild => true;

        public string GetName()
        {
            return _name;
        }
    }
}
