module.exports = {
    root: true,
    env: { browser: true, es2021: true, jest: true },
    extends: [
        'eslint:recommended',
        'plugin:@typescript-eslint/recommended',
        'plugin:@typescript-eslint/recommended-requiring-type-checking',
        'plugin:react-hooks/recommended',
        'plugin:react/recommended',
        'prettier',
        'plugin:prettier/recommended'
    ],
    parser: '@typescript-eslint/parser',
    parserOptions: {
        ecmaVersion: 'latest',
        sourceType: 'module',
        project: true,
        tsconfigRootDir: __dirname,
    },
    plugins: ['react-refresh', 'react', '@typescript-eslint', 'react-hooks'],
    rules: {
        'react-refresh/only-export-components': [
            'warn',
            { allowConstantExport: true },
        ],
        '@typescript-eslint/no-non-null-assertion': 'off',
        'react/jsx-filename-extension': [ 'warn', { 'extensions': [ '.tsx' ] } ],
        'max-len': [ 'warn', { 'code': 125, 'ignoreComments': true, 'ignoreUrls': true } ],
        'react-hooks/rules-of-hooks': 'error',
        'react-hooks/exhaustive-deps': 'warn',
        'react/prop-types': 'off',
        'prettier/prettier': [ 'error', { 'endOfLine': 'auto' } ],
        "react/jsx-uses-react": "off",
        "react/react-in-jsx-scope": "off"
    },
    settings: {
        "react": {
            "pragma": "React",
            "version": "detect"
        },
    }
}