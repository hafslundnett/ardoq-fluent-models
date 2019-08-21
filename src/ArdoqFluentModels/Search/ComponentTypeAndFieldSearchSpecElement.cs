using System;
using System.Collections.Generic;
using System.Linq;
using Ardoq.Models;

namespace ArdoqFluentModels.Search
{
    public class ComponentTypeAndFieldSearchSpecElement : SearchSpecElementBase
    {
        private readonly Dictionary<string, object> _fieldFilters = new Dictionary<string, object>();

        public string ComponentType { get; set; }
        public string Name { get; set; }
        public List<string> NameList { get; set; }
        public IDictionary<string, object> FieldFilters => _fieldFilters;

        public List<string> ParentNameList { get; set; }

        protected override IEnumerable<Component> SearchCore(IEnumerable<Tag> allTags, IEnumerable<Component> allComponents)
        {
            if (ComponentType == null && Name == null && NameList == null && ParentNameList == null && !_fieldFilters.Any())
            {
                return new List<Component>();
            }

            var allComponentsList = allComponents.ToList();

            var filteredForType = ComponentType == null
                ? allComponentsList
                : allComponentsList.Where(c => c.Type == ComponentType).ToList();

            var filteredForName = string.IsNullOrWhiteSpace(Name)
                ? filteredForType
                : filteredForType.Where(c => c.Name == Name).ToList();

            var filteredForNameList = NameList == null
                ? filteredForName
                : filteredForName.Where(c => NameList.Contains(c.Name)).ToList();

            var filteredForParentName = ParentNameList == null
                ? filteredForNameList
                : filteredForNameList.Where(c => FilterForParentNames(allComponentsList, c)).ToList();

            if (!_fieldFilters.Any())
            {
                return filteredForParentName;
            }

            return filteredForParentName.Where(comp => _fieldFilters.All(pair => comp.Fields.ContainsKey(pair.Key) && AreEqual(comp.Fields[pair.Key], pair.Value)));
        }

        private bool FilterForParentNames(List<Component> allComponentsList, Component c)
        {
            Component parentComponent = ParentComponent(allComponentsList, c);
            return parentComponent == null || parentComponent.Name == null 
                ? false 
                : ParentNameList.Contains(parentComponent.Name);
        }

        private Component ParentComponent(List<Component> allComponentsList, Component component)
        {
            var parentId = component.Parent;
            return allComponentsList.FirstOrDefault(comp => comp.Id == parentId);
        }

        private bool AreEqual(object compField, object pairValue)
        {
            if (compField == null || compField.GetType() != pairValue.GetType())
            {
                return false;
            }

            switch (compField)
            {
                case string s:
                    return s == (string) pairValue;
                case DateTime time:
                    return time == (DateTime) pairValue;
                case int i:
                    return i == (int) pairValue;
                case long l :
                    return l == (long) pairValue;
            }

            return false;
        }

        public void AddFieldFilter(string fieldName, object value)
        {
            _fieldFilters.Add(fieldName, value);
        }
    }
}
