import react from '@vitejs/plugin-react'
import path from 'node:path'
import type { UserConfig as VitestUserConfig } from 'vitest/config'

const config: VitestUserConfig = {
  plugins: [react()] as unknown as VitestUserConfig['plugins'],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: './src/setupTests.ts',
    css: false,
  },
}

// https://vite.dev/config/
export default config
