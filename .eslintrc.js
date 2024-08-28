module.exports = {
  root: true,
  env: {
    es2022: true,
    node: true,
    jest: true,
  },
  parserOptions: {
    ecmaVersion: 13,
    sourceType: 'module',
    project: ['./tsconfig.json'],
    tsconfigRootDir: __dirname,
  },
  plugins: ['@typescript-eslint'],
  extends: ['eslint:recommended', 'plugin:@typescript-eslint/recommended', 'prettier'],
  overrides: [],
  ignorePatterns: ['.eslintrc'],
  rules: {
    'no-console': 'warn',
    '@typescript-eslint/no-unused-vars': 'error',
    '@typescript-eslint/no-empty-interface': 'error',
    '@typescript-eslint/no-explicit-any': 'warn',
    '@typescript-eslint/no-inferrable-types': 'warn',
    '@typescript-eslint/no-floating-promises': 'error',
    '@typescript-eslint/consistent-type-definitions': ['warn', 'type'],
    '@typescript-eslint/explicit-function-return-type': 'warn',
    '@typescript-eslint/explicit-module-boundary-types': 'warn',
    'prefer-const': [
      'error',
      {
        destructuring: 'all',
      },
    ],
  },
};
