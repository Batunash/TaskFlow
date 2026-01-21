import axiosClient from '../api/axiosClient';

const taskService = {
  getAll: async (filter = {}) => {
    const params = new URLSearchParams(filter).toString();
    const response = await axiosClient.get(`/task?${params}`);
    return response.data; 
  },
  getByProjectId: async (projectId) => {
    const response = await axiosClient.get(`/project/${projectId}/tasks`);
    return response.data;
  },
  create: async (taskData) => {
    const response = await axiosClient.post('/task', taskData);
    return response.data;
  },
  update: async (taskId, taskData) => {
    const payload = {
        id: parseInt(taskId), 
        ...taskData          
    };
    const response = await axiosClient.put('/task', payload);
    return response.data;
  },

  delete: async (taskId) => {
    const response = await axiosClient.delete(`/task/${taskId}`);
    return response.data;
  },

  assign: async (taskId, userId) => {
    const payload = {
      taskId: parseInt(taskId),
      userId: userId 
    };
    const response = await axiosClient.post('/task/assign', payload);
    return response.data;
  },
  changeStatus: async (taskId, targetStateId) => {
    const payload = {
      taskId: parseInt(taskId),
      targetStateId: parseInt(targetStateId)
    };
    const response = await axiosClient.post('/task/status', payload);
    return response.data;
  }
};

export default taskService;