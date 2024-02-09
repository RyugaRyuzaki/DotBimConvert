import { nodeResolve } from "@rollup/plugin-node-resolve";
import commonjs from "@rollup/plugin-commonjs";
import typescript from "rollup-plugin-typescript2";

export default {
	input: "src/parser/index.ts",
	output: {
		file: "./resource/ConvertIfcDotBim.js",
		format: "esm",
	},
	plugins: [
		nodeResolve(),
		commonjs(),
		typescript({
			tsconfig: "tsconfig.rollup.json",
		}),
	],
};
