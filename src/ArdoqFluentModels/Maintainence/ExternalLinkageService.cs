using System.Collections.Generic;
using System.Linq;
using Ardoq.Models;
using ArdoqFluentModels.Ardoq;
using ArdoqFluentModels.Mapping;

namespace ArdoqFluentModels.Maintainence
{
    public class ExternalLinkageService : IExternalLinkageService
    {
        private readonly IArdoqReader _reader;
        private readonly IArdoqWriter _writer;
        private readonly Dictionary<string, IArdoqSession> _sessions = new Dictionary<string, IArdoqSession>();
        private readonly Dictionary<string, Workspace> _workspaceCache = new Dictionary<string, Workspace>();

        public ExternalLinkageService(IArdoqReader reader, IArdoqWriter writer)
        {
            _reader = reader;
            _writer = writer;
        }

        public void LinkAll(
            IExternalReferenceSpecification referenceSpecification,
            IEnumerable<ParentChildRelation> relations,
            IArdoqSession sourceWorkspaceSession,
            IMaintainenceSession maintainenceSession)
        {
            foreach (var rel in relations)
            {
                Link(referenceSpecification, rel, sourceWorkspaceSession, maintainenceSession);
            }
        }

        private void Link(IExternalReferenceSpecification referenceSpecification, ParentChildRelation rel, 
            IArdoqSession sourceWorkspaceSession, IMaintainenceSession maintainenceSession)
        {
            var targetWorkspace = GetWorkspace(referenceSpecification.WorkspaceName);
            
            var sourceComponent = sourceWorkspaceSession.GetChildComponent(rel);

            var targetComponentType = referenceSpecification.TargetComponentType;
            var targetComponentKey = referenceSpecification.GetTargetComponentKey(rel.Child);

            if (targetComponentKey == null)
            {
                return;
            }

            var targetSession = GetSession(targetWorkspace, referenceSpecification);

            var targetComponent = targetSession.GetComponentsOfType(targetComponentType)
                    .Single(c => c.Name == targetComponentKey);


            var refType = sourceWorkspaceSession.GetReferenceTypeForName(referenceSpecification.ReferenceName);
            var existingReferences = sourceWorkspaceSession.GetAllSourceReferencesFromChild(rel)
                .Where(r =>
                    r.TargetWorkspace == targetWorkspace.Id
                        && r.Target == targetComponent.Id
                        && r.Type == refType);

            if (existingReferences.Any())
            {
                return;
            }

            targetSession.AddReference(refType, sourceComponent, targetComponent);
        }

        private Workspace GetWorkspace(string name)
        {
            if (_workspaceCache.ContainsKey(name))
            {
                return _workspaceCache[name];
            }

            var workspace = _reader.GetWorkspaceNamed(name).Result;
            _workspaceCache[name] = workspace;

            return workspace;
        }

        private IArdoqSession GetSession(Workspace workspace, IExternalReferenceSpecification referenceSpecification)
        {
            if (_sessions.ContainsKey(workspace.Id))
            {
                return _sessions[workspace.Id];
            }

            var session = new ArdoqSession(workspace.Id, _reader, _writer);
            _sessions[workspace.Id] = session;

            return session;
        }
    }
}
