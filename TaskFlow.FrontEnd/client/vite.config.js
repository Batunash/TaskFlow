import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
export default defineConfig({
  plugins: [react(),tailwindcss()],
  server: {
    proxy: {
      '/api': {
        target: 'https://taskflow-api-876218411998.europe-west1.run.app', 
        changeOrigin: true,
        secure: false,
      }
    }
  }
})
