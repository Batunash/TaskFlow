using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Domain.Entities
{
    public class Workflow : IAuditableEntity
    {
        public int Id { get; private set; }
        public int ProjectId { get;  set; }
       
        private readonly List<WorkflowState> _states = new();
        public IReadOnlyCollection<WorkflowState> States => _states;

        private readonly List<WorkflowTransition> _transitions = new();
        public DateTime CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public string? LastModifiedBy { get; set; }
        public IReadOnlyCollection<WorkflowTransition> Transitions => _transitions;

        private Workflow() { } 

        public Workflow(int projectId)
        {
            ProjectId = projectId;
        }
        public void AddState(WorkflowState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            if (_states.Any(s => s.Name == state.Name))
            {
                throw new InvalidOperationException($"Workflow already contains a state named '{state.Name}'.");
            }
            if (state.IsInitial && _states.Any(s => s.IsInitial))
            {
                throw new InvalidOperationException("Workflow can only have one Initial state.");
            }
            _states.Add(state);
        }
        public void AddTransition(WorkflowTransition transition)
        {
            if (transition == null) throw new ArgumentNullException(nameof(transition));

            var fromState = _states.FirstOrDefault(s => s.Id == transition.FromStateId);
            var toState = _states.FirstOrDefault(s => s.Id == transition.ToStateId);

            if (fromState == null || toState == null)
            {
                throw new InvalidOperationException("Transition states must exist in the workflow.");
            }
            if (fromState.IsFinal)
            {
                throw new InvalidOperationException("Cannot add outgoing transition from a Final state.");
            }

            bool exists = _transitions.Any(t =>
                t.FromStateId == transition.FromStateId &&
                t.ToStateId == transition.ToStateId);

            if (exists)
            {
                throw new InvalidOperationException("A transition between these states already exists.");
            }

            _transitions.Add(transition);
        }
        public void RemoveState(WorkflowState state)
        {
            if (state == null) throw new ArgumentNullException(nameof(state));

            if (state.IsInitial)
            {
                throw new InvalidOperationException("Initial state cannot be removed");
            }
            if (_transitions.Any(t =>t.FromStateId == state.Id || t.ToStateId == state.Id))
            {
                throw new InvalidOperationException("State has transitions and cannot be removed");
            }
            _states.Remove(state);
        }
        public  void RemoveTransition(WorkflowTransition transition)
        {
            if (!_transitions.Contains(transition))
            {
                throw new InvalidOperationException("Transition not found");
            }
            var fromState = _states.First(s => s.Id == transition.FromStateId);
            if (fromState != null && fromState.IsInitial)
            {
                var outgoingCount = _transitions.Count(t => t.FromStateId == fromState.Id);
                if (outgoingCount <= 1)
                {
                    throw new InvalidOperationException("Initial state must have at least one outgoing transition");
                }
            }
            _transitions.Remove(transition);
        }
        public bool CanTransition(WorkflowState from,WorkflowState to,string userRole)
        {
            return _transitions.Any(t =>
                t.FromStateId == from.Id &&
                t.ToStateId == to.Id &&
                t.IsRoleAllowed(userRole)
            );
        }
    }
}
