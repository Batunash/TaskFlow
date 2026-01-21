import axiosClient from '../api/axiosClient';

const projectService = {
  getAll: async () => {
    const response = await axiosClient.get('/Project');
    return response.data;
  },

  create: async (projectData) => {
    const payload = {
      name: projectData.name,
      description: projectData.description
    };
    const response = await axiosClient.post('/Project', payload);
    return response.data;
  },
  
  getById: async (id) => {
    const response = await axiosClient.get(`/Project/${id}`);
    return response.data;
  },

  update: async (id, projectData) => {
    const payload = {
      id: parseInt(id),
      name: projectData.name,
      description: projectData.description
    };
    const response = await axiosClient.put(`/Project/${id}`, payload);
    return response.data;
  },

  delete: async (id) => {
    await axiosClient.delete(`/Project/${id}`);
  },

  addMember: async (projectId, email) => {
    const payload = {
      projectId: parseInt(projectId),
      email: email
    };
    const response = await axiosClient.post(`/Project/${projectId}/members`, payload);
    return response.data;
  },

  removeMember: async (projectId, userId) => {

    const payload = {
        projectId: parseInt(projectId),
        userId: userId
    };
    await axiosClient.post(`/Project/${projectId}/members/${userId}`, payload);
  }
};

export default projectService;