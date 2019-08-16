namespace ModelMaintainer.Mapping
{
    public class ParentChildRelation
    {
        public ParentChildRelation(object parent, object child)
        {
            Parent = parent;
            Child = child;
        }

        public object Parent { get; }
        public object Child { get; }

        public string PreexistingHierarchyReference { get; set; }
        public string ChildUniqueName { get; set; }
        public string ParentUniqueName { get; set; }
    }
}
