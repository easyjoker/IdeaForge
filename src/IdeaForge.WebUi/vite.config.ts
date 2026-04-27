import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      "/api": {
        target: process.env.VITE_IDEAFORGE_API_TARGET ?? "http://localhost:5246",
        changeOrigin: true
      }
    }
  },
  build: {
    chunkSizeWarningLimit: 1200
  }
});
