using System.Collections.Generic;
using Ardoq.Models;
using ArdoqFluentModels.Mapping;

namespace ArdoqFluentModels.Ardoq
{
    public interface IArdoqSession
    {
        IEnumerable<Component> GetAllComponents();
        IEnumerable<Component> GetComponentsOfType(string typeName);
        IEnumerable<Component> GetComponents(string typeName, string name); 

        void AddComponent(
            string name,
            IDictionary<string, object> values,
            string componentType,
            string parentComponentName);

        void AddComponentWithParent(
            string name,
            IDictionary<string, object> values,
            string componentType,
            Component parentComponent);

        void UpdateComponent(Component component);

        void DeleteComponent(Component component);

        Reference GetReference(
            string referenceType,
            string sourceComponentType,
            string sourceKey,
            string targetComponentType,
            string targetKey);

        void AddReference(
            int referenceType,
            string sourceComponentType,
            string sourceKey,
            string targetComponentType,
            string targetKey);

        void AddReference(
            int refType,
            ParentChildRelation source,
            ParentChildRelation target);

        void AddReference(
            int referenceType,
            Component sourceComponent,
            Component targetComponent);

        void AddReference(
            int referenceType,
            string sourceComponentId,
            string targetComponentId);
        IEnumerable<Reference> GetAllSourceReferencesFromChild(ParentChildRelation relation);
        int GetReferenceTypeForName(string refernceName);

        void DeleteReference(Reference reference);

        IEnumerable<Tag> GetAllTags();

        Tag CreateTag(string tag, Component component);
        Tag AddComponentToTag(Tag tag, Component component);
        Component GetParentComponent(ParentChildRelation relation);
        Component GetChildComponent(ParentChildRelation relation);
    }
}
