import react from '@vitejs/plugin-react';
import { defineConfig } from 'vite';

export default defineConfig(({ command }) => ({
  plugins: [react()],

  // В dev приложение работает от корня:
  // http://localhost:5173/
  //
  // В production сборка размещается в:
  // PhotoCenterWeb/Scripts/
  base: command === 'serve'
    ? '/'
    : '/Scripts/',

  build: {
    outDir: '../PhotoCenterWeb/Scripts',
    emptyOutDir: false,

    rollupOptions: {
      output: {
        entryFileNames: 'bundle.js',
        chunkFileNames: 'bundle-[name].js',
        assetFileNames: 'bundle.[ext]',
      },
    },
  },

  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:44355',
        changeOrigin: true,
        secure: false,
      },
    },
  },
}));