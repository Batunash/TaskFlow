import axiosClient from '../api/axiosClient';

const authService = {
  login: async (credentials) => {
    const response = await axiosClient.post('/Auth/Login', credentials);
    return response.data;
  },
  
  register: async (data) => {
    const response = await axiosClient.post('/Auth/Register', data);
    return response.data;
  }
};

export default authService;