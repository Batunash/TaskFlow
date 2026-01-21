import axiosClient from '../api/axiosClient';

const organizationService = {
  create: async (data) => {
    const response = await axiosClient.post('/Organization', data);
    return response.data;
  },
  getCurrent: async () => {
    const response = await axiosClient.get('/Organization/current');
    return response.data;
  },
  inviteUser: async (email) => {
    const response = await axiosClient.post('/Organization/invite', { email });
    return response.data;
  },
  getMembers: async (orgId) => {
    const response = await axiosClient.get(`/organizations/${orgId}/members`);
    return response.data;
  },
};

export default organizationService;