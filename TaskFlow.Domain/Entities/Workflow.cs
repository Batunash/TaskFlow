using System;
using System.Collections.Generic;
using System.Text;

namespace TaskFlow.Domain.Entities
{
    public class Workflow
    {
        public int Id { get; private set; }
        public int ProjectId { get;  set; }
       
        private readonly List<WorkflowState> _states = new();
        public IReadOnlyCollection<WorkflowState> States => _states;

        private readonly List<WorkflowTransition> _transitions = new();
        public IReadOnlyCollection<WorkflowTransition> Transitions => _transitions;

        private Workflow() { } 

        public Workflow(int projectId)
        {
            ProjectId = projectId;
        }
        public void AddState(WorkflowState state)
        {
            _states.Add(state);
        }
        public void AddTransition(WorkflowTransition transition)
        {
            _transitions.Add(transition);
        }
        public void RemoveState(WorkflowState state)
        {
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
            if (fromState.IsInitial &&_transitions.Count(t => t.FromStateId == fromState.Id) <= 1)
            {
                throw new InvalidOperationException("Initial state must have at least one outgoing transition");
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
