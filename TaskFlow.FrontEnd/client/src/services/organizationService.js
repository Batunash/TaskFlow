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
  inviteUser: async (username) => {
    const response = await axiosClient.post('/Organization/invite', { username });
    return response.data;
  },
 getMembers: async (orgId) => {
  const response = await axiosClient.get(`/Organization/${orgId}/members`);
  return response.data;
},
  getInvitations: async () => {
    const response = await axiosClient.get('/Organization/invitations');
    return response.data;
  },
  acceptInvitation: async (orgId) => {
    const response = await axiosClient.post(`/Organization/accept/${orgId}`);
    return response.data;
  }
};

export default organizationService;