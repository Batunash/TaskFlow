import axiosClient from '../api/axiosClient';

const userService = {
  getMe: async () => {
    const response = await axiosClient.get('/Auth/me');
    return response.data;
  }
};

export default userService;