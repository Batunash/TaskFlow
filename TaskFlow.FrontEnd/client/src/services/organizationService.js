import axiosClient from '../api/axiosClient';
const organizationService = {
  create: async (data) => {
    const response = await axiosClient.post('/Organization', data);
    return response.data;
  },
  getById: async (id) => {
    const response = await axiosClient.get(`/Organization/${id}`);
    return response.data;
  }
};

export default organizationService;