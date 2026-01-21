import axiosClient from '../api/axiosClient';

const workflowService = {

  create: async (projectId) => {
    const response = await axiosClient.post(`/projects/${projectId}/workflow`, {});
    return response.data;
  },
  getProjectStates: async (projectId) => {
    const response = await axiosClient.get(`/projects/${projectId}/workflow`);
    return response.data.states || []; 
  },
  addState: async (projectId, stateData) => {
    const response = await axiosClient.post(`/projects/${projectId}/workflow/states`, stateData);
    return response.data;
  },
  addTransition: async (projectId, transitionData) => {
    const response = await axiosClient.post(`/projects/${projectId}/workflow/transitions`, transitionData);
    return response.data;
  }
};

export default workflowService;