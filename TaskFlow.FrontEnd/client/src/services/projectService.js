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
  }
};

export default projectService;