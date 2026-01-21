import axiosClient from '../api/axiosClient';

const taskService = {
  getAll: async (filter = {}) => {
    const params = new URLSearchParams(filter).toString();
    const response = await axiosClient.get(`/Task?${params}`);
    return response.data; 
  },

  create: async (taskData) => {
    const response = await axiosClient.post('/Task', taskData);
    return response.data;
  },

  getById: async (id) => {
    const response = await axiosClient.get(`/Task/${id}`);
    return response.data;
  }
};

export default taskService;