import angularPlugin from '@angular-eslint/eslint-plugin';
import angularTemplatePlugin from '@angular-eslint/eslint-plugin-template';
import angularTemplateParser from '@angular-eslint/template-parser';
import eslint from '@eslint/js';
import { defineConfig } from 'eslint/config';
import tseslint from 'typescript-eslint';

export default defineConfig(
  {
    ignores: ['dist/', '.angular/', 'node_modules/']
  },
  {
    files: ['**/*.ts'],
    extends: [eslint.configs.recommended, tseslint.configs.recommended],
    plugins: {
      '@angular-eslint': angularPlugin
    },
    rules: {
      ...angularPlugin.configs.recommended.rules,

      // TypeScript already enforces undefined variable checks
      'no-undef': 'off',
      // Base rule must be off; @typescript-eslint version handles TS generics correctly
      'no-unused-vars': 'off',
      '@typescript-eslint/no-unused-vars': [
        'error',
        {
          argsIgnorePattern: '^_',
          varsIgnorePattern: '^_',
          caughtErrorsIgnorePattern: '^_'
        }
      ],

      // Require curly braces for all control-flow bodies — prevents subtle bugs
      curly: 'error',

      // Enforce Angular naming conventions
      '@angular-eslint/directive-selector': [
        'error',
        { type: 'attribute', prefix: 'app', style: 'camelCase' }
      ],
      '@angular-eslint/component-selector': [
        'error',
        { type: 'element', prefix: 'app', style: 'kebab-case' }
      ]
    }
  },
  {
    files: ['**/*.spec.ts'],
    languageOptions: {
      parserOptions: {
        project: ['./tsconfig.spec.json'],
        tsconfigRootDir: import.meta.dirname
      }
    }
  },
  {
    // The template plugin ships its recommended/accessibility configs in the old
    // eslintrc-style flat-config shape (parser as a string at the root level).
    // ESLint 9 rejects that in extends, so we set the parser explicitly and
    // spread the rule sets manually instead.
    files: ['**/*.html'],
    languageOptions: {
      parser: angularTemplateParser
    },
    plugins: {
      '@angular-eslint/template': angularTemplatePlugin
    },
    rules: {
      ...angularTemplatePlugin.configs.recommended.rules,
      ...angularTemplatePlugin.configs.accessibility.rules,
      // Rule requires interactive elements to have focus management — too strict
      // for placeholder screens; re-evaluate when real UI is built
      '@angular-eslint/template/interactive-supports-focus': 'off'
    }
  }
);
