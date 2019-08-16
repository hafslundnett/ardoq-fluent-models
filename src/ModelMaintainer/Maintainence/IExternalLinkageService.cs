using System.Collections.Generic;
using ModelMaintainer.Ardoq;
using ModelMaintainer.Mapping;

namespace ModelMaintainer.Maintainence
{
    public interface IExternalLinkageService
    {
        void LinkAll(
            IExternalReferenceSpecification referenceSpecification,
            IEnumerable<ParentChildRelation> relations,
            IArdoqSession sourceWorkspaceSession,
            IMaintainenceSession maintainenceSession);
    }
}
