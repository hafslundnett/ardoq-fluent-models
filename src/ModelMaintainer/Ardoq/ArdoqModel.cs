using System.Collections.Generic;

namespace ArdoqFluentModels.Ardoq
{
    public class ArdoqModel : IArdoqModel
    {
        private readonly global::Ardoq.Models.Model _internalModel;

        public ArdoqModel(global::Ardoq.Models.Model model)
        {
            _internalModel = model;
        }

        public Dictionary<string, string> ComponentTypes
        {
            get => _internalModel.ComponentTypes;
            set => _internalModel.ComponentTypes = value;
        }

        public string Name
        {
            get => _internalModel.Name;
            set => _internalModel.Name = value;
        }
        public string Id
        {
            get => _internalModel.Id;
            set => _internalModel.Id = value;
        }

        public string GetComponentTypeByName(string name)
        {
            return _internalModel.GetComponentTypeByName(name);
        }

        public int GetReferenceTypeByName(string name)
        {
            return _internalModel.GetReferenceTypeByName(name);
        }
    }
}
