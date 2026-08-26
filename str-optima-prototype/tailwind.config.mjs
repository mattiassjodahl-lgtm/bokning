/** @type {import('tailwindcss').Config} */
export default {
	content: ['./src/**/*.{astro,html,js,jsx,md,mdx,svelte,ts,tsx,vue}'],
	theme: {
		extend: {
			colors: {
				// Brand teal, sampled from the STR/Optima concept mockups.
				brand: {
					50: '#eef6f5',
					100: '#d3e7e5',
					200: '#a7cfcb',
					300: '#7ab6b0',
					400: '#4c9d96',
					500: '#33827c',
					600: '#276863',
					700: '#1f5551', // primary top bar / sidebar accent
					800: '#194542',
					900: '#123634',
				},
				today: {
					50: '#fdf3ea',
					100: '#f9e4cd',
				},
			},
			fontFamily: {
				sans: ['"Inter"', 'ui-sans-serif', 'system-ui', 'sans-serif'],
			},
		},
	},
	plugins: [],
};
