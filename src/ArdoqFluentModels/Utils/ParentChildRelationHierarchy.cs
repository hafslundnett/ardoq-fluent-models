using ArdoqFluentModels.Mapping;
using System.Collections.Generic;
using System.Linq;

namespace ArdoqFluentModels.Utils
{
    public class ParentChildRelationHierarchy
    {
        private readonly Dictionary<int, List<ParentChildRelation>> _levels = new Dictionary<int, List<ParentChildRelation>>();

        public ParentChildRelationHierarchy(ParentChildRelation relation)
        {
            _levels[0] = new List<ParentChildRelation>{relation};
        }

        public int LevelCount => _levels.Count;

        public List<ParentChildRelation> GetLevel(int level)
        {
            return _levels[level];
        }

        public void Expand(List<ParentChildRelation> relations)
        {
            var parent = _levels[0].First();
            foreach (var childRelation in relations.Where(r => r.Parent == parent.Child))
            {
                ExpandFrom(childRelation, 1, relations);
            }
        }

        public IEnumerable<object> GetAllChildren()
        {
            var result = new List<object>();
            for (int i = 0; i < LevelCount; i++)
            {
                result.AddRange(_levels[i].Select(r => r.Child));
            }

            return result;
        }

        public List<ParentChildRelation> GetAllParentChildRelations()
        {
            return _levels.Aggregate(
                new List<ParentChildRelation>(),
                (accum, level) =>
                {
                    accum.AddRange(level.Value);
                    return accum;
                });
        }

        private void ExpandFrom(ParentChildRelation current, int level, List<ParentChildRelation> relations)
        {
            List<ParentChildRelation> levelList;
            if (_levels.ContainsKey(level))
            {
                levelList = _levels[level];
            }
            else
            {
                levelList = new List<ParentChildRelation>();
                _levels[level] = levelList;
            }

            levelList.Add(current);

            var nextLevel = level + 1;
            foreach (var childRelation in relations.Where(r => r.Parent == current.Child))
            {
                ExpandFrom(childRelation, nextLevel, relations);
            }
        }
    }
}
