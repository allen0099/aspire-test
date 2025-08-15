import {defineConfig} from "vite";
import react from "@vitejs/plugin-react";
import tsconfigPaths from "vite-tsconfig-paths";
import tailwindcss from "@tailwindcss/vite";

// https://vitejs.dev/config/
export default defineConfig({
    plugins: [react(), tsconfigPaths(), tailwindcss()],
    server: {
        port: 5173,
        strictPort: true,
        proxy: {
            '/api': {
                target: process.env.services__api__http__0,
                changeOrigin: true,
                secure: false,
            }
        }
    }
});
