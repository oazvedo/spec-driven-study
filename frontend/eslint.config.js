// @ts-check
const eslint = require('@eslint/js');
const { defineConfig } = require('eslint/config');
const tseslint = require('typescript-eslint');
const angular = require('angular-eslint');
const boundaries = require('eslint-plugin-boundaries');

// Feature-Sliced Design (ADR 0009): cada camada só importa as camadas abaixo dela.
const camadas = ['app', 'pages', 'widgets', 'features', 'entities', 'shared'];
const camadasComSlices = ['pages', 'widgets', 'features', 'entities'];
const abaixoDe = (camada) => camadas.slice(camadas.indexOf(camada) + 1);

module.exports = defineConfig([
  {
    files: ['**/*.ts'],
    extends: [
      eslint.configs.recommended,
      tseslint.configs.recommended,
      tseslint.configs.stylistic,
      angular.configs.tsRecommended,
    ],
    processor: angular.processInlineTemplates,
    rules: {
      '@angular-eslint/directive-selector': [
        'error',
        {
          type: 'attribute',
          prefix: 'app',
          style: 'camelCase',
        },
      ],
      '@angular-eslint/component-selector': [
        'error',
        {
          type: 'element',
          prefix: 'app',
          style: 'kebab-case',
        },
      ],
    },
  },
  {
    files: ['src/app/**/*.ts'],
    plugins: { boundaries },
    settings: {
      // Resolve imports .ts sem extensão; sem isso o boundaries não classifica o destino e não aplica as regras
      'import/resolver': { node: { extensions: ['.ts'] } },
      'boundaries/elements': [
        { type: 'app', pattern: 'src/app/app' },
        ...camadasComSlices.map((type) => ({ type, pattern: `src/app/${type}/*`, capture: ['slice'] })),
        { type: 'shared', pattern: 'src/app/shared' },
      ],
    },
    rules: {
      'boundaries/dependencies': [
        'error',
        {
          default: 'disallow',
          policies: [
            // Imports só para camadas abaixo; uma slice não importa outra slice da mesma camada
            ...camadas.map((camada) => ({
              from: { element: { type: camada } },
              allow: { to: { element: { types: { anyOf: abaixoDe(camada) } } } },
            })),
            // De fora, uma slice só é importada pela sua API pública (index.ts)
            { disallow: { to: { element: { types: { anyOf: camadasComSlices }, fileInternalPath: '!index.ts' } } } },
          ],
        },
      ],
    },
  },
  {
    files: ['**/*.html'],
    extends: [angular.configs.templateRecommended, angular.configs.templateAccessibility],
  },
]);
