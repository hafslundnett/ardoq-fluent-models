using System.Collections.Generic;
using ArdoqFluentModels.Ardoq;
using ArdoqFluentModels.Mapping;

namespace ArdoqFluentModels.Maintainence
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
